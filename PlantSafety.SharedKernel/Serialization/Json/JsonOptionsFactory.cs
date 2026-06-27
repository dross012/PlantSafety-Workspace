/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-25
    FILE:               JsonOptionsFactory.cs
    DESCRIPTION:        Canonical JSON options factory for the SharedKernel. Registers converters, naming policies,
                        ignore rules, and source-generated context.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

using System.Text.Json;
using System.Text.Json.Serialization;
using PlantSafety.SharedKernel.Serialization.Converters;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.SharedKernel.Primitives.StrongIds;

namespace PlantSafety.SharedKernel.Serialization.Json;

public static class JsonOptionsFactory
{
    public static JsonSerializerOptions Create()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false
        };

        // StrongId support
        options.Converters.Add(new StrongIdJsonConverterFactory());

        // ValueTypes
        options.Converters.Add(new UtcTimestampJsonConverter());
        options.Converters.Add(new NonEmptyStringJsonConverter());

        // Enum as strings
        options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));

        // Source-generated context
        options.TypeInfoResolverChain.Insert(0, SharedKernelJsonContext.Default);

        return options;
    }
}
