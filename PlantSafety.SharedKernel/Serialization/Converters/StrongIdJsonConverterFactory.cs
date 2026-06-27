/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-25
    FILE:               StrongIdJsonConverterFactory.cs
    DESCRIPTION:        Reflection-safe factory for creating StrongId JSON converters. Uses constructor injection for
                        deterministic StrongId creation.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

using System;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using PlantSafety.SharedKernel.Primitives.StrongIds;

namespace PlantSafety.SharedKernel.Serialization.Converters;

public sealed class StrongIdJsonConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
        => typeof(IStrongId).IsAssignableFrom(typeToConvert);

    public override JsonConverter CreateConverter(Type type, JsonSerializerOptions options)
    {
        var ctor = type.GetConstructor(new[] { typeof(Guid) })
            ?? throw new InvalidOperationException(
                $"StrongId type '{type.Name}' must have a constructor accepting Guid.");

        var factory = typeof(StrongIdJsonConverterFactory)
            .GetMethod(nameof(CreateInternal), BindingFlags.NonPublic | BindingFlags.Static)!
            .MakeGenericMethod(type);

        return (JsonConverter)factory.Invoke(null, new object[] { ctor })!;
    }

    private static JsonConverter CreateInternal<TStrongId>(ConstructorInfo ctor)
        where TStrongId : IStrongId
    {
        return new StrongIdJsonConverter<TStrongId>(guid =>
            (TStrongId)ctor.Invoke(new object[] { guid }));
    }
}
