using System.Buffers.Binary;
using System.Globalization;

namespace FlatbufferToolkit;

/// <summary>Inspects table layout without a schema. Interpretations are hypotheses, not detected types.</summary>
public static class FlatBufferInspector
{
    public static TableInspection InspectTable(byte[] buffer, long? offset = null, int startField = 0, int count = 64)
    {
        ArgumentNullException.ThrowIfNull(buffer);
        if (startField < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(startField));
        }

        if (count is < 1 or > 256)
        {
            throw new ArgumentOutOfRangeException(nameof(count), "Field count must be between 1 and 256.");
        }

        long tableOffset;
        if (offset.HasValue)
        {
            tableOffset = offset.Value;
        }
        else
        {
            RequireRange(buffer, 0, 4, "root reference");
            tableOffset = BinaryPrimitives.ReadUInt32LittleEndian(buffer);
            if (tableOffset < 4)
            {
                throw new InvalidDataException("Root reference must point beyond the root offset word.");
            }
        }

        RequireRange(buffer, tableOffset, 4, "table header");
        int displacement = BinaryPrimitives.ReadInt32LittleEndian(buffer.AsSpan((int)tableOffset, 4));
        long vtableOffset = tableOffset - displacement;
        RequireRange(buffer, vtableOffset, 4, "vtable header");
        int vtableSize = BinaryPrimitives.ReadUInt16LittleEndian(buffer.AsSpan((int)vtableOffset, 2));
        int tableSize = BinaryPrimitives.ReadUInt16LittleEndian(buffer.AsSpan((int)vtableOffset + 2, 2));
        if (vtableSize < 4 || vtableSize % 2 != 0)
        {
            throw new InvalidDataException($"Invalid vtable size {vtableSize} at 0x{vtableOffset:X}: expected an even size of at least 4.");
        }

        if (tableSize < 4)
        {
            throw new InvalidDataException($"Invalid table size {tableSize} at 0x{tableOffset:X}: expected at least 4.");
        }

        RequireRange(buffer, vtableOffset, vtableSize, "vtable");
        RequireRange(buffer, tableOffset, tableSize, "table");
        if (vtableOffset < tableOffset + tableSize && tableOffset < vtableOffset + vtableSize)
        {
            throw new InvalidDataException("The vtable overlaps the table's inline bytes.");
        }

        int fieldCount = (vtableSize - 4) / 2;
        var entries = new ushort[fieldCount];
        for (int i = 0; i < entries.Length; i++)
        {
            entries[i] = BinaryPrimitives.ReadUInt16LittleEndian(buffer.AsSpan((int)vtableOffset + 4 + 2 * i, 2));
            if (entries[i] != 0 && (entries[i] < 4 || entries[i] >= tableSize))
            {
                throw new InvalidDataException($"Field {i} has invalid table-relative offset {entries[i]} for table size {tableSize}.");
            }
        }

        // Field IDs need not follow physical order. Aliased slots share the same
        // upper bound; zero entries have no bytes or recoverable default value.
        var boundaries = entries.Where(e => e != 0).Select(e => (int)e).Distinct().Order().Append(tableSize).ToArray();
        var spans = new Dictionary<int, int>();
        for (int i = 0; i < boundaries.Length - 1; i++)
        {
            spans[boundaries[i]] = boundaries[i + 1] - boundaries[i];
        }

        if (startField > fieldCount)
        {
            throw new ArgumentOutOfRangeException(nameof(startField), $"Table has only {fieldCount} field slots.");
        }

        int endField = Math.Min(fieldCount, startField + count);
        var fields = new List<FieldInspection>();
        for (int id = startField; id < endField; id++)
        {
            int relativeOffset = entries[id];
            if (relativeOffset == 0)
            {
                fields.Add(new(id, false, 0, null, null, null, null, null));
                continue;
            }

            long address = tableOffset + relativeOffset;
            int available = spans[relativeOffset];
            var bytes = buffer.AsSpan((int)address, available);
            PossibleReference? reference = null;
            if (available >= 4)
            {
                uint relative = BinaryPrimitives.ReadUInt32LittleEndian(bytes);
                long target = address + relative;
                reference = new(relative, target, target >= 0 && target < buffer.LongLength, relative != 0);
            }

            fields.Add(new(id, true, relativeOffset, address, available, BitConverter.ToString(bytes[..Math.Min(16, available)].ToArray()).Replace('-', ' '), Interpret(bytes), reference));
        }

        return new(tableOffset, displacement, vtableOffset, vtableSize, tableSize, fieldCount, startField, endField < fieldCount ? endField : null, fields);
    }

    private static Dictionary<string, object> Interpret(ReadOnlySpan<byte> bytes)
    {
        var values = new Dictionary<string, object>
        {
            ["uint8"] = bytes[0],
            ["int8"] = unchecked((sbyte)bytes[0])
        };
        if (bytes.Length >= 2)
        {
            values["uint16"] = BinaryPrimitives.ReadUInt16LittleEndian(bytes);
            values["int16"] = BinaryPrimitives.ReadInt16LittleEndian(bytes);
        }

        if (bytes.Length >= 4)
        {
            values["uint32"] = BinaryPrimitives.ReadUInt32LittleEndian(bytes);
            values["int32"] = BinaryPrimitives.ReadInt32LittleEndian(bytes);
            float value = BinaryPrimitives.ReadSingleLittleEndian(bytes);
            values["float32"] = float.IsFinite(value) ? (object)value : value.ToString(CultureInfo.InvariantCulture);
        }

        if (bytes.Length >= 8)
        {
            values["uint64"] = BinaryPrimitives.ReadUInt64LittleEndian(bytes);
            values["int64"] = BinaryPrimitives.ReadInt64LittleEndian(bytes);
            double value = BinaryPrimitives.ReadDoubleLittleEndian(bytes);
            values["float64"] = double.IsFinite(value) ? (object)value : value.ToString(CultureInfo.InvariantCulture);
        }

        return values;
    }

    private static void RequireRange(byte[] buffer, long offset, int size, string description)
    {
        if (offset < 0 || offset > buffer.LongLength - size)
        {
            throw new InvalidDataException($"Cannot read {description} at 0x{offset:X} ({size} bytes): outside buffer of {buffer.LongLength} bytes.");
        }
    }
}

public sealed record TableInspection(long TableOffset, int VtableDisplacement, long VtableOffset, int VtableSize, int TableSize, int FieldCount, int StartField, int? NextField, IReadOnlyList<FieldInspection> Fields)
{
    public string InterpretationNote => "All addresses are absolute byte offsets in the current buffer. " + "availableInlineBytes is an upper bound that may include padding. rawHex previews at most 16 bytes. " + "Scalar interpretations and possibleReference are hypotheses, not detected types. " + "An in-bounds target does not prove a reference. Absent fields have no bytes; defaults require a schema. " + "Field slots beyond this vtable cannot be inferred.";
}

public sealed record FieldInspection(int FieldId, bool Present, int TableRelativeOffset, long? Address, int? AvailableInlineBytes, string? RawHex, IReadOnlyDictionary<string, object>? Interpretations, PossibleReference? PossibleReference);
public sealed record PossibleReference(uint Displacement, long TargetAddress, bool TargetInBounds, bool IsForward);
