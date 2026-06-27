/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-25
    FILE:               StrongIdJsonConverter.cs
    DESCRIPTION:        Generic JSON converter for StrongId<TStrongId>. Converts StrongId to/from Guid. Used by the
                        StrongIdJsonConverterFactory.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using PlantSafety.SharedKernel.Primitives.StrongIds;

namespace PlantSafety.SharedKernel.Serialization.Converters;

public sealed class StrongIdJsonConverter<TStrongId> : JsonConverter<TStrongId>
    where TStrongId : IStrongId
{
    private readonly Func<Guid, TStrongId> _factory;

    public StrongIdJsonConverter(Func<Guid, TStrongId> factory)
    {
        _factory = factory;
    }

    public override TStrongId? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var guid = reader.GetGuid();
        return _factory(guid);
    }

    public override void Write(Utf8JsonWriter writer, TStrongId value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Value);
    }
}
