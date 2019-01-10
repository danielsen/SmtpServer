using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using SmtpServer.Authentication;
using SmtpServer.IO;
using SmtpServer.Storage;

namespace SmtpServer.Protocol
{
    /// <summary>
    /// Support for proxy protocol version 1 header for use with HAProxy.
    /// Documented at http://www.haproxy.org/download/1.8/doc/proxy-protocol.txt
    /// This should always (and only ever) be the first command seen on a new connection from HAProxy
    /// </summary>
    public sealed class ProxyCommand : SmtpCommand
    {
        public const string Command = "PROXY";

        public IPEndPoint SourceEndpoint { get; }
        public IPEndPoint DestinationEndpoint { get; }

        public ProxyCommand(ISmtpServerOptions options, IPEndPoint sourceEndpoint, IPEndPoint destinationEndpoint) : base(options)
        {
            SourceEndpoint = sourceEndpoint;
            DestinationEndpoint = destinationEndpoint;
        }

        internal override async Task<bool> ExecuteAsync(SmtpSessionContext context, CancellationToken cancellationToken)
        {
            context.ProxySourceEndpoint = SourceEndpoint;
            context.ProxyDestinationEndpoint = DestinationEndpoint;
            context.Properties["SourceAddress"] = SourceEndpoint.Address.ToString();
            context.Properties["DestinationAddress"] = DestinationEndpoint.Address.ToString();

            using (var container =
                new DisposableContainer<IEndpointAuthenticator>(
                    Options.EndpointAuthenticatorFactory.CreateInstance(context)))
            {
                if (await container.Instance.AuthenticateAsync(context, SourceEndpoint.Address.ToString(),
                    DestinationEndpoint.Address.ToString(), 
                    Options.ProxyAddresses, cancellationToken).ReturnOnAnyThread() == false)
                {
                    var response = new SmtpResponse(SmtpReplyCode.ClientNotPermitted,
                        $"PROXY not permitted from {DestinationEndpoint.Address}");
                    await context.NetworkClient.ReplyAsync(response, cancellationToken).ReturnOnAnyThread();
                    context.IsQuitRequested = true;
                    return false;
                }

                // IEndpointAuthenticator implementations are free to authenticate the
                // session based on the IPs delivered by the proxy command obviating the
                // need for further authentication. RaiseSessionAuthenticated() can be
                // called in this case to transition the state machine.
                context.Properties.TryGetValue("Authenticated", out object authenticated);

                if (authenticated != null)
                {
                    if ((bool) authenticated)
                    {
                        context.IsAuthenticated = true;
                        context.RaiseSessionAuthenticated();
                    }
                }

                return true;
            }
        }
    }
}