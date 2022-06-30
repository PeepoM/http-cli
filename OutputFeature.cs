using System.Net.Http;
using System.CommandLine;
using System.Threading.Tasks;
using System.CommandLine.Invocation;
using System.Threading;
using System;
using System.IO;

namespace project
{
    public class OutputFeature : IFeature
    {
        private readonly Option<string> outputOption;

        public OutputFeature()
        {
            outputOption = new Option<string>(
                name: "--output",
                description: "Write output to a file instead of stdout");
            outputOption.AddAlias("-o");
            outputOption.ArgumentHelpName = "file";
        }

        public void Activate(RootCommand rootCommand) => rootCommand.AddOption(outputOption);

        public HttpMessageHandler Decorate(InvocationContext context, HttpMessageHandler innerClient)
        {
            var optionValue = context.ParseResult.GetValueForOption(outputOption);
            return (optionValue is null) ? new WriteToConsoleHttpHandler(innerClient) : new WriteToFileHttpHandler(optionValue, innerClient);
        }

        internal class WriteToConsoleHttpHandler : DelegatingHandler
        {

            public WriteToConsoleHttpHandler(HttpMessageHandler innerClient)
                : base(innerClient) { }

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                var response = await base.SendAsync(request, cancellationToken);

                await response.Content.LoadIntoBufferAsync();
                Console.WriteLine(await response.Content.ReadAsStringAsync());

                return response;
            }
        }

        internal class WriteToFileHttpHandler : DelegatingHandler
        {
            private readonly string fileName;

            public WriteToFileHttpHandler(string optionValue, HttpMessageHandler innerClient)
                : base(innerClient) => fileName = optionValue;

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                var response = await base.SendAsync(request, cancellationToken);
                await response.Content.LoadIntoBufferAsync();
                using var fileStream = File.Create(fileName);
                await response.Content.CopyToAsync(fileStream);

                return response;
            }
        }
    }
}
