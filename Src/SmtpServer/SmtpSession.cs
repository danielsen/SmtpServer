﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using SmtpServer.Extensions;
using SmtpServer.IO;
using SmtpServer.Protocol;
using SmtpServer.Text;

namespace SmtpServer
{
    internal sealed class SmtpSession
    {
        readonly SmtpStateMachine _stateMachine;
        readonly SmtpSessionContext _context;
        TaskCompletionSource<bool> _taskCompletionSource;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="context">The session context.</param>
        internal SmtpSession(SmtpSessionContext context)
        {
            _context = context;
            _stateMachine = new SmtpStateMachine(_context);
        }

        /// <summary>
        /// Executes the session.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        public void Run(CancellationToken cancellationToken)
        {
            _taskCompletionSource = new TaskCompletionSource<bool>();

            RunAsync(cancellationToken)
                .ContinueWith(t =>
                {
                    try
                    {
                        _taskCompletionSource.SetResult(t.IsCompleted);
                    }
                    catch
                    {
                        _taskCompletionSource.SetResult(false);
                    }
                }, 
                cancellationToken);
        }

        /// <summary>
        /// Handles the SMTP session.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task which performs the operation.</returns>
        async Task RunAsync(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            await ExecuteAsync(_context, cancellationToken).ReturnOnAnyThread();
        }

        /// <summary>
        /// Execute the command handler against the specified session context.
        /// </summary>
        /// <param name="context">The session context to execute the command handler against.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task which asynchronously performs the execution.</returns>
        async Task ExecuteAsync(SmtpSessionContext context, CancellationToken cancellationToken)
        {
            // The PROXY protocol requires that the receiver must wait for the 
            // proxy command to be fully received before it starts processing the
            // session. Since the receiver is expected to speak first in SMTP,
            // i.e. sending the greeting on connect, we wait for the proxy
            // command to be consumed and processed before speaking to the 
            // remote client.
            if (!_context.ServerOptions.Proxy)
            {
                await OutputGreetingAsync(cancellationToken).ReturnOnAnyThread();
            }

            if (_context.ServerOptions.Proxy)
            {
                await IngestProxyAsync(context, cancellationToken).ReturnOnAnyThread();
            }

            var retries = _context.ServerOptions.MaxRetryCount;

            while (retries-- > 0 && context.IsQuitRequested == false && cancellationToken.IsCancellationRequested == false)
            {
                var text = await ReadCommandInputAsync(context, cancellationToken);

                if (text == null)
                {
                    return;
                }

                if (TryMake(context, text, out var command, out var response))
                {
                    try
                    {
                        if (await ExecuteAsync(command, context, cancellationToken).ReturnOnAnyThread())
                        {
                            _stateMachine.Transition(context);
                        }

                        retries = _context.ServerOptions.MaxRetryCount;

                        continue;
                    }
                    catch (SmtpResponseException responseException)
                    {
                        context.IsQuitRequested = responseException.IsQuitRequested;

                        response = responseException.Response;
                    }
                    catch (OperationCanceledException)
                    {
                        await context.NetworkClient.ReplyAsync(new SmtpResponse(SmtpReplyCode.ServiceClosingTransmissionChannel, "The session has be cancelled."), cancellationToken);
                        return;
                    }
                }

                await context.NetworkClient.ReplyAsync(CreateErrorResponse(response, retries), cancellationToken);
            }
        }

        /// <summary>
        /// Read the command input.
        /// </summary>
        /// <param name="context">The session context to execute the command handler against.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The input that was received from the client.</returns>
        async Task<IReadOnlyList<ArraySegment<byte>>> ReadCommandInputAsync(SmtpSessionContext context, CancellationToken cancellationToken)
        {
            var timeout = new CancellationTokenSource(_context.ServerOptions.CommandWaitTimeout);

            var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(timeout.Token, cancellationToken);
           
            try
            {
                return await context.NetworkClient.ReadLineAsync(cancellationTokenSource.Token).ReturnOnAnyThread();
            }
            catch (OperationCanceledException)
            {
                if (timeout.IsCancellationRequested)
                {
                    await context.NetworkClient.ReplyAsync(new SmtpResponse(SmtpReplyCode.ServiceClosingTransmissionChannel, "Timeout whilst waiting for input."), cancellationToken);
                    return null;
                }

                await context.NetworkClient.ReplyAsync(new SmtpResponse(SmtpReplyCode.ServiceClosingTransmissionChannel, "The session has be cancelled."), CancellationToken.None);
                return null;
            }
            finally
            {
                timeout.Dispose();
                cancellationTokenSource.Dispose();
            }
        }

        /// <summary>
        /// Create an error response.
        /// </summary>
        /// <param name="response">The original response to wrap with the error message information.</param>
        /// <param name="retries">The number of retries remaining before the session is terminated.</param>
        /// <returns>The response that wraps the original response with the additional error information.</returns>
        static SmtpResponse CreateErrorResponse(SmtpResponse response, int retries)
        {
            return new SmtpResponse(response.ReplyCode, $"{response.Message}, {retries} retry(ies) remaining.");
        }

        /// <summary>
        /// Advances the enumerator to the next command in the stream.
        /// </summary>
        /// <param name="context">The session context to use when making session based transitions.</param>
        /// <param name="segments">The list of array segments to read the command from.</param>
        /// <param name="command">The command that was found.</param>
        /// <param name="errorResponse">The error response that indicates why a command could not be accepted.</param>
        /// <returns>true if a valid command was found, false if not.</returns>
        bool TryMake(SmtpSessionContext context, IReadOnlyList<ArraySegment<byte>> segments, out SmtpCommand command, out SmtpResponse errorResponse)
        {
            var tokenEnumerator = new TokenEnumerator(new ByteArrayTokenReader(segments));

            return _stateMachine.TryMake(context, tokenEnumerator, out command, out errorResponse);
        }

        /// <summary>
        /// Execute the command.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        /// <param name="context">The execution context to operate on.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task which asynchronously performs the execution.</returns>
        Task<bool> ExecuteAsync(SmtpCommand command, SmtpSessionContext context, CancellationToken cancellationToken)
        {
            context.RaiseCommandExecuting(command);

            return command.ExecuteAsync(context, cancellationToken);
        }

        /// <summary>
        /// Output the greeting.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task which performs the operation.</returns>
        async Task OutputGreetingAsync(CancellationToken cancellationToken)
        {
            var version = typeof(SmtpSession).GetTypeInfo().Assembly.GetName().Version;

            await _context.NetworkClient.WriteLineAsync($"220 {_context.ServerOptions.ServerName} v{version} ESMTP ready", cancellationToken).ReturnOnAnyThread();
            await _context.NetworkClient.FlushAsync(cancellationToken).ReturnOnAnyThread();
        }

        async Task IngestProxyAsync(SmtpSessionContext context, CancellationToken cancellationToken)
        {
            var text = await ReadCommandInputAsync(context, cancellationToken);

            if (TryMake(context, text, out SmtpCommand command, out SmtpResponse response))
            {
                if (await ExecuteAsync(command, context, cancellationToken).ReturnOnAnyThread())
                {
                    await OutputGreetingAsync(cancellationToken).ReturnOnAnyThread();
                    await _context.NetworkClient.FlushAsync(cancellationToken).ReturnOnAnyThread();
                }
                else
                {
                    await _context.NetworkClient.FlushAsync(cancellationToken).ReturnOnAnyThread();
                }
            }   
        }
        
        /// <summary>
        /// Returns the completion task.
        /// </summary>
        internal Task<bool> Task => _taskCompletionSource.Task;
    }
}