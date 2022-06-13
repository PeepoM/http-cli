using System.Text;

namespace project;

public class Crawler : ICrawler
{
    private readonly HttpClient _client;

    public Crawler()
    {
        var retryHandler = new RetryHandler();
        retryHandler.InnerHandler = new HttpClientHandler();
        _client = new HttpClient(retryHandler);
    }

    public async Task DisplayContentAsync(HttpContent httpContent)
    {
        var strContent = await httpContent.ReadAsStringAsync();
        Console.WriteLine(strContent);
    }

    public async Task DownloadContentAsync(string fileName, HttpContent httpContent)
    {
        Console.WriteLine("Proceeding to download files:");

        await using (var source = await httpContent.ReadAsStreamAsync())
        {
            var cwd = Directory.GetCurrentDirectory();
            var filePath = Path.Combine(cwd, fileName);

            await using (Stream fileStream = File.Create(filePath))
            {
                var strWidth = Console.WindowWidth - 5;
                var sb = new StringBuilder();
                var propHash = 100d / strWidth; // amount of % represented by a single '#' symbol
                var numHash = 0; // number of '#' symbols rendered

                var buffer = new byte[4096];

                var sourceLen = httpContent.Headers.ContentLength ?? -1L;

                var (_, startTop) = Console.GetCursorPosition();

                int read, readTotal = 0;
                var percent = 0d;
                while ((read = await source.ReadAsync(buffer)) > 0)
                {
                    readTotal += read;

                    var newPercent = Math.Round(100d * readTotal / sourceLen);

                    if (newPercent > percent)
                    {
                        percent = newPercent;

                        // Calculate the number of new '#' symbols to add to progress bar
                        if (percent >= (numHash + 1) * propHash)
                        {
                            var percentDiff = percent - numHash * propHash;
                            var numNewHashes = (int)Math.Round(percentDiff / propHash);

                            var newHashes = new string('#', numNewHashes);
                            sb.Append(newHashes);

                            numHash += numNewHashes;
                        }

                        var dashes = new string('-', strWidth - numHash);

                        Console.SetCursorPosition(0, startTop);
                        Console.Write($"{sb}{dashes} {percent:0}%");
                    }

                    await fileStream.WriteAsync(buffer.AsMemory(0, read));
                }
            }
        }

        Console.WriteLine("Download has completed successfully");
    }

    public async Task<HttpResponseMessage> FetchDataAsync(Uri url)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        Console.WriteLine($"Sending a {request.Method} request to the server");

        var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

        Console.WriteLine($"Files successfully fetched\n");

        return response;
    }

    public async Task<HttpResponseMessage> PostDataAsync(string? data, Uri url)
    {
        FormUrlEncodedContent? formContent = null;

        if (data != null)
        {
            // Have to convert delimited string to dictionary the same as "curl" does
            var dict = data.Split('&', StringSplitOptions.RemoveEmptyEntries)
                .Select(part => part.Split('=', 2)).ToDictionary(split => split[0],
                    split => split.Length == 2 ? split[1] : "");

            formContent = new FormUrlEncodedContent(dict);
        }

        var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Content = formContent;
        Console.WriteLine($"Sending a {request.Method} request to the server");

        var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

        Console.WriteLine("Data successfully posted\n");

        return response;
    }
}