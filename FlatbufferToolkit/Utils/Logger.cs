namespace FlatbufferToolkit.Utils;

public enum LogLevel
{
    LOG,
    WARN,
    ERROR
};

public sealed class Logger
{
    private static Logger? _instance;
    private static readonly object _lock = new();

    private readonly RichTextBox _textBox;

    private Logger(RichTextBox textBox)
    {
        _textBox = textBox ?? throw new ArgumentNullException(nameof(textBox));
    }

    public static void Initialize(ref RichTextBox textBox)
    {
        lock (_lock)
        {
            if (_instance != null)
                throw new InvalidOperationException("Logger already initialized");

            _instance = new Logger(textBox);
        }
    }

    public static Logger Instance
    {
        get
        {
            if (_instance == null)
                throw new InvalidOperationException("Logger not initialized");

            return _instance;
        }
    }

    public void Log(LogLevel lvl, string message)
    {
        if (_textBox.IsDisposed)
            return;

        string line = $"[{DateTime.Now:HH:mm:ss} {lvl.ToString()}] {message}{Environment.NewLine}";

        if (_textBox.InvokeRequired)
        {
            _textBox.BeginInvoke(() =>
            {
                var col = lvl switch
                {
                    LogLevel.LOG => Color.Black,
                    LogLevel.WARN => Color.Yellow,
                    LogLevel.ERROR => Color.Red,
                    _ => Color.Black
                };
                _textBox.SelectionColor = col;
                _textBox.AppendText(line);
                _textBox.SelectionColor = Color.Black;
            });
        }
        else
        {
            _textBox.AppendText(line);
        }
    }
}