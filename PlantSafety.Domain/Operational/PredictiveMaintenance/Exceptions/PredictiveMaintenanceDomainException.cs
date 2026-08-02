// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.PredictiveMaintenance
//  Layer:          Domain
//  File:           PredictiveMaintenanceDomainException.cs
//  Created:        2026-07-30
//  SchemaVersion:  2026.1
//  Description:    Abstract base for all domain exceptions raised in the PredictiveMaintenance
//                  module. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Operational.PredictiveMaintenance.Exceptions;

/// <summary>Abstract base for all domain exceptions raised in the PredictiveMaintenance module.</summary>
public abstract class PredictiveMaintenanceDomainException : DomainException
{
    /// <summary>Initialises with canonical SIEM exception fields.</summary>
    protected PredictiveMaintenanceDomainException(
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
