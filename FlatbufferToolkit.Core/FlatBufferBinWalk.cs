using FlatbufferToolkit.Utils;
using System.Text;

namespace FlatbufferToolkit;

public class FlatBufferBinWalk : IDisposable
{
    private readonly BinaryReader _reader;
    private readonly Schema _schema;
    private readonly IProgress<ParseProgress>? _progress;
    public List<InspectionNode> Nodes
    {
        get;
    } = [];
    public List<ByteHighlight> Highlights
    {
        get;
    } = [];

    private readonly int _bufferSize;
    private readonly Dictionary<string, StructDef> _structDefCache = new();
    private readonly Dictionary<string, EnumDef> _enumDefCache = new();
    private EnumDef? GetEnumDef(string enumName)
    {
        if (string.IsNullOrEmpty(enumName))
        {
            return null;
        }

        if (_enumDefCache.TryGetValue(enumName, out var def))
        {
            return def;
        }

        def = _schema.Enums.Values.FirstOrDefault(e => e.Name == enumName || e.Name.EndsWith("." + enumName));
        if (def != null)
        {
            _enumDefCache[enumName] = def;
        }

        return def;
    }

    private StructDef? GetStructDef(string structName)
    {
        if (string.IsNullOrEmpty(structName))
        {
            return null;
        }

        if (_structDefCache.TryGetValue(structName, out var def))
        {
            return def;
        }

        def = _schema.Structs.Values.FirstOrDefault(s => s.Name == structName || s.Name.EndsWith("." + structName));
        if (def != null)
        {
            _structDefCache[structName] = def;
        }

        return def;
    }

    public FlatBufferBinWalk(byte[] buffer, Schema schema, IProgress<ParseProgress>? progress = null)
    {
        _reader = new BinaryReaderTracked(new MemoryStream(buffer), Highlights, progress);
        _schema = schema;
        _progress = progress;
        _bufferSize = buffer.Length;
    }

    public Dictionary<string, object>? ReadRoot()
    {
        _progress?.Report(new ParseProgress(0, "Parsing binary"));
        Nodes.Clear();
        Highlights.Clear();
        if (string.IsNullOrWhiteSpace(_schema.RootType))
        {
            throw new Exception("No root_type specified in schema");
        }

        Dictionary<string, object>? root;
        try
        {
            var rootStruct = _schema.Structs[_schema.RootType];
            var rootOffset = ReadUIntAt(0);
            var rootNode = new InspectionNode(_schema.RootType)
            {
                Offset = rootOffset
            };
            root = ReadTable(rootStruct, rootOffset, rootNode, 0);
            Nodes.Add(rootNode);
        }
        catch (Exception ex)
        {
            root = null;
            Logger.Instance.Log(LogLevel.ERROR, ex.Message);
        }

        _progress?.Report(new ParseProgress(1, "Done"));
        return root;
    }

    private Dictionary<string, object> ReadTable(StructDef structDef, long offset, InspectionNode thisNode, long referencePos)
    {
        var result = new Dictionary<string, object>();
        //Read vtable
        var vtableOffset = offset - ReadIntAt(offset);
        var vtableSize = ReadUShortAt(vtableOffset);
        var tblSize = ReadUShortAt(vtableOffset + 2);
        thisNode.ToolTipText = $"Addr: 0x{referencePos:X4} | Table: 0x{offset:X4} | VTable: 0x{vtableOffset:X4} | Elements: {(vtableSize - 4) / 2}";
        foreach (var field in structDef.Fields)
        {
            if (field.Deprecated)
            {
                continue;
            }

            var fieldVtableOffset = 4 + field.Id * 2;
            if (fieldVtableOffset >= vtableSize)
            {
                continue;
            }

            var fieldOffset = ReadUShortAt(vtableOffset + fieldVtableOffset);
            if (fieldOffset == 0)
            {
                result[field.Name] = GetDefaultValue(field);
                continue;
            }

            InspectionNode elem = new InspectionNode
            {
                Offset = offset + fieldOffset,
                ToolTipText = $"Addr: 0x{offset + fieldOffset:X4}"
            };
            result[field.Name] = ReadFieldAt(field, offset + fieldOffset, elem);
            thisNode.Children.Add(elem);
        }

        return result;
    }

