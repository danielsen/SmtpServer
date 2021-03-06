﻿namespace SmtpServer.Protocol
{
    public class SmtpResponse
    {
        public static readonly SmtpResponse Ok = new SmtpResponse(SmtpReplyCode.Ok, "Ok");
        public static readonly SmtpResponse ServiceReady = new SmtpResponse(SmtpReplyCode.ServiceReady, "ready when you are");
        public static readonly SmtpResponse MailboxUnavailable = new SmtpResponse(SmtpReplyCode.MailboxUnavailable, "mailbox unavailable");
        public static readonly SmtpResponse MailboxNameNotAllowed = new SmtpResponse(SmtpReplyCode.MailboxNameNotAllowed, "mailbox name not allowed");
        public static readonly SmtpResponse ServiceClosingTransmissionChannel = new SmtpResponse(SmtpReplyCode.ServiceClosingTransmissionChannel, "bye");
        public static readonly SmtpResponse SyntaxError = new SmtpResponse(SmtpReplyCode.SyntaxError, "syntax error");
        public static readonly SmtpResponse SizeLimitExceeded = new SmtpResponse(SmtpReplyCode.SizeLimitExceeded, "size limit exceeded");
        public static readonly SmtpResponse NoValidRecipientsGiven = new SmtpResponse(SmtpReplyCode.TransactionFailed, "no valid recipients given");
        public static readonly SmtpResponse AuthenticationFailed = new SmtpResponse(SmtpReplyCode.AuthenticationFailed, "authentication failed");
        public static readonly SmtpResponse AuthenticationSuccessful = new SmtpResponse(SmtpReplyCode.AuthenticationSuccessful, "go ahead");
        public static readonly SmtpResponse TransactionFailed = new SmtpResponse(SmtpReplyCode.TransactionFailed);
        public static readonly SmtpResponse BadSequence = new SmtpResponse(SmtpReplyCode.BadSequence, "bad sequence of commands");
        public static readonly SmtpResponse UnsecuredChannel = new SmtpResponse(SmtpReplyCode.CommandNotImplemented, "command not implemented");
        public static readonly SmtpResponse AuthenticationRequired = new SmtpResponse(SmtpReplyCode.AuthenticationRequired, "authentication required");
        public static readonly SmtpResponse SslUpgradeFailed = new SmtpResponse(SmtpReplyCode.ClientNotPermitted, "protocols not supported");
        public static readonly SmtpResponse AuthenticationError = new SmtpResponse(SmtpReplyCode.ClientNotPermitted, "4.7.0 temporary authentication failure");
        public static readonly SmtpResponse TemporaryRejection = new SmtpResponse(SmtpReplyCode.TemporaryReject, "relay unavailable. Try again later");
        public static readonly SmtpResponse SystemError = new SmtpResponse(SmtpReplyCode.TemporarySystemError, "error processing request. Try again later");
        public static readonly SmtpResponse StartMailInput = new SmtpResponse(SmtpReplyCode.StartMailInput, "start mail input, end with <CRLF>.<CRLF>");

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="replyCode">The reply code.</param>
        /// <param name="message">The reply message.</param>
        public SmtpResponse(SmtpReplyCode replyCode, string message = null)
        {
            ReplyCode = replyCode;
            Message = message;
        }

        /// <summary>
        /// Gets the Reply Code.
        /// </summary>
        public SmtpReplyCode ReplyCode { get; }

        /// <summary>
        /// Gets the repsonse message.
        /// </summary>
        public string Message { get; }

        public string ResponseToString()
        {
            return $"{(int)ReplyCode} {Message}";
        }
    }
}