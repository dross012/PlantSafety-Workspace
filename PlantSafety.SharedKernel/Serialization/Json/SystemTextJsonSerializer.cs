/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-29
    FILE:               SystemTextJsonSerializer.cs
    DESCRIPTION:        Default JSON serializer implementation backed by System.Text.Json. Deterministic and
                        infrastructure-agnostic. Additive-only.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

using System.Text.Json;
using PlantSafety.SharedKernel.Serialization.Abstractions;

namespace PlantSafety.SharedKernel.Serialization.Json;

public sealed class SystemTextJsonSerializer : ISerializer
{
    private static readonly JsonSerializerOptions _options = new(JsonSerializerDefaults.Web);

    public byte[] Serialize<T>(T value)
        => JsonSerializer.SerializeToUtf8Bytes(value, _options);

    public T? Deserialize<T>(byte[] data)
        => JsonSerializer.Deserialize<T>(data, _options);

    public object? Deserialize(byte[] data, Type type)
        => JsonSerializer.Deserialize(data, type, _options);
}