    private Dictionary<string, object> ReadStruct(StructDef structDef, long offset, InspectionNode thisNode)
    {
        var result = new Dictionary<string, object>();
        var pos = offset;
        foreach (var field in structDef.Fields)
        {
            var layout = GetInlineLayout(field.Type);
            pos = offset + Align(pos - offset, layout.Alignment);
            if (field.Deprecated)
            {
                pos += layout.Size;
                continue;
            }

            InspectionNode elem = new InspectionNode
            {
                Offset = pos,
                ToolTipText = $"Addr: 0x{pos:X4}"
            };
            result[field.Name] = ReadFieldAt(field, pos, elem);
            thisNode.Children.Add(elem);
            pos += layout.Size;
        }

        return result;
    }

    private object ReadFieldAt(FieldDef field, long pos, InspectionNode node)
    {
        Seek(pos);
        object? val = null;
        if (field.Type.IsScalar || field.Type.IsString)
        {
            node.Text = $"{field.Name}:";
        }

        switch (field.Type.BaseType)
        {
            case BaseType.Bool:
                val = _reader.ReadByte() != 0;
                break;
            case BaseType.Byte:
                val = _reader.ReadSByte();
                break;
            case BaseType.UByte:
                val = _reader.ReadByte();
                break;
            case BaseType.Short:
                val = _reader.ReadInt16();
                break;
            case BaseType.UShort:
                val = _reader.ReadUInt16();
                break;
            case BaseType.Int:
                val = _reader.ReadInt32();
                break;
            case BaseType.UInt:
                val = _reader.ReadUInt32();
                break;
            case BaseType.Long:
                val = _reader.ReadInt64();
                break;
            case BaseType.ULong:
                val = _reader.ReadUInt64();
                break;
            case BaseType.Float:
                val = _reader.ReadSingle();
                break;
            case BaseType.Double:
                val = _reader.ReadDouble();
                break;
            case BaseType.String:
                val = ReadStringAt(pos, node);
                node.Text += $"\"{val}\"";
                break;
            case BaseType.Vector:
                var typeName = field.Type.ElementType == BaseType.Obj ? field.Type.StructName : field.Type.ElementType.ToString();
                node.Text = $"{field.Name}:Vector<{typeName}>";
                val = ReadVectorAt(field, pos, node);
                break;
            case BaseType.Obj:
                node.Text = field.Type.StructName;
                var structDef = GetStructDef(field.Type.StructName);
                if (structDef != null)
                {
                    if (structDef.IsStruct)
                    {
                        val = ReadStruct(structDef, pos, node);
                    }
                    else
                    {
                        val = ReadTable(structDef, pos + ReadUIntAt(pos), node, pos);
                    }
                }

                break;
        }

        if (field.Type.IsScalar && val != null)
        {
            if (!string.IsNullOrEmpty(field.Type.EnumName))
            {
                var enumDef = GetEnumDef(field.Type.EnumName);
                if (enumDef != null)
                {
                    try
                    {
                        long longVal = Convert.ToInt64(val);
                        var ev = enumDef.Values.FirstOrDefault(v => v.Value == longVal);
                        if (ev != null)
                        {
                            val = ev.Name;
                        }
                    }
                    catch
                    {
                    }
                }
            }

            node.Text += val.ToString();
        }

        ArgumentNullException.ThrowIfNull(val);
        return val;
    }

    private object ReadVectorAt(FieldDef field, long pos, InspectionNode thisNode)
    {
        var vectorOffset = pos + ReadUIntAt(pos);
        var length = ReadIntAt(vectorOffset);
        if (length < 0 || length > _bufferSize)
        {
            throw new InvalidDataException("Invalid vector length.");
        }

        var dataOffset = vectorOffset + 4;
        thisNode.ToolTipText = $"Addr: 0x{pos:X4} | Vector: 0x{vectorOffset:X4} | Data: 0x{dataOffset:X4} | Elements: {length}";
        var list = new List<object>();
        var elemSize = GetInlineLayout(new FbsType
        {
            BaseType = field.Type.ElementType,
            StructName = field.Type.StructName
        }).Size;
        FieldDef? elemField = null;
        if (field.Type.ElementType != BaseType.Obj)
        {
            elemField = new FieldDef
            {
                Type = new FbsType
                {
                    BaseType = field.Type.ElementType,
                    StructName = field.Type.StructName
                }
            };
        }

