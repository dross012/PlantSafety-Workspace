// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Checklists.ChecklistCompletion
//  Layer:          Domain
//  File:           ChecklistCompletionInvalidStatusTransitionException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Thrown when a ChecklistCompletion lifecycle transition is not permitted.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Checklists.ChecklistCompletion.Exceptions;

/// <summary>Thrown when a <see cref="ChecklistCompletion"/> lifecycle transition is not permitted.</summary>
public sealed class ChecklistCompletionInvalidStatusTransitionException : ChecklistCompletionDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public ChecklistCompletionInvalidStatusTransitionException(
        ChecklistCompletionId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            "Cannot transition 'ChecklistCompletion' " + id + " from '" + from + "' to '" + to + "'.",
            "ChecklistCompletion.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}