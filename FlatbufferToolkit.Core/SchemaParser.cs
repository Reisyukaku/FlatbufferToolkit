using FlatbufferToolkit.Utils;
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
    Obj, // Tables & Structs
    Union
}

public class FbsType
{
    public BaseType BaseType
    {
        get;
        set;
    }
    public BaseType ElementType
    {
        get;
        set;
    }
    public string StructName
    {
        get;
        set;
    } = "";
    public string EnumName
    {
        get;
        set;
    } = "";
    public int ArrayLength
    {
        get;
        set;
    }
    public bool IsScalar => BaseType is (>= BaseType.Bool and <= BaseType.Double);
    public bool IsVector => BaseType == BaseType.Vector;
    public bool IsStruct => BaseType == BaseType.Obj;
    public bool IsString => BaseType == BaseType.String;
}

// ===== Schema Definitions =====
public class FieldDef
{
    public string Name
    {
        get;
        set;
    } = "";
    public FbsType Type
    {
        get;
        set;
    } = new();
    public string DefaultValue
    {
        get;
        set;
    } = "";
    public int Id
    {
        get;
        set;
    }
    public int Offset
    {
        get;
        set;
    }
    public bool Deprecated
    {
        get;
        set;
    }
    public bool Required
    {
        get;
        set;
    }
    public bool Key
    {
        get;
        set;
    }
    public Dictionary<string, string> Attributes
    {
        get;
        set;
    } = new();
}

public class StructDef
{
    public string Name
    {
        get;
        set;
    } = "";
    public List<FieldDef> Fields
    {
        get;
        set;
    } = [];
    public bool IsStruct
    {
        get;
        set;
    }
    public int MinAlign
    {
        get;
        set;
    }
    public Dictionary<string, string> Attributes
    {
        get;
        set;
    } = new();
}

public class EnumVal
{
    public string Name
    {
        get;
        set;
    } = "";
    public long Value
    {
        get;
        set;
    }
    public FbsType? UnionType
    {
        get;
        set;
    }
}

public class EnumDef
{
    public string Name
    {
        get;
        set;
    } = "";
    public BaseType UnderlyingType
    {
        get;
        set;
    }
    public List<EnumVal> Values
    {
        get;
        set;
    } = [];
    public bool IsUnion
    {
        get;
        set;
    }
    public Dictionary<string, string> Attributes
    {
        get;
        set;
    } = new();
}

public class Schema
{
    public Dictionary<string, StructDef> Structs
    {
        get;
        set;
    } = new();
    public Dictionary<string, EnumDef> Enums
    {
        get;
        set;
    } = new();
    public string RootType
    {
        get;
        set;
    } = "";
    public string FileIdentifier
    {
        get;
        set;
    } = "";
    public string FileExtension
    {
        get;
        set;
    } = "";
}

