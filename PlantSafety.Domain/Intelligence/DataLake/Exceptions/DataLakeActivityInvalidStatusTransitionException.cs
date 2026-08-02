// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.DataLake
//  Layer:          Domain
//  File:           DataLakeActivityInvalidStatusTransitionException.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code refactoring-agent)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a DataLakeActivity lifecycle transition is not permitted.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Intelligence.DataLake.ValueObjects.Ids;

namespace PlantSafety.Domain.Intelligence.DataLake.Exceptions;

/// <summary>Thrown when a DataLakeActivity lifecycle transition is not permitted.</summary>
public sealed class DataLakeActivityInvalidStatusTransitionException : DataLakeActivityDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public DataLakeActivityInvalidStatusTransitionException(
        DataLakeActivityId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            $"Cannot transition DataLakeActivity '{id}' from '{from}' to '{to}'.",
            "DataLake.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
