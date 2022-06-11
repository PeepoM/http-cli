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
            async (output, request, data, url) => { await HandleOptions(output, request, data, url!); },
            outputOption, requestOption, dataOption, urlArgument);
        rootCommand.AddArgument(urlArgument);
        rootCommand.AddOption(outputOption);
        rootCommand.AddOption(requestOption);
        rootCommand.AddOption(dataOption);

        return await rootCommand.InvokeAsync(args);
    }

    private async Task HandleOptions(string? output, string? request, string? data, Uri url)
    {
        Task<HttpResponseMessage> task;

        if (data != null || request == Http.Post)
            task = _crawler.PostData(data, url);
        else if (request == Http.Get)
            task = _crawler.FetchContents(url);
        else
            task = _crawler.FetchContents(url);

        Console.WriteLine("Waiting for a response...");
        var response = await task;

        if (output != null)
            await _crawler.DownloadResponseContentsAsync(output, response);
        else
            await _crawler.DisplayResultAsync(response);
    }
}