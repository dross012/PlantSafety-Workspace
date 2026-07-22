// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.Ppe
//  Layer:          Domain
//  File:           PpeDomainException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Abstract base for all domain exceptions raised in the Ppe module.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EmployeeSafety.Ppe.Exceptions;

/// <summary>Abstract base for all domain exceptions raised in the Ppe module.</summary>
public abstract class PpeDomainException : DomainException
{
    /// <summary>Initialises with canonical SIEM exception fields.</summary>
    protected PpeDomainException(
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