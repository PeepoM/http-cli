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

    public async Task DisplayResultAsync(HttpResponseMessage response)
    {
        var contents = await response.Content.ReadAsStringAsync();
        Console.WriteLine(contents);
    }

    public async Task DownloadResultContentsAsync(string fileName, HttpResponseMessage response)
    {
        Console.WriteLine("Proceeding to download files:");

        await using (var source = await response.Content.ReadAsStreamAsync())
        {
            var cwd = Directory.GetCurrentDirectory();
            var filePath = Path.Combine(cwd, fileName);

            await using (Stream fileStream = File.Create(filePath))
            {
                var strWidth = Console.WindowWidth - 5;
                var sb = new StringBuilder();
                double propHash = 100f / strWidth; // amount of % represented by a single '#' symbol
                var numHash = 0; // number of '#' symbols rendered

                var buffer = new byte[4096];

                var sourceLen = response.Content.Headers.ContentLength ?? -1L;

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

    public async Task<HttpResponseMessage> FetchContents(Uri url)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

        Console.WriteLine($"Sending a GET request to {url}");
        response.EnsureSuccessStatusCode();
        Console.WriteLine($"Files successfully fetched from the server\n");

        return response;
    }

    public async Task<HttpResponseMessage> PostData(string? data, Uri url)
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

        var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

        return response;
    }
}