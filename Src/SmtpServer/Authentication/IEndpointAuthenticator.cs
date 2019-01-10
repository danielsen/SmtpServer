using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SmtpServer.Authentication
{
    public interface IEndpointAuthenticator
    {
        /// <summary>
        /// Authenticates a remote endpoint and the PROXY command source.
        /// As noted in http://www.haproxy.org/download/1.8/doc/proxy-protocol.txt
        /// SMTP servers accepting the PROXY protocol should make an effort to validate
        /// the source of the PROXY command against a list of known proxy addresses,
        /// i.e. a list of upstream hosts allowed to connect with PROXY.
        /// Additionally, this authenticator can operate on source (remote) address
        /// to authenticate the session.
        /// </summary>
        /// <param name="context">The session context.</param>
        /// <param name="sourceAddress">The PROXY source (remote) address.</param>
        /// <param name="destinationAddress">The PROXY destination (local) address.</param>
        /// <param name="proxyAddresses">List of allowed PROXY destinations.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<bool> AuthenticateAsync(ISessionContext context, string sourceAddress,
            string destinationAddress, IReadOnlyCollection<IPAddress> proxyAddresses,
            CancellationToken cancellationToken);
    }
}