        for (int i = 0; i < length; i++)
        {
            var elemPos = dataOffset + i * elemSize;
            var child = new InspectionNode(field.Type.StructName)
            {
                Offset = elemPos,
                ToolTipText = $"Addr: 0x{elemPos:X4}"
            };
            if (field.Type.ElementType == BaseType.Obj)
            {
                var structDef = GetStructDef(field.Type.StructName);
                object? val = null;
                if (structDef != null)
                {
                    if (structDef.IsStruct)
                    {
                        val = ReadStruct(structDef, elemPos, child);
                    }
                    else
                    {
                        val = ReadTable(structDef, elemPos + ReadUIntAt(elemPos), child, elemPos);
                    }
                }

                ArgumentNullException.ThrowIfNull(val);
                list.Add(val);
            }
            else
            {
                list.Add(ReadFieldAt(elemField!, elemPos, child));
            }

            thisNode.Children.Add(child);
        }

        return list;
    }

    private string ReadStringAt(long pos, InspectionNode node)
    {
        var stringOffset = pos + ReadUIntAt(pos);
        var length = ReadIntAt(stringOffset);
        node.ToolTipText = $"Addr: 0x{pos:X4} | String: 0x{stringOffset:X4} | Data: 0x{stringOffset + 4:X4} | Bytes: {length}";
        Seek(stringOffset + 4);
        return Encoding.UTF8.GetString(_reader.ReadBytes(length));
    }

    private static object GetDefaultValue(FieldDef field)
    {
        if (!string.IsNullOrEmpty(field.DefaultValue))
        {
            switch (field.Type.BaseType)
            {
                case BaseType.Bool:
                    return field.DefaultValue != "0";
                case BaseType.Byte:
                case BaseType.Short:
                case BaseType.Int:
                case BaseType.Long:
                    return long.Parse(field.DefaultValue);
                case BaseType.UByte:
                case BaseType.UShort:
                case BaseType.UInt:
                case BaseType.ULong:
                    return ulong.Parse(field.DefaultValue);
                case BaseType.Float:
                    return float.Parse(field.DefaultValue);
                case BaseType.Double:
                    return double.Parse(field.DefaultValue);
                case BaseType.String:
                    return field.DefaultValue;
            }
        }

        return field.Type.BaseType switch
        {
            BaseType.Bool => false,
            BaseType.Byte or BaseType.Short or BaseType.Int or BaseType.Long => 0L,
            BaseType.UByte or BaseType.UShort or BaseType.UInt or BaseType.ULong => 0UL,
            BaseType.Float => 0.0f,
            BaseType.Double => 0.0,
            BaseType.String => string.Empty,
            _ => throw new Exception("No default value for this type"),
        };
    }

    private static long Align(long value, int alignment)
    {
        return (value + alignment - 1) / alignment * alignment;
    }

    private (int Size, int Alignment) GetInlineLayout(FbsType type)
    {
        if (type.BaseType == BaseType.Obj)
        {
            var def = GetStructDef(type.StructName) ?? throw new InvalidOperationException($"Unknown type: {type.StructName}");
            if (def.IsStruct)
            {
                long size = 0;
                int alignment = Math.Max(1, def.MinAlign);
                if (def.Attributes.TryGetValue("force_align", out var forcedAlignment))
                {
                    alignment = Math.Max(alignment, int.Parse(forcedAlignment));
                }

                foreach (var field in def.Fields)
                {
                    var member = GetInlineLayout(field.Type);
                    alignment = Math.Max(alignment, member.Alignment);
                    size = Align(size, member.Alignment) + member.Size;
                }

                return (checked((int)Align(size, alignment)), alignment);
            }
        }

        if (type.BaseType == BaseType.Array)
        {
            var element = GetInlineLayout(new FbsType
            {
                BaseType = type.ElementType,
                StructName = type.StructName
            });
            return (checked(element.Size * type.ArrayLength), element.Alignment);
        }

        int scalarSize = GetTypeSize(type.BaseType);
        return (scalarSize, scalarSize);
    }

    private static int GetTypeSize(BaseType type)
    {
        return type switch
        {
            BaseType.Bool or BaseType.Byte or BaseType.UByte => 1,
            BaseType.Short or BaseType.UShort => 2,
            BaseType.Int or BaseType.UInt or BaseType.Float => 4,
            BaseType.Long or BaseType.ULong or BaseType.Double => 8,
            BaseType.String or BaseType.Vector or BaseType.Obj => 4,
            _ => 0
        };
    }

    public void Dispose()
    {
        _reader.Dispose();
    }

