using System.Net.Http;
using System.CommandLine;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.CommandLine.Invocation;
using System.Threading;
using System;

namespace project
{
    public class RootCommandBuilder
    {
        private readonly IEnumerable<IFeature> features;

        public RootCommandBuilder(IEnumerable<IFeature> features)
        {
            this.features = features;
        }

        public RootCommand BuildRootCommand()
        {
            var rootCommand = new RootCommand("curl-like command line tool to transfer data to and from servers");

            foreach (var feature in features)
            {
                feature.Activate(rootCommand);
            }

            rootCommand.SetHandler(HandleInvocation);

            return rootCommand;
        }

        private async Task HandleInvocation(InvocationContext context)
        {
            HttpMessageHandler messageHandler = new SocketsHttpHandler();
            foreach (var feature in features)
            {
                messageHandler = feature.Decorate(context, messageHandler);
            }

            using var client = new HttpClient(messageHandler, true) { BaseAddress = new Uri("https://example.com") };

            await client.SendAsync(new HttpRequestMessage(), HttpCompletionOption.ResponseContentRead, CancellationToken.None);
        }
    }
}
