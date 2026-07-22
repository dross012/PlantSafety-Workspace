// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PPE.PPECategory
//  Layer:          Domain
//  File:           PPECategoryDomainException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Abstract base for all domain exceptions raised in the PPECategory sub-module.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PPE.PPECategory.Exceptions;

/// <summary>Abstract base for all domain exceptions raised in the PPECategory module.</summary>
public abstract class PPECategoryDomainException : DomainException
{
    /// <summary>Initialises with canonical SIEM exception fields.</summary>
    protected PPECategoryDomainException(
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