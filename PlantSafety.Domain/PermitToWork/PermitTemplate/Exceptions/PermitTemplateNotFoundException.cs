// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PermitToWork.PermitTemplate
//  Layer:          Domain
//  File:           PermitTemplateNotFoundException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a PermitTemplate aggregate lookup finds no match.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PermitToWork.PermitTemplate.Exceptions;

/// <summary>Thrown when a <see cref="PermitTemplate"/> aggregate lookup finds no match.</summary>
public sealed class PermitTemplateNotFoundException : PermitTemplateDomainException
{
    /// <summary>Creates a not-found exception for the given PermitTemplate identity.</summary>
    public PermitTemplateNotFoundException(PermitTemplateId id, CorrelationId correlationId, CausationId causationId)
        : base(
            $"PermitTemplate with id '{id}' was not found.",
            "PermitTemplate.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}