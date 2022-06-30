using System.CommandLine;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace project
{

    public static class Controller
    {
        public static async Task<int> Main(string[] args)
        {
            // Website examples to get different types of files from
            // "http://speedtest.tele2.net/1MB.zip";
            // "http://webcode.me/";
            // "https://www.gnu.org/graphics/gnu-and-penguin-color-300x276.jpg";

            // Website to post file for testing purposes
            // "https://httpbin.org/post"

            var host = Host
                .CreateDefaultBuilder(args)
                .ConfigureServices((builder, services) =>
                {
                    services.AddTransient<RootCommandBuilder>();
                    services.TryAddEnumerable(ServiceDescriptor.Transient<IFeature, SetHttpVerbFeature>());
                    services.TryAddEnumerable(ServiceDescriptor.Transient<IFeature, SetUrlFeature>());
                    services.TryAddEnumerable(ServiceDescriptor.Transient<IFeature, VerboseFeature>());
                    services.TryAddEnumerable(ServiceDescriptor.Transient<IFeature, OutputFeature>());
                })
                .Build();

            var rootCommand = host.Services.GetRequiredService<RootCommandBuilder>().BuildRootCommand();

            return await rootCommand.InvokeAsync(args);
        }
    }
}
