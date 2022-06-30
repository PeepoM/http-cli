using System.Net.Http;
using System.CommandLine;
using System.Threading.Tasks;
using System.CommandLine.Invocation;
using System.Threading;

namespace project
{
    public class SetHttpVerbFeature : IFeature
    {
        private readonly Option<string> requestOption;

        public SetHttpVerbFeature()
        {
            requestOption = new Option<string>(
                    name: "--request",
                    description: "Specify type of HTTP request verb to use");
            requestOption.AddAlias("-X");
            requestOption.ArgumentHelpName = "verb";
        }

        public void Activate(RootCommand rootCommand) => rootCommand.AddOption(requestOption);

        public HttpMessageHandler Decorate(InvocationContext context, HttpMessageHandler innerClient)
        {
            var optionValue = context.ParseResult.GetValueForOption(requestOption);
            return (optionValue is null) ? innerClient : new SetVerbHttpHandler(optionValue, innerClient);
        }

        internal class SetVerbHttpHandler : DelegatingHandler
        {
            private readonly HttpMethod method;

            public SetVerbHttpHandler(string optionValue, HttpMessageHandler innerClient)
                : base(innerClient) => method = new HttpMethod(optionValue);

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                request.Method = method;
                return base.SendAsync(request, cancellationToken);
            }
        }
    }
}
