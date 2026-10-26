using System.Buffers.Binary;
using System.Globalization;
using Avalonia.Controls;
using FlatbufferToolkit.UI.Models;

namespace FlatbufferToolkit.UI.Controls;

public partial class DataInspector : UserControl
{
    public DataInspector()
    {
        InitializeComponent();
        Inspect([]);
    }

    public void Inspect(ReadOnlySpan<byte> bytes)
    {
        Values.ItemsSource = new InspectorValue[]
        {
            new("U8", bytes.Length >= 1 ? Format(bytes[0]) : ""),
            new("S8", bytes.Length >= 1 ? Format(unchecked((sbyte)bytes[0])) : ""),
            new("U16", bytes.Length >= 2 ? Format(BinaryPrimitives.ReadUInt16LittleEndian(bytes)) : ""),
            new("S16", bytes.Length >= 2 ? Format(BinaryPrimitives.ReadInt16LittleEndian(bytes)) : ""),
            new("U32", bytes.Length >= 4 ? Format(BinaryPrimitives.ReadUInt32LittleEndian(bytes)) : ""),
            new("S32", bytes.Length >= 4 ? Format(BinaryPrimitives.ReadInt32LittleEndian(bytes)) : ""),
            new("U64", bytes.Length >= 8 ? Format(BinaryPrimitives.ReadUInt64LittleEndian(bytes)) : ""),
            new("S64", bytes.Length >= 8 ? Format(BinaryPrimitives.ReadInt64LittleEndian(bytes)) : ""),
            new("Float", bytes.Length >= 4 ? Format(BinaryPrimitives.ReadSingleLittleEndian(bytes)) : ""),
            new("Double", bytes.Length >= 8 ? Format(BinaryPrimitives.ReadDoubleLittleEndian(bytes)) : "")
        };
    }

    private static string Format<T>(T value) where T : IFormattable
    {
        return value.ToString(null, CultureInfo.InvariantCulture);
    }
}
