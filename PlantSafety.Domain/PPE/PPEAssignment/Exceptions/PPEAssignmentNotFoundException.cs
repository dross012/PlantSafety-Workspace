// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PPE.PPEAssignment
//  Layer:          Domain
//  File:           PPEAssignmentNotFoundException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a PPEAssignment aggregate lookup finds no match.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PPE.PPEAssignment.Exceptions;

/// <summary>Thrown when a <see cref="PPEAssignment"/> aggregate lookup finds no match.</summary>
public sealed class PPEAssignmentNotFoundException : PPEAssignmentDomainException
{
    /// <summary>Creates a not-found exception for the given PPEAssignment identity.</summary>
    public PPEAssignmentNotFoundException(PPEAssignmentId id, CorrelationId correlationId, CausationId causationId)
        : base(
            $"PPEAssignment with id '{id}' was not found.",
            "PPEAssignment.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}