    private void Seek(long pos)
    {
        if (pos < 0 || pos > _bufferSize)
        {
            throw new InvalidDataException($"Offset 0x{pos:X} is outside the buffer.");
        }

        _reader.BaseStream.Position = pos;
    }

    private short ReadShortAt(long pos)
    {
        Seek(pos);
        return _reader.ReadInt16();
    }

    private ushort ReadUShortAt(long pos)
    {
        Seek(pos);
        return _reader.ReadUInt16();
    }

    private int ReadIntAt(long pos)
    {
        Seek(pos);
        return _reader.ReadInt32();
    }

    private uint ReadUIntAt(long pos)
    {
        Seek(pos);
        return _reader.ReadUInt32();
    }

    private long ReadLongAt(long pos)
    {
        Seek(pos);
        return _reader.ReadInt64();
    }

    private ulong ReadULongAt(long pos)
    {
        Seek(pos);
        return _reader.ReadUInt64();
    }
}

public class BinaryReaderTracked : BinaryReader
{
    private readonly List<ByteHighlight> _highlights;
    private readonly IProgress<ParseProgress>? _progress;
    private long _readBytes;
    private int _lastPercent = -1;
    public BinaryReaderTracked(Stream input, List<ByteHighlight> highlights, IProgress<ParseProgress>? progress = null) : base(input)
    {
        _highlights = highlights;
        _progress = progress;
    }

    private bool HasOverlap(List<ByteHighlight> regions, long offset, int length)
    {
        long start = offset;
        long end = offset + length;
        int lo = 0;
        int hi = regions.Count - 1;
        while (lo <= hi)
        {
            int mid = (lo + hi) / 2;
            var r = regions[mid];
            long rStart = r.Start;
            long rEnd = r.Start + r.Length;
            if (end <= rStart)
            {
                hi = mid - 1;
            }
            else if (start >= rEnd)
            {
                lo = mid + 1;
            }
            else
            {
                return true;
            }
        }

        return false;
    }

    private T Track<T>(int size, string col, Func<T> read)
    {
        long pos = BaseStream.Position;
        if (size > BaseStream.Length - pos)
        {
            throw new EndOfStreamException("Value extends beyond the buffer.");
        }

        if (!HasOverlap(_highlights, pos, size)) //TODO: vtables get reused in vectors at least; also probably best to track separates as a bitfield
        {
            _highlights.Add(new ByteHighlight(pos, size, col));
            _highlights.Sort((a, b) => a.Start.CompareTo(b.Start));
            _readBytes += size;
            int percent = (int)Math.Min(100, 100 * _readBytes / Math.Max(1, BaseStream.Length));
            if (percent != _lastPercent)
            {
                _lastPercent = percent;
                _progress?.Report(new ParseProgress(percent / 100d, "Parsing binary"));
            }
        }

        return read();
    }

    public override byte ReadByte()
    {
        return Track(1, "#40986C", base.ReadByte);
    }

    public override sbyte ReadSByte()
    {
        return Track(1, "#40986C", base.ReadSByte);
    }

    public override short ReadInt16()
    {
        return Track(2, "#409DA8", base.ReadInt16);
    }

    public override ushort ReadUInt16()
    {
        return Track(2, "#409DA8", base.ReadUInt16);
    }

    public override int ReadInt32()
    {
        return Track(4, "#A56AC7", base.ReadInt32);
    }

    public override uint ReadUInt32()
    {
        return Track(4, "#A56AC7", base.ReadUInt32);
    }

    public override long ReadInt64()
    {
        return Track(8, "#D79038", base.ReadInt64);
    }

    public override ulong ReadUInt64()
    {
        return Track(8, "#D79038", base.ReadUInt64);
    }

    public override float ReadSingle()
    {
        return Track(4, "#DB6868", base.ReadSingle);
    }

    public override double ReadDouble()
    {
        return Track(8, "#CD65A5", base.ReadDouble);
    }

    public override byte[] ReadBytes(int count)
    {
        long pos = BaseStream.Position;
        var bytes = base.ReadBytes(count);
        if (bytes.Length != count)
        {
            throw new EndOfStreamException("String extends beyond the buffer.");
        }

        _highlights.Add(new ByteHighlight(pos, count, "#48B6B0"));
        _highlights.Sort((a, b) => a.Start.CompareTo(b.Start));
        return bytes;
    }
}
