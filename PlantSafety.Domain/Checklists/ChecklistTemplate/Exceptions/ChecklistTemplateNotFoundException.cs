// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Checklists.ChecklistTemplate
//  Layer:          Domain
//  File:           ChecklistTemplateNotFoundException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Thrown when a ChecklistTemplate aggregate lookup finds no match.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Checklists.ChecklistTemplate.Exceptions;

/// <summary>Thrown when a <see cref="ChecklistTemplate"/> aggregate lookup finds no match.</summary>
public sealed class ChecklistTemplateNotFoundException : ChecklistTemplateDomainException
{
    /// <summary>Creates a not-found exception for the given ChecklistTemplate identity.</summary>
    public ChecklistTemplateNotFoundException(ChecklistTemplateId id, CorrelationId correlationId, CausationId causationId)
        : base(
            "ChecklistTemplate with id '" + id + "' was not found.",
            "ChecklistTemplate.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}