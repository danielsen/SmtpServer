using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace SmtpServer.Authentication
{
    public abstract class EndpointAuthenticator : IEndpointAuthenticator, IEndpointAuthenticatorFactory
    {
        public abstract Task<bool> AuthenticateAsync(ISessionContext context, string sourceAddress,
            string destinationAddress, IReadOnlyCollection<IPAddress> proxyAddresses, 
            CancellationToken cancellationToken);

        public virtual IEndpointAuthenticator CreateInstance(ISessionContext context)
        {
            return this;
        }
    }
}
