using Microsoft.Extensions.Logging;
//TODO: Check if this is actually a modern solution because it really doesnt look like it
/// <summary>
/// MsTestLogger<typeparamref name="T"/> for ILogger
/// </summary>
/// <typeparam name="T"></typeparam>
public class MsTestLogger<T> : ILogger<T>, IDisposable
{
    private TestContext _output;

    public MsTestLogger(TestContext output)
    {
        _output = output;
    }
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
        _output.WriteLine("{0}: {1}\n{2}", logLevel, state.ToString(), (exception == null) ? string.Empty : exception.ToString());
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public IDisposable BeginScope<TState>(TState state)
    {
        return this;
    }

    public void Dispose()
    {
    }
}
