// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Checklists.ChecklistTemplate
//  Layer:          Domain
//  File:           ChecklistTemplateInvalidStatusTransitionException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Thrown when a ChecklistTemplate lifecycle transition is not permitted.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Checklists.ChecklistTemplate.Exceptions;

/// <summary>Thrown when a <see cref="ChecklistTemplate"/> lifecycle transition is not permitted.</summary>
public sealed class ChecklistTemplateInvalidStatusTransitionException : ChecklistTemplateDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public ChecklistTemplateInvalidStatusTransitionException(
        ChecklistTemplateId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            "Cannot transition 'ChecklistTemplate' " + id + " from '" + from + "' to '" + to + "'.",
            "ChecklistTemplate.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}