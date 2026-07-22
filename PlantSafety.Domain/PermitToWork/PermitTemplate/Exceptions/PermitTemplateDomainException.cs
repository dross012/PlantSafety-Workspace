// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PermitToWork.PermitTemplate
//  Layer:          Domain
//  File:           PermitTemplateDomainException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Abstract base for all domain exceptions raised in the PermitTemplate sub-module.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PermitToWork.PermitTemplate.Exceptions;

/// <summary>Abstract base for all domain exceptions raised in the PermitTemplate module.</summary>
public abstract class PermitTemplateDomainException : DomainException
{
    /// <summary>Initialises with canonical SIEM exception fields.</summary>
    protected PermitTemplateDomainException(
        string message,
        string errorCode,
        string errorCategory,
        string severity,
        CorrelationId correlationId,
        CausationId causationId)
        : base(message, errorCode, errorCategory, severity, correlationId, causationId)
    {
    }
}