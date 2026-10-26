using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace FlatbufferToolkit.UI.Controls;

internal sealed class HexCanvas(HexView owner) : Control
{
    internal const double RowHeight = 20;
    private Typeface Font => new(owner.FontFamily);
    internal double CharacterWidth => MakeText("0", Brushes.Gray).Width;

    private FormattedText MakeText(string text, IBrush brush)
    {
        return new(text, CultureInfo.InvariantCulture, FlowDirection.LeftToRight, Font, owner.FontSize, brush);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        bool dark = ActualThemeVariant == Avalonia.Styling.ThemeVariant.Dark;
        IBrush foreground = dark ? Brushes.WhiteSmoke : Brushes.Black;
        var selection = owner.TryFindResource("ToolkitSelection", owner.ActualThemeVariant, out var selectedBrush)
            && selectedBrush is IBrush selectionBrush
            ? selectionBrush : Brushes.SteelBlue;
        context.DrawRectangle(owner.Background ?? Brushes.Transparent, null, Bounds.WithX(0).WithY(0));
        double characterWidth = CharacterWidth;
        void DrawText(string text, double x, double y, IBrush? brush = null)
        {
            context.DrawText(MakeText(text, brush ?? foreground), new Point(12 + x * characterWidth, y + 2));
        }

        DrawText("Offset", 0, 0, Brushes.Gray);
        for (int cell = 0; cell < 16; cell++)
        {
            DrawText(cell.ToString("X2"), 10 + cell * 3, 0, Brushes.Gray);
        }

        DrawText("ASCII", 60, 0, Brushes.Gray);
        int rows = Math.Max(0, (int)Math.Ceiling(Bounds.Height / RowHeight) - 1);
        for (int row = 0; row < rows; row++)
        {
            long start = (owner.FirstRow + row) * 16;
            if (start >= owner.Buffer.LongLength)
            {
                break;
            }

            double y = (row + 1) * RowHeight;
            DrawText(start.ToString("X8"), 0, y, Brushes.Gray);
            for (int cell = 0; cell < 16 && start + cell < owner.Buffer.LongLength; cell++)
            {
                long index = start + cell;
                bool selected = owner.SelectionLength > 0
                    && index >= owner.SelectionStart
                    && index < owner.SelectionStart + owner.SelectionLength;
                var highlight = owner.FindHighlight(index);
                if (selected || highlight != null)
                {
                    IBrush brush = selected ? selection : new SolidColorBrush(Color.Parse(highlight!.Color), 0.35);
                    context.DrawRectangle(brush, null, new Rect(12 + (10 + cell * 3) * characterWidth, y, characterWidth * 2.8, RowHeight));
                    context.DrawRectangle(brush, null, new Rect(12 + (60 + cell) * characterWidth, y, characterWidth, RowHeight));
                }

                byte value = owner.Buffer[index];
                var ink = selected ? Brushes.White : foreground;
                DrawText(value.ToString("X2"), 10 + cell * 3, y, ink);
                DrawText(value is >= 32 and <= 126 ? ((char)value).ToString() : ".", 60 + cell, y, ink);
            }
        }

        if (owner.Buffer.Length == 0)
        {
            var message = MakeText("Empty buffer", Brushes.Gray);
            var position = new Point(
                (Bounds.Width - message.Width) / 2,
                (Bounds.Height - message.Height) / 2);
            context.DrawText(message, position);
        }
    }
}
