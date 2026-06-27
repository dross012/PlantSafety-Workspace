/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-06
    FILE:               IStrongId.cs
    DESCRIPTION:        Marker interface for all StrongId value objects. Provides a unified contract for identity
                        primitives across all bounded contexts.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

namespace PlantSafety.SharedKernel.Primitives.StrongIds;

/// <summary>
/// Marker interface for all StrongId value objects.
/// </summary>
public interface IStrongId
{
    /// <summary>
    /// The underlying Guid value.
    /// </summary>
    Guid Value { get; }

    /// <summary>
    /// Indicates whether the StrongId wraps Guid.Empty.
    /// </summary>
    bool IsEmpty { get; }
}
