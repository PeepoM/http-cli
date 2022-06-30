using System.Net.Http;
using System.CommandLine;
using System.Threading.Tasks;
using System.CommandLine.Invocation;
using System.Threading;
using System;

namespace project
{
    public class VerboseFeature : IFeature
    {
        private readonly Option<bool> verbose;

        public VerboseFeature()
        {
            verbose = new Option<bool>(
                name: "--verbose",
                description: "Verbose mode");
        }

        public void Activate(RootCommand rootCommand) => rootCommand.AddOption(verbose);

        public HttpMessageHandler Decorate(InvocationContext context, HttpMessageHandler innerClient)
        {
            var optionValue = context.ParseResult.GetValueForOption(verbose);
            return !optionValue ? innerClient : new VerboseHttpHandler(innerClient);
        }

        internal class VerboseHttpHandler : DelegatingHandler
        {

            public VerboseHttpHandler(HttpMessageHandler innerClient)
                : base(innerClient) { }

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                var response = await base.SendAsync(request, cancellationToken);

                Console.WriteLine($"{response.RequestMessage?.Method} {response.RequestMessage?.RequestUri} {response.Version} {(int)response.StatusCode} {response.StatusCode}");

                return response;
            }
        }
    }
}
