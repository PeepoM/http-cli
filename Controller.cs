namespace project;

public class Controller
{
    public static async Task<int> Main(string[] args)
    {
        // Website examples to get different types of files from
        // "http://speedtest.tele2.net/1MB.zip";
        // "http://webcode.me/";
        // "https://www.gnu.org/graphics/gnu-and-penguin-color-300x276.jpg";

        // Website to post file for testing purposes
        // "https://httpbin.org/post"
        ICrawler crawler = new Crawler();
        return await new Command(crawler).InvokeCommandAsync(args);
    }
}