namespace FlatbufferToolkit;

internal static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    private static void Main()
    {
        var args = Environment.GetCommandLineArgs();
        if (args.Any(z => z.EndsWith("-dark")))
            Application.SetColorMode(SystemColorMode.Dark);
        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
    }
}