/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-25
    FILE:               NonEmptyStringJsonConverter.cs
    DESCRIPTION:        JSON converter for NonEmptyString. Serializes as a raw string. Deserializes into trimmed value or
                        Empty sentinel.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using PlantSafety.SharedKernel.Primitives.ValueTypes;

namespace PlantSafety.SharedKernel.Serialization.Converters;

public sealed class NonEmptyStringJsonConverter : JsonConverter<NonEmptyString>
{
    public override NonEmptyString Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var raw = reader.GetString();
        return new NonEmptyString(raw ?? string.Empty);
    }

    public override void Write(Utf8JsonWriter writer, NonEmptyString value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Value);
    }
}
