// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Common.Persistence
//  Layer:          Infrastructure
//  File:           AggregateReflectionRehydrator.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:
//      Shared reflection-based helper for rehydrating DDD aggregate roots that expose only a
//      private parameterless constructor and private-set auto-properties/private backing
//      fields (the canonical shape mandated by the Aggregate Rules for behavior-driven
//      mutation). Because this platform forbids EF Core (Dapper + stored procedures only) and
//      the Domain layer deliberately exposes no persistence-facing constructor/setter surface,
//      Infrastructure repositories rehydrate aggregates from stored-procedure result rows via
//      reflection rather than by widening the Domain's public API. This is the same technique
//      used internally by mainstream serializers (e.g. System.Text.Json,
//      System.Runtime.Serialization) to populate objects without invoking business
//      constructors.
//      Additive-only evolution. Does not modify any Domain or Application file.
// ============================================================================
using System.Reflection;
using System.Runtime.CompilerServices;

namespace PlantSafety.Infrastructure.Common.Persistence;

/// <summary>
/// Reflection-based aggregate rehydration helper. Used exclusively by Infrastructure Dapper
/// repositories to reconstruct aggregate roots (and their private child-entity collections)
/// from persisted rows, bypassing the Domain's behavior-only public API by design.
/// </summary>
public static class AggregateReflectionRehydrator
{
    private const BindingFlags InstanceFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

    /// <summary>
    /// Allocates a new, fully uninitialized <typeparamref name="T"/> instance without invoking
    /// any constructor (equivalent to the technique used by <c>System.Text.Json</c> and other
    /// serializers for types with no public constructor).
    /// </summary>
    public static T CreateUninitialized<T>() where T : class
        => (T)RuntimeHelpers.GetUninitializedObject(typeof(T));

    /// <summary>Sets a (possibly private-set) auto-property value via reflection.</summary>
    public static void SetProperty<T>(T instance, string propertyName, object? value) where T : class
    {
        var property = typeof(T).GetProperty(propertyName, InstanceFlags)
            ?? throw new InvalidOperationException($"Property '{propertyName}' not found on type '{typeof(T).FullName}' during rehydration.");
        property.SetValue(instance, value);
    }

    /// <summary>Sets a private field value directly via reflection (used for readonly backing lists).</summary>
    public static void SetField<T>(T instance, string fieldName, object? value) where T : class
    {
        var field = typeof(T).GetField(fieldName, InstanceFlags)
            ?? throw new InvalidOperationException($"Field '{fieldName}' not found on type '{typeof(T).FullName}' during rehydration.");
        field.SetValue(instance, value);
    }

    /// <summary>
    /// Returns the live backing <see cref="List{TItem}"/> referenced by a private readonly
    /// field (e.g. <c>_responseActions</c>), so callers can populate it in place during
    /// rehydration without cloning or replacing the field.
    /// </summary>
    public static List<TItem> GetListField<T, TItem>(T instance, string fieldName) where T : class
    {
        var field = typeof(T).GetField(fieldName, InstanceFlags)
            ?? throw new InvalidOperationException($"Field '{fieldName}' not found on type '{typeof(T).FullName}' during rehydration.");
        return (List<TItem>)field.GetValue(instance)!;
    }
}
