using System.CommandLine;
using static System.Net.WebRequestMethods;

namespace project;

public class Command
{
    private readonly ICrawler _crawler;

    public Command(ICrawler crawler)
    {
        _crawler = crawler;
    }

    public async Task<int> InvokeCommandAsync(string[] args)
    {
        var urlArgument = new Argument<Uri>(
            name: "URL",
            description: "Address of a web page");

        var outputOption = new Option<string?>(
            name: "--output",
            description: "Write output to a file instead of stdout");
        outputOption.AddAlias("-o");

        var requestOption = new Option<string?>(
            name: "--request",
            description: "Specify type of HTTP request command to use");
        requestOption.AddAlias("-X");

        var dataOption = new Option<string?>(
            name: "--data",
            description: "Initiate an HTTP POST request to post data");
        dataOption.AddAlias("-d");

        var rootCommand = new RootCommand("curl-like command line tool to transfer data to and from servers");
        rootCommand.SetHandler(
            async (output, request, data, url) => { await HandleOptionsAsync(output, request, data, url!); },
            outputOption, requestOption, dataOption, urlArgument);
        rootCommand.AddArgument(urlArgument);
        rootCommand.AddOption(outputOption);
        rootCommand.AddOption(requestOption);
        rootCommand.AddOption(dataOption);

        return await rootCommand.InvokeAsync(args);
    }

    private async Task HandleOptionsAsync(string? outputFile, string? request, string? data, Uri url)
    {
        try
        {
            Task<HttpContent> crawlerTask;

            if (data != null || request == Http.Post)
                crawlerTask = _crawler.PostDataAsync(data, url);
            else if (request == Http.Get)
                crawlerTask = _crawler.FetchContentAsync(url);
            else
                crawlerTask = _crawler.FetchContentAsync(url);

            Console.WriteLine("Waiting for a response...");
            var httpContent = await crawlerTask;

            if (outputFile != null)
                await _crawler.DownloadContentAsync(outputFile, httpContent);
            else
                await _crawler.DisplayContentAsync(httpContent);
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine($"Exception: {e.Message}");
        }
        catch (TaskCanceledException e)
        {
            Console.WriteLine($"Exception: {e.Message}");
        }
    }
}