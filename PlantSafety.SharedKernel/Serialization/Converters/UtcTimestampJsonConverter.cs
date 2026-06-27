/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-25
    FILE:               UtcTimestampJsonConverter.cs
    DESCRIPTION:        JSON converter for UtcTimestamp. Ensures ISO-8601 round-trip formatting and UTC normalization.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using PlantSafety.SharedKernel.Primitives.ValueTypes;

namespace PlantSafety.SharedKernel.Serialization.Converters;

public sealed class UtcTimestampJsonConverter : JsonConverter<UtcTimestamp>
{
    public override UtcTimestamp Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var dto = reader.GetDateTimeOffset();
        return new UtcTimestamp(dto.UtcDateTime);
    }

    public override void Write(Utf8JsonWriter writer, UtcTimestamp value, JsonSerializerOptions options)
    {
        if (value.IsEmpty)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStringValue(value.Value);
    }
}
