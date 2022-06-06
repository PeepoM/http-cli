using System.CommandLine;
using Program;
using static System.Net.WebRequestMethods;

class Command
{
    public static async Task<int> InitializeCommand(Crawler crawler, string[] args)
    {
        Argument<Uri> urlArgument = new Argument<Uri>(
            name: "URL",
            description: "Address of a web page");

        Option<string?> outputOption = new Option<string?>(
            name: "--output",
            description: "Write output to a file instead of stdout");
        outputOption.AddAlias("-o");

        Option<string?> requestOption = new Option<string?>(
            name: "--request",
            description: "Specify type of HTTP request command to use");
        requestOption.AddAlias("-X");

        Option<string?> dataOption = new Option<string?>(
            name: "--data",
            description: "Initiate an HTTP POST request to post data");
        dataOption.AddAlias("-d");

        RootCommand rootCommand = new RootCommand("curl-like command line tool to transfer data to and from servers");
        rootCommand.SetHandler(async (output, request, data, url) =>
        {
            await HandleOptions(crawler, output, request, data, url!);
        }, outputOption, requestOption, dataOption, urlArgument);
        rootCommand.AddArgument(urlArgument);
        rootCommand.AddOption(outputOption);
        rootCommand.AddOption(requestOption);
        rootCommand.AddOption(dataOption);

        return await rootCommand.InvokeAsync(args);
    }

    private static async Task HandleOptions(
        Crawler crawler,
        string? output,
        string? request,
        string? data,
        Uri url)
    {
        Stream? contents = null;
        long size = 0L;

        if (data != null || request == Http.Post)
            contents = await crawler.PostData(data, url);
        else if (request == Http.Get)
            (size, contents) = await crawler.FetchContents(url);
        else
            (size, contents) = await crawler.FetchContents(url);

        if (output != null)
            await crawler.DownloadContents(output, size!, contents!);
        else
            await crawler.DisplayContents(contents!);
    }
}