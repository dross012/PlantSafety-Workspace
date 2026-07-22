// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Persistence
//  Layer:          Infrastructure
//  File:           AggregateReflectionHydrator.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Shared helper for rehydrating DDD aggregate roots (private setters,
//                  private parameterless "for rehydration only" constructor, no public
//                  Rehydrate() factory method) from flat Dapper read-model rows, without
//                  requiring any change to Domain-layer aggregate/factory source. Used by
//                  Checklists/Auditing/SafetyCulture Dapper repositories (and any other
//                  module following the same aggregate shape) to bridge the gap between
//                  Dapper (no ORM identity map / no reflection-based materialization) and
//                  aggregates that intentionally expose no public setters.
//                  Additive-only evolution.
// ============================================================================
using System.Reflection;

namespace PlantSafety.Infrastructure.Persistence.Reflection;

/// <summary>
/// Reflection-based aggregate rehydration helper. Aggregates in this codebase use a private
/// parameterless constructor ("For rehydration only.") plus private-setter properties and
/// private backing fields for child collections, with no public Rehydrate()/factory support
/// for reconstructing an aggregate from persisted state. Since Domain source must not be
/// modified to add one, this helper uses reflection to construct an uninitialized instance
/// and populate its private state directly from a read-model row.
/// </summary>
public static class AggregateReflectionHydrator
{
    private const BindingFlags InstanceFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

    /// <summary>
    /// Creates an uninitialized instance of <typeparamref name="T"/> without invoking any
    /// constructor (bypasses the private "for rehydration only" constructor entirely).
    /// </summary>
    public static T CreateUninitialized<T>() where T : class
        => (T)System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(typeof(T));

    /// <summary>
    /// Sets a (possibly private-setter) auto-property value on <paramref name="instance"/> by name.
    /// </summary>
    public static void SetProperty<T>(T instance, string propertyName, object? value) where T : class
    {
        var property = typeof(T).GetProperty(propertyName, InstanceFlags)
            ?? throw new InvalidOperationException($"Property '{propertyName}' not found on type '{typeof(T).Name}'.");

        property.SetValue(instance, value);
    }

    /// <summary>
    /// Returns the live reference to a private backing collection field (e.g. <c>_items</c>),
    /// so callers can populate it in place (readonly fields may still have their referenced
    /// collection mutated).
    /// </summary>
    public static TCollection GetPrivateCollectionField<T, TCollection>(T instance, string fieldName)
        where T : class
        where TCollection : class
    {
        var field = typeof(T).GetField(fieldName, InstanceFlags)
            ?? throw new InvalidOperationException($"Field '{fieldName}' not found on type '{typeof(T).Name}'.");

        return (TCollection)(field.GetValue(instance)
            ?? throw new InvalidOperationException($"Field '{fieldName}' on type '{typeof(T).Name}' was null."));
    }

    /// <summary>
    /// Sets a private instance field (e.g. a readonly backing-collection field such as
    /// <c>_domainEvents</c> or <c>_acknowledgments</c>) directly by name. Required because
    /// <see cref="CreateUninitialized{T}"/> bypasses all constructors and therefore skips field
    /// initializers -- private collection fields must be seeded before
    /// <see cref="GetPrivateCollectionField{T, TCollection}"/> can be used against them.
    /// </summary>
    public static void SetField<T>(T instance, string fieldName, object? value) where T : class
    {
        var field = typeof(T).GetField(fieldName, InstanceFlags)
            ?? throw new InvalidOperationException($"Field '{fieldName}' not found on type '{typeof(T).Name}'.");

        field.SetValue(instance, value);
    }
}
