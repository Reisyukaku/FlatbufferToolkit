namespace FlatbufferToolkit;

public sealed class InspectionNode(string? text = null)
{
    public string Text
    {
        get;
        set;
    } = text ?? "";
    public string ToolTipText
    {
        get;
        set;
    } = "";
    public long Offset
    {
        get;
        set;
    }
    public List<InspectionNode> Children
    {
        get;
    } = [];

    public override string ToString()
    {
        return Text;
    }
}

public sealed record ByteHighlight(long Start, int Length, string Color);
public sealed record ParseProgress(double Fraction, string Message);
