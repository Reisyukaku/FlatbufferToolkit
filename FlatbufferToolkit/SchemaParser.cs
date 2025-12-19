using FlatbufferToolkit.UI;
using FlatbufferToolkit.UI.IDE;
using FlatbufferToolkit.Utils;
using ScintillaNET;
using System.Text;

namespace FlatbufferToolkit;
// ===== Type System =====

public enum BaseType
{
    None,
    Bool,
    Byte,
    UByte,
    Short,
    UShort,
    Int,
    UInt,
    Long,
    ULong,
    Float,
    Double,
    String,
    Vector,
    Array,
    Obj,      // Tables & Structs
    Union
}

public class FbsType
{
    public BaseType BaseType { get; set; }
    public BaseType ElementType { get; set; }
    public string StructName { get; set; }
    public string EnumName { get; set; }
    public int ArrayLength { get; set; }

    public bool IsScalar => BaseType is (>= BaseType.Bool and <= BaseType.Double);
    public bool IsVector => BaseType == BaseType.Vector;
    public bool IsStruct => BaseType == BaseType.Obj;
    public bool IsString => BaseType == BaseType.String;
}

// ===== Schema Definitions =====

public class FieldDef
{
    public string Name { get; set; }
    public FbsType Type { get; set; }
    public string DefaultValue { get; set; }
    public int Id { get; set; }
    public int Offset { get; set; }
    public bool Deprecated { get; set; }
    public bool Required { get; set; }
    public bool Key { get; set; }
    public Dictionary<string, string> Attributes { get; set; } = new();
}

public class StructDef
{
    public string Name { get; set; }
    public List<FieldDef> Fields { get; set; } = [];
    public bool IsStruct { get; set; }
    public int MinAlign { get; set; }
    public Dictionary<string, string> Attributes { get; set; } = new();
}

public class EnumVal
{
    public string Name { get; set; }
    public long Value { get; set; }
    public FbsType UnionType { get; set; }
}

public class EnumDef
{
    public string Name { get; set; }
    public BaseType UnderlyingType { get; set; }
    public List<EnumVal> Values { get; set; } = [];
    public bool IsUnion { get; set; }
    public Dictionary<string, string> Attributes { get; set; } = new();
}

public class Schema
{
    public Dictionary<string, StructDef> Structs { get; set; } = new();
    public Dictionary<string, EnumDef> Enums { get; set; } = new();
    public string RootType { get; set; }
    public string FileIdentifier { get; set; }
    public string FileExtension { get; set; }
}

// ===== IDL Parser =====

public class SchemaParser
{
    private StreamReader _stream;
    private ulong _line = 1;

    private Schema _schema = new();
    private string _currentNamespace = "";

