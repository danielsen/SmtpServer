using SmtpServer.Protocol;

namespace SmtpServer
{
    public class SmtpResponseEventArgs : SessionEventArgs
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="context">The session context.</param>
        /// <param name="response">The response.</param>
        public SmtpResponseEventArgs(ISessionContext context, SmtpResponse response) : base(context)
        {
            Response = response;
        }

        /// <summary>
        /// The response for this event.
        /// </summary>
        public SmtpResponse Response { get; }
    }
}
