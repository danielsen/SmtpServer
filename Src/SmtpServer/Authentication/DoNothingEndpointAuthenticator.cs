using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SmtpServer.Authentication
{
    internal sealed class DoNothingEndpointAuthenticator : EndpointAuthenticator
    {
        internal static readonly DoNothingEndpointAuthenticator Instance = new DoNothingEndpointAuthenticator();

        public override Task<bool> AuthenticateAsync(ISessionContext context, string sourceAddress,
            string destinationAddress, IReadOnlyCollection<IPAddress> proxyAddresses,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(true);
        }
    }
}