    public Schema? Parse(Scintilla fbsContent)
    {
        Progress.Instance.Setup(fbsContent.Lines.Count, "Parsing schema");
        _stream = new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(fbsContent.GetTextSafe())));
        _schema = new Schema();

        try
        {
            while (!IsEof())
            {
                ParseTopLevel();
            }
            Progress.Instance.SetProgress(fbsContent.Lines.Count, "Done");
            return _schema;
        }
        catch (Exception ex)
        {
            Logger.Instance.Log(LogLevel.ERROR, ex.Message);
            return null;
        }
    }

    private void ParseTopLevel()
    {
        SkipWhitespaceAndComments();
        if (IsEof()) return;

        if (IsKeyword("namespace"))
        {
            ParseNamespace();
        }
        else if (IsKeyword("table"))
        {
            ParseTable(false);
        }
        else if (IsKeyword("struct"))
        {
            ParseTable(true);
        }
        else if (IsKeyword("enum"))
        {
            ParseEnum(false);
        }
        else if (IsKeyword("union"))
        {
            ParseEnum(true);
        }
        else if (IsKeyword("root_type"))
        {
            Next();
            var identif = ParseIdentifier();
            _schema.RootType = _currentNamespace != string.Empty ? $"{_currentNamespace}.{identif}" : identif;
            Expect(';');
        }
        else if (IsKeyword("file_identifier"))
        {
            Next();
            _schema.FileIdentifier = ParseStringConstant();
            Expect(';');
        }
        else if (IsKeyword("file_extension"))
        {
            Next();
            _schema.FileExtension = ParseStringConstant();
            Expect(';');
        }
        else if (IsKeyword("attribute"))
        {
            Next();
            ParseStringConstant(); // Just skip for now
            Expect(';');
        }
        else if (IsKeyword("include"))
        {
            Next();
            ParseStringConstant(); // Just skip for now
            Expect(';');
        }
        else if (IsKeyword("rpc_service"))
        {
            SkipUntil('}');
            Expect('}');
        }
        else
        {
            Next(); // Skip unknown
        }
    }

    private void ParseNamespace()
    {
        Next();
        _currentNamespace = ParseIdentifier();
        while (Peek() == '.')
        {
            Next();
            _currentNamespace += "." + ParseIdentifier();
        }
        Expect(';');
    }

    private void ParseTable(bool isStruct)
    {
        Next();
        var structDef = new StructDef
        {
            Name = ParseIdentifier(),
            IsStruct = isStruct
        };

        ParseMetadata(structDef.Attributes);
        Expect('{');
        SkipWhitespaceAndComments();

        int fieldId = 0;
        while (Peek() != '}')
        {
            if (IsEof()) throw new Exception("Unexpected EOF in table");

            var field = new FieldDef
            {
                Name = ParseIdentifier(),
                Id = fieldId++
            };

            SkipWhitespaceAndComments();
            Expect(':');
            SkipWhitespaceAndComments();

            field.Type = ParseType();

            // Default value
            if (Peek() == '=')
            {
                Next();
                field.DefaultValue = ParseScalarValue();
            }

            ParseMetadata(field.Attributes);

            // Check for metadata attributes
            if (field.Attributes.TryGetValue("id", out var attribute))
            {
                field.Id = int.Parse(attribute);
            }
            field.Deprecated = field.Attributes.ContainsKey("deprecated");
            field.Required = field.Attributes.ContainsKey("required");
            field.Key = field.Attributes.ContainsKey("key");

            Expect(';');
            structDef.Fields.Add(field);
            SkipWhitespaceAndComments();
        }

        Expect('}');

        var fullName = string.IsNullOrEmpty(_currentNamespace)
            ? structDef.Name
            : $"{_currentNamespace}.{structDef.Name}";
        _schema.Structs[fullName] = structDef;
    }

    private void ParseEnum(bool isUnion)
    {
        Next();
        var enumDef = new EnumDef
        {
            Name = ParseIdentifier(),
            IsUnion = isUnion,
            UnderlyingType = BaseType.Int
        };

        SkipWhitespaceAndComments();
        if (Peek() == ':')
        {
            Next();
            var typeName = ParseIdentifier();
            enumDef.UnderlyingType = ParseBaseTypeName(typeName);
        }

        ParseMetadata(enumDef.Attributes);
        Expect('{');

        long currentValue = 0;
        while (Peek() != '}')
        {
            if (IsEof()) throw new Exception("Unexpected EOF in enum");

            var enumVal = new EnumVal
            {
                Name = ParseIdentifier(),
                Value = currentValue
            };

            SkipWhitespaceAndComments();

            if (isUnion && Peek() == ':')
            {
                Next();
                enumVal.UnionType = ParseType();
            }

            SkipWhitespaceAndComments();

            if (Peek() == '=')
            {
                Next();
                var valStr = ParseScalarValue();
                enumVal.Value = long.Parse(valStr);
            }

            currentValue = enumVal.Value + 1;
            enumDef.Values.Add(enumVal);

            if (Peek() == ',') Next();

            SkipWhitespaceAndComments();
        }

        Expect('}');

        var fullName = string.IsNullOrEmpty(_currentNamespace)
            ? enumDef.Name
            : $"{_currentNamespace}.{enumDef.Name}";
        _schema.Enums[fullName] = enumDef;
    }

    private FbsType ParseType()
    {
        var type = new FbsType();

        if (Peek() == '[')
        {
            Next();
            var elementType = ParseType();

            if (Peek() == ':')
            {
                Next();
                type.ArrayLength = int.Parse(ParseScalarValue());
                type.BaseType = BaseType.Array;
            }
            else
            {
                type.BaseType = BaseType.Vector;
            }

            type.ElementType = elementType.BaseType;
            type.StructName = elementType.StructName;
            type.EnumName = elementType.EnumName;
            Expect(']');
        }
        else
        {
            var typeName = ParseIdentifier();
            type.BaseType = ParseBaseTypeName(typeName);

            if (type.BaseType == BaseType.Obj)
            {
                type.StructName = typeName;
            }
            else if (type.BaseType == BaseType.None)
            {
                // Might be enum or custom type
                type.EnumName = typeName;
                type.BaseType = BaseType.Int; // Default for enums
            }
        }

        return type;
    }

    private BaseType ParseBaseTypeName(string name)
    {
        return name switch
        {
            "bool" => BaseType.Bool,
            "byte" or "int8" => BaseType.Byte,
            "ubyte" or "uint8" => BaseType.UByte,
            "short" or "int16" => BaseType.Short,
            "ushort" or "uint16" => BaseType.UShort,
            "int" or "int32" => BaseType.Int,
            "uint" or "uint32" => BaseType.UInt,
            "long" or "int64" => BaseType.Long,
            "ulong" or "uint64" => BaseType.ULong,
            "float" or "float32" => BaseType.Float,
            "double" or "float64" => BaseType.Double,
            "string" => BaseType.String,
            _ => BaseType.Obj // Assume struct/table
        };
    }

    private void ParseMetadata(Dictionary<string, string> attrs)
    {
        if (Peek() == '(')
        {
            Next();
            while (Peek() != ')')
            {
                var key = ParseIdentifier();
                string value = "true";

                if (Peek() == ':')
                {
                    Next();
                    value = ParseScalarValue();
                }

                attrs[key] = value;

                if (Peek() == ',') Next();
            }
            Expect(')');
        }
    }

    private string ParseScalarValue()
    {
        SkipWhitespaceAndComments();

        if (char.IsDigit(Peek()) || Peek() == '-' || Peek() == '+')
        {
            var sb = new StringBuilder();
            while (char.IsDigit(Peek()) || Peek() == '.' ||
                   Peek() == '-' || Peek() == '+' ||
                   Peek() == 'e' || Peek() == 'E' || Peek() == 'x')
            {
                sb.Append(Peek());
                Next();
            }
            return sb.ToString();
        }
        else if (Peek() == '"')
        {
            return ParseStringConstant();
        }
        else
        {
            return ParseIdentifier();
        }
    }

    private string ParseStringConstant()
    {
        Expect('"');
        var sb = new StringBuilder();
        while (Peek() != '"')
        {
            if (Peek() == '\\')
            {
                Next();
                sb.Append(Peek());
            }
            else
            {
                sb.Append(Peek());
            }
            Next();
        }
        Expect('"');
        return sb.ToString();
    }

    private string ParseIdentifier()
    {
        SkipWhitespaceAndComments();
        var sb = new StringBuilder();

        if (!char.IsLetter(Peek()) && Peek() != '_')
            throw new Exception($"Expected identifier at line {_line}");

        while (char.IsLetterOrDigit(Peek()) || Peek() == '_')
        {
            sb.Append(Peek());
            Next();
        }

        return sb.ToString();
    }

    private bool IsKeyword(string keyword)
    {
        SkipWhitespaceAndComments();
        var saved = _stream.BaseStream.Position;

        foreach (var ch in keyword)
        {
            if (Peek() != ch)
            {
                _stream.BaseStream.Position = saved;
                return false;
            }
            Next();
        }

        if (char.IsLetterOrDigit(Peek()) || Peek() == '_')
        {
            _stream.BaseStream.Position = saved;
            return false;
        }

        _stream.BaseStream.Position = saved;
        return true;
    }

    private void Expect(char expected)
    {
        SkipWhitespaceAndComments();
        if (Peek() != expected)
            throw new Exception($"Expected '{expected}' but got '{Peek()}' at line {_line}");
        Next();
    }

    private void SkipUntil(char ch)
    {
        while (!IsEof() && Peek() != ch)
        {
            Next();
        }
    }

    private void SkipWhitespaceAndComments()
    {
        while (!IsEof())
        {
            if (char.IsWhiteSpace(Peek()))
            {
                if (Peek() == '\n') _line++;
                Next();
            }
            else if (Peek() == '/' && Peek() == '/')
            {
                while (!IsEof() && Peek() != '\n') Next();
            }
            else if (Peek() == '/' && Peek() == '*')
            {
                Next(); Next();
                while (!IsEof() && !(Peek() == '*' && Peek() == '/'))
                {
                    if (Peek() == '\n') _line++;
                    Next();
                }
                if (!IsEof()) { Next(); Next(); }
            }
            else
            {
                break;
            }
        }
    }

    private char Peek() => (char)_stream.Peek();
    private void Next() { _stream.Read(); Progress.Instance.SetProgress((int)_line); }
    private bool IsEof() => _stream.EndOfStream;
}