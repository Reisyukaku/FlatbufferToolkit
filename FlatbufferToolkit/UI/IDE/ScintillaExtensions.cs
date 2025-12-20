using ScintillaNET;

namespace FlatbufferToolkit.UI.IDE;

public static class ScintillaExtensions
{
    private static readonly string[] Keywords =
    [
        "namespace", "table", "struct", "enum", "union", "root_type",
        "file_extension", "file_identifier", "attribute", "rpc_service",
        "include", "native_include"
    ];

    private static readonly string[] Types =
    [
        "bool", "byte", "ubyte", "short", "ushort", "int", "uint",
        "float", "long", "ulong", "double", "int8", "uint8", "int16",
        "uint16", "int32", "uint32", "int64", "uint64", "float32",
        "float64", "string"
    ];

    public static void InitFbsLexer(this Scintilla scintilla)
    {
        scintilla.LexerName = "cpp";
        var styles = scintilla.Styles;

        // Scintilla uses its own styling system; WinForms BackColor/ForeColor alone won't
        // affect the editor surface. Configure STYLE_DEFAULT then clear all styles.
        var editorBackColor = Application.IsDarkModeEnabled ? SystemColors.ControlLight : SystemColors.ControlLightLight;
        var editorForeColor = SystemColors.ControlText;

        // Reset all styles
        scintilla.StyleResetDefault();
        styles[Style.Default].BackColor = editorBackColor;
        styles[Style.Default].ForeColor = editorForeColor;
        styles[Style.Default].Font = "Consolas";
        styles[Style.Default].Size = 12;
        scintilla.StyleClearAll();

        // Configure styles
        styles[NativeMethods.SCE_C_COMMENT].ForeColor = Application.IsDarkModeEnabled ? Color.LightGreen : Color.Green;
        styles[NativeMethods.SCE_C_COMMENT].Italic = true;

        styles[NativeMethods.SCE_C_COMMENTLINE].ForeColor = Application.IsDarkModeEnabled ? Color.LightGreen : Color.Green;
        styles[NativeMethods.SCE_C_COMMENTLINE].Italic = true;

        styles[NativeMethods.SCE_C_WORD].ForeColor = Application.IsDarkModeEnabled ? Color.LightBlue : Color.Blue;
        styles[NativeMethods.SCE_C_WORD].Bold = true;

        styles[NativeMethods.SCE_C_WORD2].ForeColor = Application.IsDarkModeEnabled ? Color.Cyan : Color.DarkCyan;
        styles[NativeMethods.SCE_C_WORD2].Bold = true;

        styles[NativeMethods.SCE_C_STRING].ForeColor = Application.IsDarkModeEnabled ? Color.RosyBrown : Color.Brown;

        styles[NativeMethods.SCE_C_NUMBER].ForeColor = Application.IsDarkModeEnabled ? Color.Coral : Color.Red;

        styles[NativeMethods.SCE_C_OPERATOR].ForeColor = Color.Gray;
        styles[NativeMethods.SCE_C_OPERATOR].Bold = true;

        styles[NativeMethods.SCE_C_PREPROCESSOR].ForeColor = Application.IsDarkModeEnabled ? Color.MediumPurple : Color.DarkMagenta;

        // Set keywords for autocomplete (optional)
        scintilla.SetKeywords(0, string.Join(" ", Keywords));
        scintilla.SetKeywords(1, string.Join(" ", Types));
        styles[Style.LineNumber].Font = "Consolas";
        styles[Style.LineNumber].Size = 8;
        if (Application.IsDarkModeEnabled)
        {
            scintilla.CaretForeColor = editorForeColor;
            scintilla.SelectionBackColor = SystemColors.Highlight;
            scintilla.SelectionTextColor = SystemColors.HighlightText;
            styles[Style.LineNumber].ForeColor = Color.Gray;
            styles[Style.LineNumber].BackColor = SystemColors.ControlDark;
        }
    }

    private static void OnCharAdded(object? sender, CharAddedEventArgs e)
    {
        var scintilla = (Scintilla)sender!;

        // Auto-close brackets, braces, quotes
        var closingChar = e.Char switch
        {
            '{' => '}',
            '[' => ']',
            '(' => ')',
            '"' => '"',
            '\'' => '\'',
            _ => '\0'
        };

        if (closingChar != '\0')
        {
            var currentPos = scintilla.CurrentPosition;
            scintilla.InsertText(currentPos, closingChar.ToString());
            scintilla.CurrentPosition = currentPos;
            scintilla.SelectionStart = currentPos;
            scintilla.SelectionEnd = currentPos;
        }
    }

    public static void SetupAutoComplete(this Scintilla scintilla)
    { 
        scintilla.CharAdded += OnCharAdded;
    }

    public static void ShowLineNumbers(this Scintilla scintilla, bool show)
    {
        scintilla.Margins[0].Type = MarginType.Number;
        scintilla.Margins[0].Width = show ? 25 : 0;
    }

    public static string GetTextSafe(this Scintilla scintilla)
    {
        return scintilla.InvokeRequired ? scintilla.Invoke(() => scintilla.GetTextRange(0, scintilla.TextLength)) : scintilla.GetTextRange(0, scintilla.TextLength);
    }
}