// ===== IDL Parser =====
public class SchemaParser
{
    private string _text = "";
    private int _position;
    private IProgress<ParseProgress>? _progress;
    private ulong _line = 1;
    private Schema _schema = new();
    private string _currentNamespace = "";
    public Schema? Parse(string fbsContent, IProgress<ParseProgress>? progress = null)
    {
        _text = fbsContent;
        _position = 0;
        _lastPercent = -1;
        _line = 1;
        _currentNamespace = "";
        _progress = progress;
        ReportProgress();
        _schema = new Schema();
        try
        {
            while (!IsEof())
            {
                ParseTopLevel();
            }

            foreach (var structDef in _schema.Structs.Values)
            {
                foreach (var field in structDef.Fields)
                {
                    if (field.Type.BaseType == BaseType.Obj)
                    {
                        var eDef = _schema.Enums.Values.FirstOrDefault(e => e.Name == field.Type.StructName || e.Name.EndsWith("." + field.Type.StructName));
                        if (eDef != null)
                        {
                            field.Type.BaseType = eDef.UnderlyingType;
                            field.Type.EnumName = eDef.Name;
                            field.Type.StructName = "";
                        }
                    }
                    else if (field.Type.BaseType == BaseType.Vector && field.Type.ElementType == BaseType.Obj)
                    {
                        var eDef = _schema.Enums.Values.FirstOrDefault(e => e.Name == field.Type.StructName || e.Name.EndsWith("." + field.Type.StructName));
                        if (eDef != null)
                        {
                            field.Type.ElementType = eDef.UnderlyingType;
                            field.Type.EnumName = eDef.Name;
                            field.Type.StructName = "";
                        }
                    }
                    else if (field.Type.BaseType == BaseType.Array && field.Type.ElementType == BaseType.Obj)
                    {
                        var eDef = _schema.Enums.Values.FirstOrDefault(e => e.Name == field.Type.StructName || e.Name.EndsWith("." + field.Type.StructName));
                        if (eDef != null)
                        {
                            field.Type.ElementType = eDef.UnderlyingType;
                            field.Type.EnumName = eDef.Name;
                            field.Type.StructName = "";
                        }
                    }
                }
            }

            progress?.Report(new ParseProgress(1, "Schema parsed"));
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
        if (IsEof())
        {
            return;
        }

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
            if (IsEof())
            {
                throw new Exception("Unexpected EOF in table");
            }

            var field = new FieldDef
            {
                Name = ParseIdentifier(),
                Id = fieldId++
            };
            SkipWhitespaceAndComments();
            Expect(':');
            SkipWhitespaceAndComments();
            field.Type = ParseType();
            SkipWhitespaceAndComments();
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
        var fullName = string.IsNullOrEmpty(_currentNamespace) ? structDef.Name : $"{_currentNamespace}.{structDef.Name}";
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
            if (IsEof())
            {
                throw new Exception("Unexpected EOF in enum");
            }

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
            if (Peek() == ',')
            {
                Next();
            }

            SkipWhitespaceAndComments();
        }

        Expect('}');
        var fullName = string.IsNullOrEmpty(_currentNamespace) ? enumDef.Name : $"{_currentNamespace}.{enumDef.Name}";
        _schema.Enums[fullName] = enumDef;
    }

    private FbsType ParseType()
    {
        var type = new FbsType();
        if (Peek() == '[')
        {
            Next();
            var elementType = ParseType();
            SkipWhitespaceAndComments();
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
        SkipWhitespaceAndComments();
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
                if (Peek() == ',')
                {
                    Next();
                }
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
            while (char.IsDigit(Peek()) || Peek() == '.' || Peek() == '-' || Peek() == '+' || Peek() == 'e' || Peek() == 'E' || Peek() == 'x')
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
            if (IsEof())
            {
                throw new FormatException($"Unterminated string at line {_line}");
            }

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
        {
            throw new Exception($"Expected identifier at line {_line}");
        }

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
        if (!_text.AsSpan(_position).StartsWith(keyword, StringComparison.Ordinal))
        {
            return false;
        }

        int end = _position + keyword.Length;
        if (end < _text.Length && (char.IsLetterOrDigit(_text[end]) || _text[end] == '_'))
        {
            return false;
        }

        _position = end - 1;
        return true;
    }

    private void Expect(char expected)
    {
        SkipWhitespaceAndComments();
        if (Peek() != expected)
        {
            throw new Exception($"Expected '{expected}' but got '{Peek()}' at line {_line}");
        }

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
                Next();
            }
            else if (Peek() == '/' && Peek(1) == '/')
            {
                while (!IsEof() && Peek() != '\n')
                {
                    Next();
                }
            }
            else if (Peek() == '/' && Peek(1) == '*')
            {
                Next();
                Next();
                while (!IsEof() && !(Peek() == '*' && Peek(1) == '/'))
                {
                    Next();
                }

                if (IsEof())
                {
                    throw new FormatException("Unterminated block comment");
                }

                Next();
                Next();
            }
            else
            {
                break;
            }
        }
    }

    private char Peek(int ahead = 0)
    {
        return _position + ahead < _text.Length ? _text[_position + ahead] : '\0';
    }

    private void Next()
    {
        if (IsEof())
        {
            return;
        }

        if (_text[_position++] == '\n')
        {
            _line++;
        }

        ReportProgress();
    }

    private int _lastPercent = -1;
    private void ReportProgress()
    {
        int percent = _text.Length == 0 ? 100 : (int)(100L * _position / _text.Length);
        if (percent == _lastPercent)
        {
            return;
        }

        _lastPercent = percent;
        _progress?.Report(new ParseProgress(percent / 100d, "Parsing schema"));
    }

    private bool IsEof()
    {
        return _position >= _text.Length;
    }
}
