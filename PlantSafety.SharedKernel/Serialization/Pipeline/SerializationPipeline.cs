/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-29
    FILE:               SerializationPipeline.cs
    DESCRIPTION:        Multi-format serialization orchestrator. Resolves serializers via the registry and provides a
                        unified API for serialization and deserialization across formats.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

using PlantSafety.SharedKernel.Serialization.Abstractions;
using PlantSafety.SharedKernel.Serialization.Registry;

namespace PlantSafety.SharedKernel.Serialization.Pipeline;

public sealed class SerializationPipeline
{
    private readonly string _defaultFormat;

    public SerializationPipeline(string defaultFormat = "json")
    {
        _defaultFormat = defaultFormat;
    }

    private ISerializer Resolve(string? format)
    {
        if (!string.IsNullOrWhiteSpace(format))
            return SerializerRegistry.Get(format);

        return SerializerRegistry.Get(_defaultFormat);
    }

    /// <summary>
    /// Serializes a value using the specified or default format.
    /// </summary>
    public byte[] Serialize<T>(T value, string? format = null)
        => Resolve(format).Serialize(value);

    /// <summary>
    /// Deserializes a value using the specified or default format.
    /// </summary>
    public T? Deserialize<T>(byte[] data, string? format = null)
        => Resolve(format).Deserialize<T>(data);

    /// <summary>
    /// Deserializes a value using the specified or default format.
    /// </summary>
    public object? Deserialize(byte[] data, Type type, string? format = null)
        => Resolve(format).Deserialize(data, type);
}
