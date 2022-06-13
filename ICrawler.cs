namespace project;

public interface ICrawler
{
    Task DisplayContentAsync(HttpContent httpContent);

    Task DownloadContentAsync(string fileName, HttpContent httpContent);

    Task<HttpResponseMessage> FetchDataAsync(Uri url);

    Task<HttpResponseMessage> PostDataAsync(string? data, Uri url);
}