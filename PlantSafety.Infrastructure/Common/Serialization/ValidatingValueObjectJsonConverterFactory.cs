// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Common
//  Layer:          Infrastructure
//  File:           ValidatingValueObjectJsonConverterFactory.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Eventing & Messaging Agent -- regression audit)
//  SchemaVersion:  2026.1
//  Description:    JsonConverterFactory that recognises this codebase's pervasive "validating
//                  record struct" value-object convention (Handbook §2.8 Value Objects
//                  Architectural Rule -- e.g. PlantSafety.SharedKernel.Identifiers.CorrelationId,
//                  CausationId, UserId; PlantSafety.SharedKernel.Primitives.ValueTypes.
//                  UtcTimestamp, PerformedByActor; PlantSafety.Domain.RiskManagement.ValueObjects.
//                  Ids.RiskId, RiskScore; and every other module's XxxId/XxxScore-shaped value
//                  object) and reconstructs them via their validating constructor, instead of
//                  System.Text.Json's default object-converter behaviour.
//
//                  THE BUG THIS FIXES (2026-08-03 regression audit): every C# struct -- including
//                  `readonly record struct` -- always has an implicit PUBLIC parameterless
//                  constructor at the CLR level, even when the only *declared* constructor is
//                  private/validating (e.g. `RiskId(Guid value)`, `PerformedByActor(ActorKind,
//                  UserId?)`). System.Text.Json's default object converter prefers that
//                  parameterless constructor when one is public, then tries to populate public
//                  SETTABLE properties from the JSON object -- but every value object in this
//                  codebase exposes only get-only properties (Handbook §2.8 Rule 2 "Immutable").
//                  The net effect is NOT an exception: STJ silently constructs a zeroed/default
//                  instance (e.g. RiskId.Value == Guid.Empty, PerformedByActor.Kind == default
//                  ActorKind(0), UserId == null) and returns it -- meaning every domain event's
//                  RiskId/CorrelationId/CausationId/UtcTimestamp/PerformedByActor/etc. field
//                  round-trips through the Outbox (OutboxWriter.Enqueue -> OutboxProcessor.
//                  PublishAsync deserialize -> bus.Publish -> transport re-serialize -> consumer
//                  deserialize) as SILENTLY CORRUPTED DEFAULT DATA, not a thrown/logged failure.
//                  This was verified empirically (round-trip: {"value":"&lt;real-guid&gt;"} ->
//                  Value == Guid.Empty) before this converter was written. It affects the vast
//                  majority of domain events in the platform, since `PerformedByActor Actor` alone
//                  appears in 500+ domain-event-adjacent files, and every event also carries
//                  CorrelationId/CausationId/an EventId-shaped id/an aggregate id, all of which
//                  follow this exact shape.
//
//                  Detection is structural, matching the same "duck-typed, no common base
//                  interface" approach established by SmartEnumJsonConverterFactory: a value type
//                  in a "PlantSafety" namespace that exposes a constructor (public or non-public,
//                  and NOT the implicit parameterless or record-struct-generated copy constructor)
//                  whose parameter names all match public readable instance property names
//                  case-insensitively. That constructor is used to reconstruct the instance on
//                  read (running its validation exactly as domain code does); on write, every
//                  public readable property (including derived/computed ones, e.g. RiskScore.Score)
//                  is emitted, preserving the exact JSON shape these types already produced via
//                  STJ's default write path (write behaviour was never broken -- only read).
//                  Additive-only evolution: safe to apply globally, including to value objects
//                  added by future modules that follow the same established §2.8 convention.
// ============================================================================
using System.Reflection;

namespace PlantSafety.Infrastructure.Common.Serialization;

/// <summary>
/// Recognises this codebase's validating-constructor, immutable value-object <c>record struct</c>
/// convention (Handbook §2.8) and reconstructs instances via that constructor on read, rather than
/// relying on System.Text.Json's default parameterless-constructor-plus-property-set behaviour,
/// which silently discards constructor-validated state for these get-only-property types.
/// </summary>
public sealed class ValidatingValueObjectJsonConverterFactory : JsonConverterFactory
{
    /// <inheritdoc/>
    public override bool CanConvert(Type typeToConvert)
    {
        if (!typeToConvert.IsValueType || typeToConvert.IsEnum || typeToConvert.IsPrimitive) return false;

        // Scoped to this codebase's own types only -- never intercept BCL/third-party value types
        // (Guid, DateTime, decimal, etc.), which STJ already handles correctly.
        if (typeToConvert.Namespace is null ||
            !typeToConvert.Namespace.StartsWith("PlantSafety", StringComparison.Ordinal))
        {
            return false;
        }

        // Nullable<T> is handled by STJ's own built-in nullable-wrapping around the converter
        // resolved for the underlying T -- registering for T alone is sufficient.
        if (Nullable.GetUnderlyingType(typeToConvert) is not null) return false;

        return TryResolveConstructor(typeToConvert, out _, out _);
    }

