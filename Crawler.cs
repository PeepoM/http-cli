using System.Text;

namespace Program
{
    class Crawler
    {
        private readonly HttpClient _client;

        public Crawler()
        {
            _client = new HttpClient();
        }

        public async Task DisplayContents(Stream streamContents)
        {
            StreamReader reader = new StreamReader(streamContents);
            string contents = await reader.ReadToEndAsync();
            Console.WriteLine(contents);
        }

        public async Task DownloadContents(string fileName, long contentsStreamLen, Stream contentsStream)
        {
            Console.WriteLine("Proceeding to download files:");

            using (Stream source = contentsStream)
            {
                string cwd = Directory.GetCurrentDirectory();
                string filePath = Path.Combine(cwd, fileName);

                using (Stream fileStream = File.Create(filePath))
                {
                    int strWidth = Console.WindowWidth - 5;
                    StringBuilder sb = new StringBuilder();
                    double propHash = 100f / strWidth;  // amount of % represented by a single '#' sumbol
                    int numHash = 0;  // number of '#' symbols rendered

                    byte[] buffer = new byte[4096];

                    (_, int startTop) = Console.GetCursorPosition();

                    int read, readTotal = 0;
                    double percent = 0d;
                    while ((read = await source.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        readTotal += read;

                        double newPercent = Math.Round(100d * readTotal / contentsStreamLen);

                        if (newPercent > percent)
                        {
                            percent = newPercent;

                            // Calculate the number of new '#' symbols to add to progress bar
                            if (percent >= (numHash + 1) * propHash)
                            {
                                double percentDiff = percent - numHash * propHash;
                                int numNewHashes = (int)Math.Round(percentDiff / propHash);

                                string newHashes = new String('#', numNewHashes);
                                sb.Append(newHashes);

                                numHash += numNewHashes;
                            }

                            string dashes = new String('-', strWidth - numHash);

                            Console.SetCursorPosition(0, startTop);
                            Console.Write($"{sb}{dashes} {percent:0}%");
                        }

                        fileStream.Write(buffer, 0, read);
                    }
                }
            }

            Console.WriteLine("Download has completed successfully");
        }

        public async Task<(long, Stream?)> FetchContents(Uri url)
        {
            HttpResponseMessage response = await _client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);

            Console.WriteLine($"Sending a GET request to {url}");
            response.EnsureSuccessStatusCode();
            Console.WriteLine($"Files successfully fetched from the server\n");

            long streamLen = response.Content.Headers.ContentLength.HasValue ?
                        response.Content.Headers.ContentLength.Value : -1L;

            return (streamLen, await response.Content.ReadAsStreamAsync());
        }

        public async Task<Stream?> PostData(string? data, Uri url)
        {
            FormUrlEncodedContent? formContent = null;

            if (data != null)
            {
                // Have to convert delimited string to dictionary the same as "curl" does
                Dictionary<string, string> dict = data.
                            Split('&', StringSplitOptions.RemoveEmptyEntries).
                            Select(part => part.Split('=', 2)).
                            ToDictionary(split => split[0],
                                        split => split.Length == 2 ? split[1] : "");

                formContent = new FormUrlEncodedContent(dict);
            }

            HttpResponseMessage response = await _client.PostAsync(url, formContent);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStreamAsync();
        }
    }
}