/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-29
    FILE:               YamlSerializer.cs
    DESCRIPTION:        YAML serializer using YamlDotNet. Deterministic, infrastructure-agnostic, and additive-only.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

using System.Text;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using PlantSafety.SharedKernel.Serialization.Abstractions;

namespace PlantSafety.SharedKernel.Serialization.Yaml;

public sealed class YamlSerializer : PlantSafety.SharedKernel.Serialization.Abstractions.ISerializer
{
    private static readonly YamlDotNet.Serialization.ISerializer _serializer =
        new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

    private static readonly IDeserializer _deserializer =
        new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

    public byte[] Serialize<T>(T value)
    {
        var yaml = _serializer.Serialize(value);
        return Encoding.UTF8.GetBytes(yaml);
    }

    public T? Deserialize<T>(byte[] data)
    {
        var yaml = Encoding.UTF8.GetString(data);
        return _deserializer.Deserialize<T>(yaml);
    }

    public object? Deserialize(byte[] data, Type type)
    {
        var yaml = Encoding.UTF8.GetString(data);
        return _deserializer.Deserialize(yaml, type);
    }
}
