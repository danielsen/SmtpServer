using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using SmtpServer;
using SmtpServer.Authentication;
using SmtpServer.Tracing;

namespace SmtpServer.SampleApp
{
    class Program
    {
        public class SimpleEndpointAuthenticator : EndpointAuthenticator
        {
            public SimpleEndpointAuthenticator() { }

            public override Task<bool> AuthenticateAsync(ISessionContext context,
                string sourceAddress, string destinationAddress,
                IReadOnlyCollection<IPAddress> proxyAddresses, CancellationToken cancellationToken)
            {
                var proxyAddress = IPAddress.Parse(destinationAddress);

                if (!proxyAddresses.Any(x => x.Equals(proxyAddress)))
                {
                    return Task.FromResult(false);
                }

                context.Properties["Authenticated"] = false;
                return Task.FromResult(true);
            }
        }

        public class SimpleServer
        {
            public void Run()
            {
                var cancellationTokenSource = new CancellationTokenSource();

                var options = new SmtpServerOptionsBuilder()
                    .ServerName("mail-x.domain.com")
                    .Endpoint(b => b
                        .Port(8025)
                        .AuthenticationRequired()
                        .AllowUnsecureAuthentication()
                    )
                    .CommandWaitTimeout(TimeSpan.FromSeconds(100))
                    .EndpointAuthenticator(new SimpleEndpointAuthenticator())
                    .ProxyAddresses(new List<string>(){"192.168.1.1"})
                    .Build();

                var server = new SmtpServer(options);
                server.SessionCreated += OnSessionCreated;
                server.SessionCompleted += OnSessionCompleted;

                var serverTask = server.StartAsync(cancellationTokenSource.Token);

                Console.WriteLine("Press any key to shutdown");
                Console.ReadKey();

                cancellationTokenSource.Cancel();
                serverTask.Wait();
            }

            static void OnSessionCreated(object sender, SessionEventArgs e)
            {
                Console.WriteLine("Session Created.");
                e.Context.CommandExecuting += OnCommandExecuting;
            }

            static void OnCommandExecuting(object sender, SmtpCommandExecutingEventArgs e)
            {
                Console.WriteLine("Command executing.");

                new TracingSmtpCommandVisitor(Console.Out).Visit(e.Command);
            }

            static void OnSessionCompleted(object sender, SessionEventArgs e)
            {
                Console.WriteLine("Session completed");
            }
        }

        static void Main(string[] args)
        {
            var server = new SimpleServer();
            server.Run();
        }
    }
}
