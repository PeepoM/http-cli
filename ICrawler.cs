namespace project;

public interface ICrawler
{
    Task DisplayResponseContentsAsync(HttpResponseMessage response);

    Task DownloadResponseContentsAsync(string fileName, HttpResponseMessage response);

    Task<HttpResponseMessage> FetchContentsAsync(Uri url);

    Task<HttpResponseMessage> PostDataAsync(string? data, Uri url);
}