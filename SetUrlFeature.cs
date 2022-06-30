using System.Net.Http;
using System.CommandLine;
using System.Threading.Tasks;
using System.CommandLine.Invocation;
using System.Threading;
using System;

namespace project
{
    public class SetUrlFeature : IFeature
    {
        private readonly Argument<Uri> urlArgument;

        public SetUrlFeature()
        {
            urlArgument = new Argument<Uri>(
                name: "URL",
                description: "Address of a web page");
        }

        public void Activate(RootCommand rootCommand) => rootCommand.AddArgument(urlArgument);

        public HttpMessageHandler Decorate(InvocationContext context, HttpMessageHandler innerClient)
        {
            var uri = context.ParseResult.GetValueForArgument(urlArgument);
            return (uri is null) ? innerClient : new SetUrlHttpHandler(uri, innerClient);
        }

        internal class SetUrlHttpHandler : DelegatingHandler
        {
            private readonly Uri uri;

            public SetUrlHttpHandler(Uri uri, HttpMessageHandler innerClient)
                : base(innerClient) => this.uri = uri;

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                request.RequestUri = uri;
                return base.SendAsync(request, cancellationToken);
            }
        }
    }
}
