// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PPE.PPEAssignment
//  Layer:          Domain
//  File:           PPEAssignmentInvalidStatusTransitionException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a PPEAssignment lifecycle transition is not permitted.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PPE.PPEAssignment.Exceptions;

/// <summary>Thrown when a <see cref="PPEAssignment"/> lifecycle transition is not permitted.</summary>
public sealed class PPEAssignmentInvalidStatusTransitionException : PPEAssignmentDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public PPEAssignmentInvalidStatusTransitionException(
        PPEAssignmentId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            $"Cannot transition 'PPEAssignment' {id} from '{from}' to '{to}'.",
            "PPEAssignment.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}