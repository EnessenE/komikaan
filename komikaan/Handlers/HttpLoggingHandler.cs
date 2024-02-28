namespace komikaan.Handlers;

public class HttpLoggingHandler : DelegatingHandler
{
    private readonly ILogger<HttpLoggingHandler> _logger;

    public HttpLoggingHandler(ILogger<HttpLoggingHandler> logger)
    {
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Guid id = Guid.NewGuid();
        HttpResponseMessage response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        _logger.LogDebug("[{Id}] Request: {Request}", id, request);
        _logger.LogDebug("[{Id}] Response: {Response}", id, response);

        //Avoid executed this call unless we are actually in trace
        if (_logger.IsEnabled(LogLevel.Trace))
        {
            _logger.LogTrace("[{Id}] Raw data: {data}", id, response.Content.ReadAsStringAsync().GetAwaiter().GetResult());
        }

        return response;
    }
}