using System.Net.Http;
using System.CommandLine;
using System.CommandLine.Invocation;

namespace project
{
    public interface IFeature
    {
        void Activate(RootCommand rootCommand);
        HttpMessageHandler Decorate(InvocationContext context, HttpMessageHandler innerClient);
    }
}