    /// <inheritdoc/>
    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var converterType = typeof(ValidatingValueObjectJsonConverter<>).MakeGenericType(typeToConvert);
        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }

    /// <summary>
    /// Resolves the "primary" constructor of <paramref name="type"/> for reconstruction purposes:
    /// the constructor (public or non-public) with the most parameters, all of which name-match
    /// (case-insensitively) a public readable instance property, excluding both the implicit
    /// struct parameterless constructor and any record-struct-generated copy constructor (a single
    /// parameter of <paramref name="type"/> itself).
    /// </summary>
    internal static bool TryResolveConstructor(Type type, out ConstructorInfo? ctor, out ParameterInfo[] parameters)
    {
        var publicProps = type
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && p.GetIndexParameters().Length == 0)
            .ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);

        ConstructorInfo? best = null;
        var bestParams = Array.Empty<ParameterInfo>();

        foreach (var candidate in type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
        {
            var ps = candidate.GetParameters();
            if (ps.Length == 0) continue;
            if (ps.Length == 1 && ps[0].ParameterType == type) continue;
            if (!ps.All(p => p.Name is not null && publicProps.ContainsKey(p.Name))) continue;

            if (ps.Length > bestParams.Length)
            {
                best = candidate;
                bestParams = ps;
            }
        }

        ctor = best;
        parameters = bestParams;
        return best is not null;
    }
}

/// <summary>
/// Reads/writes a single closed validating value-object struct type <typeparamref name="T"/> via
/// its resolved validating constructor, running the exact same invariant checks the domain type's
/// own constructor already enforces (Handbook §2.8 Rule 3 "Invariant-Driven Validation").
/// </summary>
internal sealed class ValidatingValueObjectJsonConverter<T> : JsonConverter<T> where T : struct
{
    private static readonly PropertyInfo[] ReadableProperties = typeof(T)
        .GetProperties(BindingFlags.Public | BindingFlags.Instance)
        .Where(p => p.CanRead && p.GetIndexParameters().Length == 0)
        .ToArray();

    private static readonly ConstructorInfo Ctor;
    private static readonly ParameterInfo[] CtorParameters;

    static ValidatingValueObjectJsonConverter()
    {
        if (!ValidatingValueObjectJsonConverterFactory.TryResolveConstructor(typeof(T), out var ctor, out var parameters) || ctor is null)
        {
            throw new InvalidOperationException(
                $"ValidatingValueObjectJsonConverter<{typeof(T).FullName}>: no validating constructor could be resolved.");
        }

        Ctor = ctor;
        CtorParameters = parameters;
    }

    /// <inheritdoc/>
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException($"Expected StartObject token while reading {typeof(T).FullName}.");

        using var doc = JsonDocument.ParseValue(ref reader);
        var values = new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase);
        foreach (var prop in doc.RootElement.EnumerateObject())
            values[prop.Name] = prop.Value.Clone();

        var args = new object?[CtorParameters.Length];
        for (var i = 0; i < CtorParameters.Length; i++)
        {
            var p = CtorParameters[i];
            if (values.TryGetValue(p.Name!, out var element))
            {
                args[i] = JsonSerializer.Deserialize(element.GetRawText(), p.ParameterType, options);
            }
            else if (p.HasDefaultValue)
            {
                args[i] = p.DefaultValue;
            }
            else if (Nullable.GetUnderlyingType(p.ParameterType) is not null || !p.ParameterType.IsValueType)
            {
                args[i] = null;
            }
            else
            {
                throw new JsonException($"Missing required property '{p.Name}' while reading {typeof(T).FullName}.");
            }
        }

        return (T)Ctor.Invoke(args);
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        foreach (var prop in ReadableProperties)
        {
            var val = prop.GetValue(value);
            if (val is null && options.DefaultIgnoreCondition == JsonIgnoreCondition.WhenWritingNull) continue;

            var name = options.PropertyNamingPolicy?.ConvertName(prop.Name) ?? prop.Name;
            writer.WritePropertyName(name);
            JsonSerializer.Serialize(writer, val, prop.PropertyType, options);
        }

        writer.WriteEndObject();
    }
}
