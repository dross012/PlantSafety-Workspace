// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.CrisisManagement
//  Layer:          Domain
//  File:           CrisisManagementDomainException.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Abstract base for all domain exceptions raised in the SafetyEvents.CrisisManagement
//                  module. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.CrisisManagement.Exceptions;

/// <summary>Abstract base for all domain exceptions raised in the SafetyEvents.CrisisManagement module.</summary>
public abstract class CrisisManagementDomainException : DomainException
{
    /// <summary>Initialises with canonical SIEM exception fields.</summary>
    protected CrisisManagementDomainException(
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
