using System;
using System.Collections.Generic;
using System.Net;
using SmtpServer.IO;

namespace SmtpServer
{
    public interface ISessionContext
    {
        /// <summary>
        /// Fired when a command is about to execute.
        /// </summary>
        event EventHandler<SmtpCommandExecutingEventArgs> CommandExecuting;

        /// <summary>
        /// Fired when the session has been authenticated.
        /// </summary>
        event EventHandler<EventArgs> SessionAuthenticated;

        /// <summary>
        /// Fired when the session delivers a response.
        /// </summary>
        event EventHandler<SmtpResponseEventArgs> ResponseDelivered;

        /// <summary>
        /// Gets the options that the server was created with.
        /// </summary>
        ISmtpServerOptions ServerOptions { get; }

        /// <summary>
        /// Gets the endpoint definition.
        /// </summary>
        IEndpointDefinition EndpointDefinition { get; }

        /// <summary>
        /// Gets the text stream to read from and write to.
        /// </summary>
        INetworkClient NetworkClient { get; }

        /// <summary>
        /// Gets the current transaction.
        /// </summary>
        SmtpMessageTransaction Transaction { get; }

        /// <summary>
        /// Returns a value indicating whether or nor the current session is authenticated.
        /// </summary>
        bool IsAuthenticated { get; }

        /// <summary>
        /// Returns a set of propeties for the current session.
        /// </summary>
        IDictionary<string, object> Properties { get; }

        /// <summary>
        /// The source address of the client connected to a proxy as reported by proxy-protocol.
        /// </summary>
        IPEndPoint ProxySourceEndpoint { get; }

        /// <summary>
        /// The destination endpoint on the proxy as reported by proxy-protocol.
        /// </summary>
        IPEndPoint ProxyDestinationEndpoint { get; }

        /// <summary>
        /// Number of transactions in this session.
        /// </summary>
        int TransactionCount { get; }

        /// <summary>
        /// Number of recipients independent of transaction.
        /// </summary>
        int RecipientCount { get; }
    }
}