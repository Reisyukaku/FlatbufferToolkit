using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using FlatbufferToolkit.MCP;
using FlatbufferToolkit.UI;
using FlatbufferToolkit.Utils;

namespace FlatbufferToolkit.UI.Views;

public partial class MainWindow : Window
{
    private readonly ObservableCollection<TabItem> _tabs = [];
    private bool _closed;
    public EditorView? CurrentEditor => (Tabs.SelectedItem as TabItem)?.Content as EditorView;

    public MainWindow()
    {
        InitializeComponent();
        Tabs.ItemsSource = _tabs;
        Logger.Instance.MessageLogged += OnLog;
        DragDrop.SetAllowDrop(this, true);
        AddHandler(DragDrop.DragOverEvent, OnDragOver);
        AddHandler(DragDrop.DropEvent, OnDrop);
        KeyDown += OnKeyDown;
        Opened += (_, _) => McpServerManager.Instance.Start();
        Closed += (_, _) =>
        {
            _closed = true;
            Logger.Instance.MessageLogged -= OnLog;
            McpServerManager.Instance.Stop();
            var manager = McpToolManager.Instance;
            manager.ReadHexProvider = null;
            manager.ReadSchemaProvider = null;
            manager.WriteSchemaProvider = null;
            manager.BufferSnapshotProvider = null;
        };
        var tools = McpToolManager.Instance;
        tools.ReadHexProvider = (offset, length) => OnUi(() => RequireEditor().ReadHex(offset, length));
        tools.ReadSchemaProvider = () => OnUi(() => RequireEditor().ReadSchema());
        tools.WriteSchemaProvider = text => OnUi(() =>
        {
            RequireEditor().WriteSchema(text);
            return true;
        });
        tools.BufferSnapshotProvider = () => OnUi(() => RequireEditor().GetBufferSnapshot());
    }

