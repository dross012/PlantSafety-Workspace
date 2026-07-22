// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Common
//  Layer:          Infrastructure
//  File:           StoredProcedureBuilderAggregateExtensions.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Extension over StoredProcedureBuilder that binds an aggregate property
//                  (including value-object/enum-typed properties with private setters)
//                  directly to a stored-procedure parameter, using AggregateHydrator to
//                  resolve the correct ADO primitive and StoredProcedureBuilder's existing
//                  strongly-typed Add* overloads to bind it. No dynamic SQL; only parameter
//                  binding. Additive-only evolution.
// ============================================================================
using System.Reflection;

namespace PlantSafety.Infrastructure.Common.Persistence;

/// <summary>
/// Adds a single generic <see cref="AddAggregateProperty{TAgg}"/> extension to
/// <see cref="StoredProcedureBuilder"/> so the ~26 EmployeeSafety/ContractorSafety/
/// MachineSafety/SafetyProcedures repositories do not need to hand-write per-property
/// unwrapping code for every value object/enum on every aggregate.
/// </summary>
public static class StoredProcedureBuilderAggregateExtensions
{
    private const BindingFlags InstanceFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

    /// <summary>
    /// Reads <paramref name="propertyName"/> off <paramref name="agg"/>, unwraps any
    /// value-object/enum wrapping, and binds it to <paramref name="sqlParamName"/> using the
    /// correct <see cref="StoredProcedureBuilder"/> overload for the property's declared
    /// (unwrapped) primitive type.
    /// </summary>
    public static StoredProcedureBuilder AddAggregateProperty<TAgg>(
        this StoredProcedureBuilder builder, string sqlParamName, TAgg agg, string propertyName, int stringSize = 4000)
        where TAgg : class
    {
        var prop = typeof(TAgg).GetProperty(propertyName, InstanceFlags)
            ?? throw new InvalidOperationException($"Property '{propertyName}' not found on {typeof(TAgg).Name}.");

        var primitiveType = ResolvePrimitiveType(prop.PropertyType);
        var value = AggregateHydrator.Dehydrate(agg, propertyName);

        if (primitiveType == typeof(Guid)) return builder.AddGuid(sqlParamName, (Guid?)value);
        if (primitiveType == typeof(string)) return builder.AddString(sqlParamName, (string?)value, stringSize);
        if (primitiveType == typeof(int)) return builder.AddInt(sqlParamName, (int?)value);
        if (primitiveType == typeof(decimal)) return builder.AddDecimal(sqlParamName, (decimal?)value);
        if (primitiveType == typeof(bool)) return builder.AddBool(sqlParamName, (bool?)value);
        if (primitiveType == typeof(DateTime)) return builder.AddDateTime(sqlParamName, (DateTime?)value);

        throw new NotSupportedException($"Unsupported primitive type '{primitiveType.Name}' for property '{propertyName}' on {typeof(TAgg).Name}.");
    }

    /// <summary>
    /// Resolves the flat ADO primitive type that a (possibly value-object/enum/nullable)
    /// declared property type ultimately binds to, by unwrapping <c>Nullable&lt;T&gt;</c>,
    /// enums (-&gt; <see cref="string"/>), and any type exposing a public <c>Value</c> property
    /// (StrongIds, <see cref="UtcTimestamp"/>) down to its underlying primitive.
    /// </summary>
    private static Type ResolvePrimitiveType(Type declaredType)
    {
        var t = Nullable.GetUnderlyingType(declaredType) ?? declaredType;

        if (t.IsEnum) return typeof(string);
        if (t == typeof(Guid) || t == typeof(string) || t == typeof(int) ||
            t == typeof(decimal) || t == typeof(bool) || t == typeof(DateTime))
            return t;

        var valueProp = t.GetProperty("Value", InstanceFlags);
        if (valueProp is not null && valueProp.PropertyType != t)
            return ResolvePrimitiveType(valueProp.PropertyType);

        return t;
    }
}
