namespace SmtpServer.Authentication
{
    public interface IEndpointAuthenticatorFactory
    {
        /// <summary>
        /// Creates an instance of an EndpointAuthenticator for the given session.
        /// </summary>
        /// <param name="context">The session context.</param>
        /// <returns>The EndpointAuthenticator instance for the session context.</returns>
        IEndpointAuthenticator CreateInstance(ISessionContext context);
    }
}
