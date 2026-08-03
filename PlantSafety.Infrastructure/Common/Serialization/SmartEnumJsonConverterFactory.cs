// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Common
//  Layer:          Infrastructure
//  File:           SmartEnumJsonConverterFactory.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Phase 5 §7 Performance & Stress Hardening)
//  SchemaVersion:  2026.1
//  Description:    JsonConverterFactory that recognises this codebase's "Smart Enum" pattern
//                  (Handbook Rule 2.7 -- e.g. PlantSafety.Domain.RiskManagement.Enums.RiskLevel,
//                  Checklists.SmartEnums.ChecklistCategory, Auditing.SmartEnums.AuditDisciplineType,
//                  SafetyCulture.SmartEnums.CultureMetricType) and (de)serialises those instances
//                  as their plain Code string, instead of System.Text.Json's reflection-based
//                  default (which fails: Smart Enum constructors are private, so STJ cannot
//                  materialise new instances, and it also cannot pick a JSON representation for a
//                  type with no public constructor). Previously, the OutboxProcessor's fallback
//                  path silently swallowed this and republished the raw envelope wrapper instead of
//                  the real, strongly-typed domain event -- meaning any event carrying a Smart Enum
//                  value (e.g. RiskManagementCreatedEvent.Level : RiskLevel) never reached its real
//                  MassTransit IConsumer<T>.
//
//                  Detection is structural (duck-typed), not via a common base class/marker
//                  interface, because none exists in this codebase today -- every Smart Enum listed
//                  above independently implements the same shape (sealed class, exclusively
//                  non-public constructors, a public instance `Code` string property, a public
//                  static `From(string)` factory that resolves-or-throws). Any type matching that
//                  shape is treated as a Smart Enum. Additive-only evolution: safe to apply
//                  globally, including to Smart Enums added by future modules that follow the same
//                  established convention.
// ============================================================================
using System.Reflection;

namespace PlantSafety.Infrastructure.Common.Serialization;

/// <summary>
/// Recognises and converts this codebase's "Smart Enum" convention types to/from their
/// <c>Code</c> string, matching how these types already <c>ToString()</c> and are referenced
/// (SQL columns, DTOs, dashboards) everywhere else in the codebase.
/// </summary>
public sealed class SmartEnumJsonConverterFactory : JsonConverterFactory
{
    /// <inheritdoc/>
    public override bool CanConvert(Type typeToConvert)
    {
        if (!typeToConvert.IsClass || typeToConvert.IsAbstract) return false;

        // Distinguishes a genuine closed Smart Enum from an incidental DTO that happens to
        // expose a "Code" string property: Smart Enums in this codebase expose exclusively
        // private (non-public) constructors, forcing all instantiation through the static
        // From(...)/well-known static field members.
        if (typeToConvert.GetConstructors(BindingFlags.Public | BindingFlags.Instance).Length > 0) return false;

        var codeProperty = typeToConvert.GetProperty("Code", BindingFlags.Public | BindingFlags.Instance);
        if (codeProperty is null || codeProperty.PropertyType != typeof(string) || !codeProperty.CanRead) return false;

        var fromMethod = typeToConvert.GetMethod(
            "From",
            BindingFlags.Public | BindingFlags.Static,
            binder: null,
            types: [typeof(string)],
            modifiers: null);

        return fromMethod is not null && fromMethod.ReturnType == typeToConvert;
    }

    /// <inheritdoc/>
    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var converterType = typeof(SmartEnumJsonConverter<>).MakeGenericType(typeToConvert);
        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }
}

/// <summary>
/// Reads/writes a single closed Smart Enum type <typeparamref name="T"/> as its <c>Code</c>
/// string. The <c>From(string)</c>/<c>Code</c> accessors are resolved once per closed generic
/// type into compiled delegates (not held open as raw <see cref="MethodInfo"/> invocations) so
/// the hot Outbox publish/consume path pays reflection cost once per type, not per message.
/// </summary>
internal sealed class SmartEnumJsonConverter<T> : JsonConverter<T> where T : class
{
    private static readonly Func<string, T> FromCode = BuildFromCode();
    private static readonly Func<T, string> GetCode = BuildGetCode();

    /// <inheritdoc/>
    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null) return null;

        var code = reader.GetString();
        return string.IsNullOrEmpty(code) ? null : FromCode(code);
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        => writer.WriteStringValue(GetCode(value));

    private static Func<string, T> BuildFromCode()
    {
        var method = typeof(T).GetMethod(
            "From",
            BindingFlags.Public | BindingFlags.Static,
            binder: null,
            types: [typeof(string)],
            modifiers: null)!;

        return (Func<string, T>)Delegate.CreateDelegate(typeof(Func<string, T>), method);
    }

    private static Func<T, string> BuildGetCode()
    {
        var getMethod = typeof(T).GetProperty("Code", BindingFlags.Public | BindingFlags.Instance)!.GetGetMethod()!;
        return (Func<T, string>)Delegate.CreateDelegate(typeof(Func<T, string>), getMethod);
    }
}
