/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-29
    FILE:               ProtobufSerializer.cs
    DESCRIPTION:        Protobuf serializer using protobuf-net. Deterministic, AOT-safe, and infrastructure-agnostic.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

using PlantSafety.SharedKernel.Serialization.Abstractions;
using ProtoBuf;

namespace PlantSafety.SharedKernel.Serialization.Protobuf;

public sealed class ProtobufSerializer : ISerializer
{
    public byte[] Serialize<T>(T value)
    {
        using var ms = new MemoryStream();
        Serializer.Serialize(ms, value);
        return ms.ToArray();
    }

    public T? Deserialize<T>(byte[] data)
    {
        using var ms = new MemoryStream(data);
        return Serializer.Deserialize<T>(ms);
    }

    public object? Deserialize(byte[] data, Type type)
    {
        using var ms = new MemoryStream(data);
        return Serializer.NonGeneric.Deserialize(type, ms);
    }
}
