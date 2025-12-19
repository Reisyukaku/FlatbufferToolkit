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

        // Reset all styles
        scintilla.StyleResetDefault();
        scintilla.Styles[NativeMethods.SCE_C_DEFAULT].Font = "Consolas";
        scintilla.Styles[NativeMethods.SCE_C_DEFAULT].Size = 12;
        scintilla.StyleClearAll();

        // Configure styles
        scintilla.Styles[NativeMethods.SCE_C_DEFAULT].ForeColor = Color.Black;

        scintilla.Styles[NativeMethods.SCE_C_COMMENT].ForeColor = Color.Green;
        scintilla.Styles[NativeMethods.SCE_C_COMMENT].Italic = true;

        scintilla.Styles[NativeMethods.SCE_C_COMMENTLINE].ForeColor = Color.Green;
        scintilla.Styles[NativeMethods.SCE_C_COMMENTLINE].Italic = true;

        scintilla.Styles[NativeMethods.SCE_C_WORD].ForeColor = Color.Blue;
        scintilla.Styles[NativeMethods.SCE_C_WORD].Bold = true;

        scintilla.Styles[NativeMethods.SCE_C_WORD2].ForeColor = Color.DarkCyan;
        scintilla.Styles[NativeMethods.SCE_C_WORD2].Bold = true;

        scintilla.Styles[NativeMethods.SCE_C_STRING].ForeColor = Color.Brown;

        scintilla.Styles[NativeMethods.SCE_C_NUMBER].ForeColor = Color.Red;

        scintilla.Styles[NativeMethods.SCE_C_OPERATOR].ForeColor = Color.DarkGray;
        scintilla.Styles[NativeMethods.SCE_C_OPERATOR].Bold = true;

        scintilla.Styles[NativeMethods.SCE_C_IDENTIFIER].ForeColor = Color.Black;

        scintilla.Styles[NativeMethods.SCE_C_PREPROCESSOR].ForeColor = Color.DarkMagenta;

        // Set keywords for autocomplete (optional)
        scintilla.SetKeywords(0, string.Join(" ", Keywords));
        scintilla.SetKeywords(1, string.Join(" ", Types));
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