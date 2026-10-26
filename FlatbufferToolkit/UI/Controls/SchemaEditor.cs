using System.Xml;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using AvaloniaEdit;
using AvaloniaEdit.Highlighting;
using AvaloniaEdit.Highlighting.Xshd;
using AvaloniaEdit.Indentation.CSharp;

namespace FlatbufferToolkit.UI.Controls;

public sealed class SchemaEditor : TextEditor
{
    protected override Type StyleKeyOverride => typeof(TextEditor);

    public SchemaEditor()
    {
        ShowLineNumbers = true;
        HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto;
        VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto;
        TextArea.IndentationStrategy = new CSharpIndentationStrategy(Options);
        Options.ConvertTabsToSpaces = true;
        Options.IndentationSize = 4;
        using var reader = XmlReader.Create(new StringReader(Highlighting));
        SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
        TextArea.TextEntering += (_, e) =>
        {
            if (e.Text is not { Length: 1 } text)
            {
                return;
            }

            char close = text[0] switch
            {
                '{' => '}',
                '[' => ']',
                '(' => ')',
                '"' => '"',
                _ => '\0'
            };
            if (close == '\0' || SelectionLength != 0)
            {
                return;
            }

            int offset = CaretOffset;
            Document.Insert(offset, text + close);
            CaretOffset = offset + 1;
            e.Handled = true;
        };
    }

    private const string Highlighting = """
        <SyntaxDefinition name="FlatBuffers" extensions=".fbs" xmlns="http://icsharpcode.net/sharpdevelop/syntaxdefinition/2008">
          <Color name="Comment" foreground="#6A9955" />
          <Color name="Keyword" foreground="#568BD5" fontWeight="bold" />
          <Color name="Type" foreground="#359DAB" />
          <Color name="String" foreground="#C57955" />
          <Color name="Number" foreground="#B68B47" />
          <RuleSet>
            <Span color="Comment" begin="//" />
            <Span color="Comment" multiline="true" begin="/\*" end="\*/" />
            <Span color="String" begin="&quot;" end="&quot;" />
            <Keywords color="Keyword">
              <Word>namespace</Word><Word>table</Word><Word>struct</Word><Word>enum</Word><Word>union</Word>
              <Word>root_type</Word><Word>file_identifier</Word><Word>file_extension</Word>
              <Word>attribute</Word><Word>include</Word><Word>native_include</Word><Word>rpc_service</Word>
            </Keywords>
            <Keywords color="Type">
              <Word>bool</Word><Word>byte</Word><Word>ubyte</Word><Word>short</Word><Word>ushort</Word>
              <Word>int</Word><Word>uint</Word><Word>long</Word><Word>ulong</Word><Word>float</Word>
              <Word>double</Word><Word>string</Word><Word>true</Word><Word>false</Word>
              <Word>int8</Word><Word>uint8</Word><Word>int16</Word><Word>uint16</Word>
              <Word>int32</Word><Word>uint32</Word><Word>int64</Word><Word>uint64</Word>
              <Word>float32</Word><Word>float64</Word>
            </Keywords>
            <Rule color="Number">\b(0[xX][0-9a-fA-F]+|\d+(\.\d+)?([eE][+-]?\d+)?)\b</Rule>
          </RuleSet>
        </SyntaxDefinition>
        """;
}
