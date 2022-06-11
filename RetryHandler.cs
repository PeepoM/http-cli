namespace project;

public class RetryHandler : DelegatingHandler
{
    private readonly int _maxRetries;
    private readonly int _waitTimeMillis;

    public RetryHandler(int maxRetries = 3, int waitTimeMillis = 3000)
    {
        _maxRetries = maxRetries;
        _waitTimeMillis = waitTimeMillis;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        HttpResponseMessage response;
        bool isIdempotent = request.Method != HttpMethod.Post &&
                            request.Method != HttpMethod.Patch;

        int i = 0, statusCode;
        do
        {
            response = await base.SendAsync(request, cancellationToken);

            statusCode = (int)response.StatusCode;

            if ((statusCode is >= 500 and < 600) && isIdempotent && i + 1 < _maxRetries)
            {
                Console.WriteLine($"Waiting {_waitTimeMillis / 1000} sec before trying again...");
                await Task.Delay(_waitTimeMillis, cancellationToken);
            }
        } while ((statusCode is >= 500 and < 600) && isIdempotent && i++ < _maxRetries);

        return response;
    }
}