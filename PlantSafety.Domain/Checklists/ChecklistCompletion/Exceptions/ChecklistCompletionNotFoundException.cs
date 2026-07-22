// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Checklists.ChecklistCompletion
//  Layer:          Domain
//  File:           ChecklistCompletionNotFoundException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Thrown when a ChecklistCompletion aggregate lookup finds no match.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Checklists.ChecklistCompletion.Exceptions;

/// <summary>Thrown when a <see cref="ChecklistCompletion"/> aggregate lookup finds no match.</summary>
public sealed class ChecklistCompletionNotFoundException : ChecklistCompletionDomainException
{
    /// <summary>Creates a not-found exception for the given ChecklistCompletion identity.</summary>
    public ChecklistCompletionNotFoundException(ChecklistCompletionId id, CorrelationId correlationId, CausationId causationId)
        : base(
            "ChecklistCompletion with id '" + id + "' was not found.",
            "ChecklistCompletion.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}