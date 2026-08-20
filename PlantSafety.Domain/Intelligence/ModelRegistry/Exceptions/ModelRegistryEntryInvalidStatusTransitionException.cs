// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.ModelRegistry
//  Layer:          Domain
//  File:           ModelRegistryEntryInvalidStatusTransitionException.cs
//  Created:        2026-08-20
//  Author:         PlantSafety Platform Team (Claude Code file-and-folder-agent)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a ModelRegistryEntry lifecycle transition is not permitted. Mirrors
//                  PlantSafety.Domain.Intelligence.DataLake.Exceptions
//                  .DataLakeActivityInvalidStatusTransitionException.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Intelligence.ModelRegistry.ValueObjects.Ids;

namespace PlantSafety.Domain.Intelligence.ModelRegistry.Exceptions;

/// <summary>Thrown when a ModelRegistryEntry lifecycle transition is not permitted.</summary>
public sealed class ModelRegistryEntryInvalidStatusTransitionException : ModelRegistryEntryDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public ModelRegistryEntryInvalidStatusTransitionException(
        ModelRegistryEntryId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            $"Cannot transition ModelRegistryEntry '{id}' from '{from}' to '{to}'.",
            "ModelRegistry.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
