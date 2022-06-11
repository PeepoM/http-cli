using System.Text;

namespace project;

public class Crawler : ICrawler
{
    private readonly HttpClient _client;

    public Crawler()
    {
        DelegatingHandler retryHandler = new RetryHandler();
        retryHandler.InnerHandler = new HttpClientHandler();
        _client = new HttpClient(retryHandler);
    }

    public async Task DisplayResultAsync(HttpResponseMessage response)
    {
        string contents = await response.Content.ReadAsStringAsync();
        Console.WriteLine(contents);
    }

    public async Task DownloadResultContentsAsync(string fileName, HttpResponseMessage response)
    {
        Console.WriteLine("Proceeding to download files:");

        await using (Stream source = await response.Content.ReadAsStreamAsync())
        {
            string cwd = Directory.GetCurrentDirectory();
            string filePath = Path.Combine(cwd, fileName);

            await using (Stream fileStream = File.Create(filePath))
            {
                int strWidth = Console.WindowWidth - 5;
                StringBuilder sb = new StringBuilder();
                double propHash = 100f / strWidth; // amount of % represented by a single '#' symbol
                int numHash = 0; // number of '#' symbols rendered

                byte[] buffer = new byte[4096];

                long sourceLen = response.Content.Headers.ContentLength ?? -1L;

                (_, int startTop) = Console.GetCursorPosition();

                int read, readTotal = 0;
                double percent = 0d;
                while ((read = await source.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    readTotal += read;

                    double newPercent = Math.Round(100d * readTotal / sourceLen);

                    if (newPercent > percent)
                    {
                        percent = newPercent;

                        // Calculate the number of new '#' symbols to add to progress bar
                        if (percent >= (numHash + 1) * propHash)
                        {
                            double percentDiff = percent - numHash * propHash;
                            int numNewHashes = (int)Math.Round(percentDiff / propHash);

                            string newHashes = new string('#', numNewHashes);
                            sb.Append(newHashes);

                            numHash += numNewHashes;
                        }

                        string dashes = new string('-', strWidth - numHash);

                        Console.SetCursorPosition(0, startTop);
                        Console.Write($"{sb}{dashes} {percent:0}%");
                    }

                    await fileStream.WriteAsync(buffer, 0, read);
                }
            }
        }

        Console.WriteLine("Download has completed successfully");
    }

    public async Task<HttpResponseMessage> FetchContents(Uri url)
    {
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
        HttpResponseMessage response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

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
            Dictionary<string, string> dict = data.Split('&', StringSplitOptions.RemoveEmptyEntries)
                .Select(part => part.Split('=', 2)).ToDictionary(split => split[0],
                    split => split.Length == 2 ? split[1] : "");

            formContent = new FormUrlEncodedContent(dict);
        }

        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Content = formContent;

        HttpResponseMessage response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

        return response;
    }
}