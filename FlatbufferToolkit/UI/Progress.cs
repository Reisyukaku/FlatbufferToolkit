namespace FlatbufferToolkit.UI;

public sealed class Progress
{
    private static Progress? _instance;
    private readonly Label _progLbl;
    private readonly ProgressBar _progBar;

    private static readonly object _lock = new();

    private Progress(ProgressBar progBar, Label progLbl)
    {
        _progBar = progBar;
        _progLbl = progLbl;
    }

    public static void Initialize(ref ProgressBar progBar, ref Label progLbl)
    {
        lock (_lock)
        {
            if (_instance != null)
                throw new InvalidOperationException("Progressbar already initialized");

            _instance = new Progress(progBar, progLbl);
        }
    }

    public static Progress Instance
    {
        get
        {
            if (_instance == null)
                throw new InvalidOperationException("Progressbar not initialized");

            return _instance;
        }
    }

    public void Setup(int maxVal, string msg)
    {
        if (_progBar.IsDisposed || _progLbl.IsDisposed)
            return;

        if (_progBar.InvokeRequired)
            _progBar.BeginInvoke(() =>
            {
                _progBar.Maximum = maxVal;
                _progBar.Value = 0;
            });
        else
        {
            _progBar.Maximum = maxVal;
            _progBar.Value = 0;
        }

        if (_progLbl.InvokeRequired)
            _progLbl.BeginInvoke(() => { _progLbl.Text = msg; });
        else
            _progLbl.Text = msg;
    }

    public void SetProgress(int val, string msg = "")
    {
        if (_progBar.IsDisposed || _progLbl.IsDisposed)
            return;

        if (_progBar.InvokeRequired)
            _progBar.BeginInvoke(() => { _progBar.Value = val; });
        else
            _progBar.Value = val;

        if (msg != string.Empty)
        {
            if (_progLbl.InvokeRequired)
                _progLbl.BeginInvoke(() => { _progLbl.Text = msg; });
            else
                _progLbl.Text = msg;
        }
    }

    public void IncrementProgress(int val, string msg = "")
    {
        if (_progBar.IsDisposed || _progLbl.IsDisposed)
            return;

        if (_progBar.InvokeRequired)
            _progBar.BeginInvoke(() => { _progBar.Value += val; });
        else
            _progBar.Value += val;

        if (msg != string.Empty)
        {
            if (_progLbl.InvokeRequired)
                _progLbl.BeginInvoke(() => { _progLbl.Text = msg; });
            else
                _progLbl.Text = msg;
        }
    }
}