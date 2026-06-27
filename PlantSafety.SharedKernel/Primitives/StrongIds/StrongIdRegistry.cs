/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-29
    FILE:               StrongIdRegistry.cs
    DESCRIPTION:        Central registry for StrongId types. Enables reflection-free creation, parsing, and metadata
                        lookup for all StrongId types across bounded contexts.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

using System;
using System.Collections.Concurrent;

namespace PlantSafety.SharedKernel.Primitives.StrongIds;

/// <summary>
/// Registry for StrongId types.
/// </summary>
public static class StrongIdRegistry
{
    private static readonly ConcurrentDictionary<Type, Func<Guid, IStrongId>> _factories = new();

    /// <summary>
    /// Registers a StrongId type with a factory.
    /// </summary>
    public static void Register<TStrongId>(Func<Guid, TStrongId> factory)
        where TStrongId : IStrongId
        => _factories[typeof(TStrongId)] = guid => factory(guid);

    /// <summary>
    /// Creates a StrongId instance from a Guid.
    /// </summary>
    public static IStrongId Create(Type type, Guid value)
    {
        if (_factories.TryGetValue(type, out var factory))
            return factory(value);

        throw new InvalidOperationException(
            $"StrongId type '{type.Name}' is not registered in StrongIdRegistry.");
    }
}
