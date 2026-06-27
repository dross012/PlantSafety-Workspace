/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-29
    FILE:               BinarySerializer.cs
    DESCRIPTION:        Deterministic binary serializer using BinaryWriter/BinaryReader. Intended for internal SharedKernel
                        primitives only. Not a general-purpose object graph serializer.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

using PlantSafety.SharedKernel.Serialization.Abstractions;

namespace PlantSafety.SharedKernel.Serialization.Binary;

public sealed class BinarySerializer : ISerializer
{
    public byte[] Serialize<T>(T value)
    {
        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms);

        WriteValue(writer, value!);
        return ms.ToArray();
    }

    public T? Deserialize<T>(byte[] data)
    {
        using var ms = new MemoryStream(data);
        using var reader = new BinaryReader(ms);

        return (T?)ReadValue(reader, typeof(T));
    }

    public object? Deserialize(byte[] data, Type type)
    {
        using var ms = new MemoryStream(data);
        using var reader = new BinaryReader(ms);

        return ReadValue(reader, type);
    }

    private static void WriteValue(BinaryWriter writer, object value)
    {
        switch (value)
        {
            case int i: writer.Write(i); break;
            case long l: writer.Write(l); break;
            case bool b: writer.Write(b); break;
            case string s: writer.Write(s); break;
            case Guid g: writer.Write(g.ToByteArray()); break;
            default:
                throw new NotSupportedException($"BinarySerializer does not support type {value.GetType().Name}.");
        }
    }

    private static object? ReadValue(BinaryReader reader, Type type)
    {
        if (type == typeof(int)) return reader.ReadInt32();
        if (type == typeof(long)) return reader.ReadInt64();
        if (type == typeof(bool)) return reader.ReadBoolean();
        if (type == typeof(string)) return reader.ReadString();
        if (type == typeof(Guid)) return new Guid(reader.ReadBytes(16));

        throw new NotSupportedException($"BinarySerializer does not support type {type.Name}.");
    }
}
