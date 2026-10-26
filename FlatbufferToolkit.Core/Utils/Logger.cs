namespace FlatbufferToolkit.Utils;

public enum LogLevel
{
    LOG,
    WARN,
    ERROR
}

public sealed class Logger
{
    public static Logger Instance
    {
        get;
    } = new();

    public event Action<LogLevel, string>? MessageLogged;
    private Logger()
    {
    }

    public void Log(LogLevel level, string message)
    {
        MessageLogged?.Invoke(level, $"[{DateTime.Now:HH:mm:ss} {level}] {message}{Environment.NewLine}");
    }
}
