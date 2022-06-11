namespace project;

public interface ICrawler
{
    Task DisplayResultAsync(HttpResponseMessage response);

    Task DownloadResponseContentsAsync(string fileName, HttpResponseMessage response);

    Task<HttpResponseMessage> FetchContents(Uri url);

    Task<HttpResponseMessage> PostData(string? data, Uri url);
}