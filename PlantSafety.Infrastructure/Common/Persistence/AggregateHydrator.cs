// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Common
//  Layer:          Infrastructure
//  File:           AggregateHydrator.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Generic reflection-based rehydration/dehydration helper used by the
//                  EmployeeSafety / ContractorSafety / MachineSafety / SafetyProcedures
//                  Dapper repositories. These aggregates (like most in this codebase) expose
//                  only a private parameterless constructor and private-set properties, and
//                  do not (yet) expose a public Domain-layer rehydration/snapshot factory.
//                  Rather than widen the Domain public surface (out of scope for an
//                  Infrastructure-only task), this helper uses reflection - entirely inside
//                  the Infrastructure assembly - to (a) materialise an aggregate instance from
//                  a flat Dapper "row" POCO with matching property names, and (b) extract a
//                  primitive value back out of an aggregate/value-object graph for use as a
//                  stored-procedure parameter. No dynamic SQL is built; this only prepares
//                  CLR values that are subsequently passed as strongly-typed ADO parameters.
//                  Additive-only evolution.
// ============================================================================
using System.Reflection;

namespace PlantSafety.Infrastructure.Common.Persistence;

/// <summary>
/// Reflection-based aggregate rehydration/dehydration helper for aggregates with private
/// setters and a private parameterless constructor (rehydration-only). Confined entirely to
/// the Infrastructure layer; never touches Domain source.
/// </summary>
public static class AggregateHydrator
{
    private const BindingFlags InstanceFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

    /// <summary>
    /// Creates an uninitialised <typeparamref name="TAgg"/> instance (via its private
    /// parameterless constructor, so field initialisers still run) and copies every
    /// property present on <paramref name="row"/> onto the identically-named property
    /// of the aggregate, performing value-object/enum wrapping as required.
    /// </summary>
    public static TAgg Hydrate<TAgg, TRow>(TRow row)
        where TAgg : class
        where TRow : class
    {
        var agg = (TAgg)(Activator.CreateInstance(typeof(TAgg), nonPublic: true)
            ?? throw new InvalidOperationException($"Unable to construct {typeof(TAgg).Name} for rehydration."));

        foreach (var rowProp in typeof(TRow).GetProperties())
        {
            var aggProp = typeof(TAgg).GetProperty(rowProp.Name, InstanceFlags);
            if (aggProp is null || !aggProp.CanWrite)
                continue;

            var rawValue = rowProp.GetValue(row);
            var wrapped = Wrap(rawValue, aggProp.PropertyType);
            aggProp.SetValue(agg, wrapped);
        }

        return agg;
    }

    /// <summary>
    /// Converts a raw ADO/Dapper scalar (Guid, string, int, decimal, bool, DateTime, or their
    /// nullable equivalents) into the declared CLR type of an aggregate property: unwrapped
    /// primitives pass through, enums are parsed from their stored string form, and
    /// StrongId/UtcTimestamp-shaped value objects (any type exposing a public static
    /// <c>FromGuid(Guid)</c> method, or <see cref="PlantSafety.SharedKernel.Primitives.ValueTypes.UtcTimestamp"/>)
    /// are reconstructed via their canonical factory.
    /// </summary>
    public static object? Wrap(object? rawValue, Type declaredType)
    {
        var underlying = Nullable.GetUnderlyingType(declaredType) ?? declaredType;

        if (rawValue is null || rawValue is DBNull)
        {
            if (underlying == typeof(string)) return null;
            if (Nullable.GetUnderlyingType(declaredType) is not null) return null;
            if (!declaredType.IsValueType) return null;
            return null;
        }

        if (underlying.IsEnum)
            return Enum.Parse(underlying, Convert.ToString(rawValue)!);

        if (underlying == typeof(UtcTimestamp))
            return new UtcTimestamp(Convert.ToDateTime(rawValue));

        var fromGuid = underlying.GetMethod("FromGuid", BindingFlags.Public | BindingFlags.Static, [typeof(Guid)]);
        if (fromGuid is not null)
            return fromGuid.Invoke(null, [(Guid)rawValue]);

        if (underlying == typeof(Guid)) return (Guid)rawValue;
        if (underlying == typeof(string)) return (string)rawValue;
        if (underlying == typeof(int)) return Convert.ToInt32(rawValue);
        if (underlying == typeof(decimal)) return Convert.ToDecimal(rawValue);
        if (underlying == typeof(bool)) return Convert.ToBoolean(rawValue);
        if (underlying == typeof(DateTime)) return Convert.ToDateTime(rawValue);

        return rawValue;
    }

    /// <summary>
    /// Reads the named property off <paramref name="agg"/> (including private-set
    /// properties) and reduces it to the flat ADO primitive that
    /// <see cref="Persistence.Dapper.SqlBuilder.StoredProcedureBuilder"/> can bind:
    /// value objects are unwrapped via their public <c>Value</c> property, enums via
    /// <see cref="Enum.ToString()"/>, and everything else passes through unchanged.
    /// </summary>
    public static object? Dehydrate<TAgg>(TAgg agg, string propertyName) where TAgg : class
    {
        var prop = typeof(TAgg).GetProperty(propertyName, InstanceFlags)
            ?? throw new InvalidOperationException($"Property '{propertyName}' not found on {typeof(TAgg).Name}.");

        var raw = prop.GetValue(agg);
        return Unwrap(raw);
    }

    private static object? Unwrap(object? value)
    {
        if (value is null) return null;

        var type = value.GetType();

        if (type.IsEnum) return value.ToString();

        if (type == typeof(Guid) || type == typeof(string) || type == typeof(int) ||
            type == typeof(decimal) || type == typeof(bool) || type == typeof(DateTime))
            return value;

        var valueProp = type.GetProperty("Value", InstanceFlags);
        if (valueProp is not null && valueProp.PropertyType != type)
            return Unwrap(valueProp.GetValue(value));

        return value;
    }
}
