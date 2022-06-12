namespace project;

public interface ICrawler
{
    Task DisplayContentAsync(HttpContent httpContent);

    Task DownloadContentAsync(string fileName, HttpContent httpContent);

    Task<HttpContent> FetchContentAsync(Uri url);

    Task<HttpContent> PostDataAsync(string? data, Uri url);
}