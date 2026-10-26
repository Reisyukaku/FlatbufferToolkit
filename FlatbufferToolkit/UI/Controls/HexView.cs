using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Input.Platform;

namespace FlatbufferToolkit.UI.Controls;

public sealed class HexView : UserControl
{
    private readonly HexCanvas _canvas;
    private readonly ScrollBar _scroll = new()
    {
        Orientation = Avalonia.Layout.Orientation.Vertical,
        SmallChange = 1
    };
    private byte[] _buffer = [];
    private long _anchor;
    private long _caret;
    private bool _hasSelection;
    private bool _lowNibble;
    private long[] _highlightEnds = [];
    public event EventHandler? SelectionChanged;
    public event EventHandler? BufferChanged;
    public event EventHandler? OpenSelectionRequested;
    public IReadOnlyList<ByteHighlight> Highlights
    {
        get;
        private set;
    } = [];
    public long SelectionStart => Math.Min(_anchor, _caret);
    public long SelectionLength => _hasSelection ? Math.Abs(_caret - _anchor) + 1 : 0;
    public byte[] Buffer => _buffer;
    public long FirstRow => (long)_scroll.Value;

    public HexView()
    {
        _canvas = new HexCanvas(this)
        {
            Focusable = true,
            ClipToBounds = true
        };
        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,Auto")
        };
        grid.Children.Add(_canvas);
        Grid.SetColumn(_scroll, 1);
        grid.Children.Add(_scroll);
        Content = grid;
        ActualThemeVariantChanged += (_, _) => _canvas.InvalidateVisual();
        PropertyChanged += (_, e) =>
        {
            if (e.Property == BackgroundProperty
                || e.Property == FontFamilyProperty || e.Property == FontSizeProperty)
            {
                _canvas.InvalidateVisual();
            }
        };
        _scroll.ValueChanged += (_, _) => _canvas.InvalidateVisual();
        _canvas.SizeChanged += (_, _) => UpdateScroll();
        var copy = new MenuItem
        {
            Header = "Copy hex"
        };
        copy.Click += async (_, _) => await CopyAsync();
        var select = new MenuItem
        {
            Header = "Select all"
        };
        select.Click += (_, _) => Select(0, _buffer.LongLength);
        var open = new MenuItem
        {
            Header = "Open selection in new tab"
        };
        open.Click += (_, _) => OpenSelectionRequested?.Invoke(this, EventArgs.Empty);
        _canvas.ContextMenu = new ContextMenu
        {
            ItemsSource = new[]
            {
                copy,
                select,
                open
            }
        };
        _canvas.PointerPressed += (_, e) =>
        {
            if (!e.GetCurrentPoint(_canvas).Properties.IsLeftButtonPressed)
            {
                return;
            }

            _canvas.Focus();
            if (HitTest(e.GetPosition(_canvas)) is not { } offset)
            {
                return;
            }

            if (!e.KeyModifiers.HasFlag(KeyModifiers.Shift) || !_hasSelection)
            {
                _anchor = offset;
            }

            _caret = offset;
            _hasSelection = true;
            _lowNibble = false;
            e.Pointer.Capture(_canvas);
            Changed();
            e.Handled = true;
        };
        _canvas.PointerMoved += (_, e) =>
        {
            if (e.Pointer.Captured != _canvas || !e.GetCurrentPoint(_canvas).Properties.IsLeftButtonPressed)
            {
                return;
            }

            var point = e.GetPosition(_canvas);
            if (point.Y < HexCanvas.RowHeight)
            {
                _scroll.Value--;
            }
            else if (point.Y > _canvas.Bounds.Height - HexCanvas.RowHeight)
            {
                _scroll.Value++;
            }

            if (HitTest(point) is { } offset)
            {
                _caret = offset;
                Changed();
            }
        };
        _canvas.PointerReleased += (_, e) => e.Pointer.Capture(null);
        _canvas.PointerWheelChanged += (_, e) =>
        {
            _scroll.Value -= e.Delta.Y * 3;
            e.Handled = true;
        };
        _canvas.KeyDown += OnKeyDown;
        _canvas.TextInput += (_, e) =>
        {
            if (!_hasSelection || e.Text is not { Length: 1 } text || !Uri.IsHexDigit(text[0]))
            {
                return;
            }

            int digit = Convert.ToInt32(text, 16);
            int index = (int)_caret;
            _buffer[index] = _lowNibble ? (byte)((_buffer[index] & 0xf0) | digit) : (byte)((digit << 4) | (_buffer[index] & 0x0f));
            if (_lowNibble)
            {
                _caret = Math.Min(_buffer.LongLength - 1, _caret + 1);
            }

            _anchor = _caret;
            _lowNibble = !_lowNibble;
            Highlights = [];
            BufferChanged?.Invoke(this, EventArgs.Empty);
            EnsureVisible();
            Changed();
            e.Handled = true;
        };
    }

    public void Load(byte[] buffer)
    {
        _buffer = buffer;
        _anchor = _caret = 0;
        _hasSelection = false;
        _lowNibble = false;
        _scroll.Value = 0;
        Highlights = [];
        UpdateScroll();
        Changed();
    }

    public void SetHighlights(IReadOnlyList<ByteHighlight> highlights)
    {
        Highlights = highlights.OrderBy(h => h.Start).ToArray();
        _highlightEnds = new long[Highlights.Count];
        long end = 0;
        for (int i = 0; i < Highlights.Count; i++)
        {
            _highlightEnds[i] = end = Math.Max(end, Highlights[i].Start + Highlights[i].Length);
        }

        _canvas.InvalidateVisual();
    }

    public void Select(long start, long length)
    {
        if (start < 0 || length < 0 || start > _buffer.LongLength || length > _buffer.LongLength - start)
        {
            throw new ArgumentOutOfRangeException(nameof(start));
        }

        _anchor = start;
        _caret = length == 0 ? start : start + length - 1;
        _hasSelection = length > 0;
        _lowNibble = false;
        EnsureVisible();
        Changed();
    }

    public byte[] SelectedBytes()
    {
        return _buffer.AsSpan((int)SelectionStart, (int)SelectionLength).ToArray();
    }

    private async Task CopyAsync()
    {
        if (TopLevel.GetTopLevel(this)?.Clipboard is { } clipboard && SelectionLength > 0)
        {
            await clipboard.SetTextAsync(BitConverter.ToString(SelectedBytes()).Replace("-", " "));
        }
    }

    private async void OnKeyDown(object? sender, KeyEventArgs e)
    {
        bool command = (e.KeyModifiers & (KeyModifiers.Control | KeyModifiers.Meta)) != 0;
        if (command && e.Key == Key.C)
        {
            await CopyAsync();
            e.Handled = true;
            return;
        }

        if (command && e.Key == Key.A)
        {
            Select(0, _buffer.LongLength);
            e.Handled = true;
            return;
        }

        if (_buffer.Length == 0)
        {
            return;
        }

        long? next = e.Key switch
        {
            Key.Left => _caret - 1,
            Key.Right => _caret + 1,
            Key.Up => _caret - 16,
            Key.Down => _caret + 16,
            Key.PageUp => _caret - VisibleRows * 16,
            Key.PageDown => _caret + VisibleRows * 16,
            Key.Home => command ? 0 : _caret / 16 * 16,
            Key.End => command ? _buffer.LongLength - 1 : _caret / 16 * 16 + 15,
            _ => null
        };
        if (!next.HasValue)
        {
            return;
        }

        _caret = Math.Clamp(next.Value, 0, _buffer.LongLength - 1);
        if (!e.KeyModifiers.HasFlag(KeyModifiers.Shift) || !_hasSelection)
        {
            _anchor = _caret;
        }

        _hasSelection = true;
        _lowNibble = false;
        EnsureVisible();
        Changed();
        e.Handled = true;
    }

    private int VisibleRows => Math.Max(1, (int)(_canvas.Bounds.Height / HexCanvas.RowHeight) - 1);

    private void UpdateScroll()
    {
        _scroll.Maximum = Math.Max(0, (_buffer.LongLength + 15) / 16 - VisibleRows);
        _scroll.ViewportSize = VisibleRows;
        _scroll.LargeChange = VisibleRows;
        _canvas.InvalidateVisual();
    }

    private void EnsureVisible()
    {
        if (!_hasSelection)
        {
            return;
        }

        long row = _caret / 16;
        if (row < FirstRow)
        {
            _scroll.Value = row;
        }
        else if (row >= FirstRow + VisibleRows)
        {
            _scroll.Value = row - VisibleRows + 1;
        }
    }

    private void Changed()
    {
        _canvas.InvalidateVisual();
        SelectionChanged?.Invoke(this, EventArgs.Empty);
    }

    private long? HitTest(Point point)
    {
        double width = _canvas.CharacterWidth;
        double column = (point.X - 12) / width;
        int cell = column >= 60 ? (int)(column - 60) : (int)((column - 10) / 3);
        if (column < 10 || cell is < 0 or > 15 || point.Y < HexCanvas.RowHeight)
        {
            return null;
        }

        long offset = (FirstRow + (long)(point.Y / HexCanvas.RowHeight) - 1) * 16 + cell;
        return offset < _buffer.LongLength ? offset : null;
    }

    internal ByteHighlight? FindHighlight(long offset)
    {
        int lo = 0, hi = Highlights.Count;
        while (lo < hi)
        {
            int mid = lo + (hi - lo) / 2;
            if (Highlights[mid].Start <= offset)
            {
                lo = mid + 1;
            }
            else
            {
                hi = mid;
            }
        }

        for (int i = lo - 1; i >= 0 && _highlightEnds[i] > offset; i--)
        {
            if (offset - Highlights[i].Start < Highlights[i].Length)
            {
                return Highlights[i];
            }
        }

        return null;
    }
}
