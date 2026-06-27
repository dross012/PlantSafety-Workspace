/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-29
    FILE:               SerializerRegistry.cs
    DESCRIPTION:        Central registry for all SharedKernel serializers. Provides deterministic lookup by format name
                        and additive-only registration for custom serializers.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

using System.Collections.Concurrent;
using PlantSafety.SharedKernel.Serialization.Abstractions;
using PlantSafety.SharedKernel.Serialization.Json;
using PlantSafety.SharedKernel.Serialization.Xml;
using PlantSafety.SharedKernel.Serialization.Bson;
using PlantSafety.SharedKernel.Serialization.MessagePack;
using PlantSafety.SharedKernel.Serialization.Protobuf;
using PlantSafety.SharedKernel.Serialization.Yaml;
using PlantSafety.SharedKernel.Serialization.Binary;

namespace PlantSafety.SharedKernel.Serialization.Registry;

public static class SerializerRegistry
{
    private static readonly ConcurrentDictionary<string, ISerializer> _serializers = new(
        StringComparer.OrdinalIgnoreCase);

    static SerializerRegistry()
    {
        // Built-in formats
        Register("json", new SystemTextJsonSerializer());
        Register("xml", new XmlSerializer());
        Register("bson", new BsonSerializer());
        Register("msgpack", new MessagePackSerializer());
        Register("protobuf", new ProtobufSerializer());
        Register("yaml", new YamlSerializer());
        Register("binary", new BinarySerializer());
    }

    /// <summary>
    /// Registers a serializer under a format name.
    /// </summary>
    public static void Register(string format, ISerializer serializer)
        => _serializers[format] = serializer;

    /// <summary>
    /// Retrieves a serializer by format name.
    /// </summary>
    public static ISerializer Get(string format)
    {
        if (_serializers.TryGetValue(format, out var serializer))
            return serializer;

        throw new InvalidOperationException(
            $"No serializer registered for format '{format}'.");
    }

    /// <summary>
    /// Returns all registered formats.
    /// </summary>
    public static IReadOnlyCollection<string> Formats
        => _serializers.Keys.ToArray();
}