    private static T OnUi<T>(Func<T> action)
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            return action();
        }

        return Dispatcher.UIThread.InvokeAsync(action).GetAwaiter().GetResult();
    }

    private EditorView RequireEditor()
    {
        return CurrentEditor ?? throw new InvalidOperationException("No active binary buffer.");
    }

    public void OpenBuffer(FileAccessor file)
    {
        var editor = new EditorView();
        editor.LoadFile(file);
        editor.StatusChanged += (_, _) => UpdateStatus();
        editor.NewBufferRequested += (_, buffer) => OpenBuffer(buffer);
        var tab = new TabItem
        {
            Content = editor
        };
        var close = new Button
        {
            Content = "×"
        };
        close.Classes.Add("tab-close");
        close.Click += (_, _) => CloseTab(tab);
        var header = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal
        };
        var label = new TextBlock
        {
            Text = file.Name,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
        };
        label.Classes.Add("tab-label");
        header.Children.Add(label);
        header.Children.Add(close);
        tab.Header = header;
        _tabs.Add(tab);
        Tabs.SelectedItem = tab;
        EmptyEditor.IsVisible = false;
        Tabs.IsVisible = true;
        UpdateStatus();
    }

    private void CloseTab(TabItem tab)
    {
        if (tab.Content is EditorView { IsParsing: true })
        {
            return;
        }

        int index = _tabs.IndexOf(tab);
        _tabs.Remove(tab);
        if (_tabs.Count > 0 && Tabs.SelectedItem == null)
        {
            Tabs.SelectedIndex = Math.Min(index, _tabs.Count - 1);
        }

        EmptyEditor.IsVisible = _tabs.Count == 0;
        Tabs.IsVisible = _tabs.Count > 0;
        UpdateStatus();
    }

    private async Task OpenFilesAsync()
    {
        try
        {
            var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Open FlatBuffer binary",
                AllowMultiple = true
            });
            foreach (var file in files)
            {
                await OpenStorageFileAsync(file);
            }
        }
        catch (Exception ex)
        {
            Logger.Instance.Log(LogLevel.ERROR, ex.Message);
        }
    }

    private async Task OpenStorageFileAsync(IStorageFile file)
    {
        await using var stream = await file.OpenReadAsync();
        using var memory = new MemoryStream();
        await stream.CopyToAsync(memory);
        OpenBuffer(new FileAccessor(Path.GetFileNameWithoutExtension(file.Name), memory.ToArray()));
    }

    private static readonly FilePickerFileType SchemaType = new("FlatBuffers schema")
    {
        Patterns = ["*.fbs"]
    };
    private async Task SaveSchemaAsync()
    {
        var editor = CurrentEditor;
        if (editor == null)
        {
            return;
        }

        try
        {
            var file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Save schema",
                DefaultExtension = "fbs",
                SuggestedFileName = editor.FileName + ".fbs",
                FileTypeChoices = [SchemaType],
                ShowOverwritePrompt = true
            });
            if (file == null)
            {
                return;
            }

            await using var stream = await file.OpenWriteAsync();
            stream.SetLength(0);
            await using var writer = new StreamWriter(stream);
            await writer.WriteAsync(editor.ReadSchema());
        }
        catch (Exception ex)
        {
            Logger.Instance.Log(LogLevel.ERROR, ex.Message);
        }
    }

    private async Task RunAsync()
    {
        if (CurrentEditor is not { IsParsing: false } editor)
        {
            return;
        }

        Output.Text = "";
        var progress = new Progress<ParseProgress>(p =>
        {
            if (_closed)
            {
                return;
            }

            ProgressBar.Value = p.Fraction;
            ProgressText.Text = p.Message;
        });
        await editor.ParseAsync(progress);
    }

    private void UpdateStatus()
    {
        if (Status == null)
        {
            return;
        }

        Status.Text = CurrentEditor is { } editor
            ? $"Hex: 0x{editor.SelectionStart:X} | 0x{editor.SelectionLength:X} bytes    Text: Line {editor.CurrentLine}"
            : "Ready";
    }

    private void OnLog(LogLevel level, string line)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (_closed)
            {
                return;
            }

            string text = (Output.Text ?? "") + line;
            Output.Text = text.Length > 100_000 ? text[^100_000..] : text;
            Output.CaretIndex = Output.Text.Length;
        });
    }

    private void OnDragOver(object? sender, DragEventArgs e)
    {
        e.DragEffects = e.DataTransfer.Contains(DataFormat.File) ? DragDropEffects.Copy : DragDropEffects.None;
        e.Handled = true;
    }

    private async void OnDrop(object? sender, DragEventArgs e)
    {
        try
        {
            foreach (var file in e.DataTransfer.TryGetFiles() ?? [])
            {
                if (file is IStorageFile storageFile)
                {
                    await OpenStorageFileAsync(storageFile);
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Instance.Log(LogLevel.ERROR, ex.Message);
        }

        e.Handled = true;
    }

    private async void OnKeyDown(object? sender, KeyEventArgs e)
    {
        bool command = (e.KeyModifiers & (KeyModifiers.Control | KeyModifiers.Meta)) != 0;
        if (command && e.Key == Key.O)
        {
            await OpenFilesAsync();
        }
        else if (command && e.Key == Key.S)
        {
            await SaveSchemaAsync();
        }
        else if (command && e.Key == Key.W && Tabs.SelectedItem is TabItem tab)
        {
            CloseTab(tab);
        }
        else if (e.Key == Key.F5)
        {
            await RunAsync();
        }
        else
        {
            return;
        }

        e.Handled = true;
    }

    private async void OpenClick(object? sender, RoutedEventArgs e)
    {
        await OpenFilesAsync();
    }

    private async void SaveSchemaClick(object? sender, RoutedEventArgs e)
    {
        await SaveSchemaAsync();
    }

    private async void OpenSchemaClick(object? sender, RoutedEventArgs e)
    {
        if (CurrentEditor is not { IsParsing: false } editor)
        {
            return;
        }

        try
        {
            var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Open schema",
                FileTypeFilter = [SchemaType]
            });
            if (files.Count == 0)
            {
                return;
            }

            await using var stream = await files[0].OpenReadAsync();
            using var reader = new StreamReader(stream);
            editor.WriteSchema(await reader.ReadToEndAsync());
        }
        catch (Exception ex)
        {
            Logger.Instance.Log(LogLevel.ERROR, ex.Message);
        }
    }

    private async void RunClick(object? sender, RoutedEventArgs e)
    {
        await RunAsync();
    }

    private void CloseTabClick(object? sender, RoutedEventArgs e)
    {
        if (Tabs.SelectedItem is TabItem tab)
        {
            CloseTab(tab);
        }
    }

    private void ExitClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private void TabSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        UpdateStatus();
    }

    private void LineNumbersClick(object? sender, RoutedEventArgs e)
    {
        Settings.Instance.ShowTextNumbers = LineNumbersItem.IsChecked;
        EmptyEditor.UpdateSettings();
        foreach (var tab in _tabs)
        {
            ((EditorView)tab.Content!).UpdateSettings();
        }
    }

    private void InspectorClick(object? sender, RoutedEventArgs e)
    {
        Settings.Instance.ShowDataInspector = InspectorItem.IsChecked;
        EmptyEditor.UpdateSettings();
        foreach (var tab in _tabs)
        {
            ((EditorView)tab.Content!).UpdateSettings();
        }
    }
}
