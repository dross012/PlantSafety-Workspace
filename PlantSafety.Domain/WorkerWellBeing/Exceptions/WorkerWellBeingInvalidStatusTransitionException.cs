// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         WorkerWellBeing
//  Layer:          Domain
//  File:           WorkerWellBeingInvalidStatusTransitionException.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Thrown when a WorkerWellBeing lifecycle transition is not permitted.
//                  Phase 4 full build-out. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.WorkerWellBeing.Exceptions;

/// <summary>Thrown when a <see cref="WorkerWellBeing"/> lifecycle transition is not permitted.</summary>
public sealed class WorkerWellBeingInvalidStatusTransitionException : WorkerWellBeingDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public WorkerWellBeingInvalidStatusTransitionException(
        WorkerWellBeingId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            $"Cannot transition 'WorkerWellBeing' {id} from '{from}' to '{to}'.",
            "WorkerWellBeing.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
