// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.PreventiveMaintenance
//  Layer:          Domain
//  File:           PreventiveMaintenanceDomainException.cs
//  Created:        2026-07-30
//  SchemaVersion:  2026.1
//  Description:    Abstract base for all domain exceptions raised in the PreventiveMaintenance
//                  module. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Operational.PreventiveMaintenance.Exceptions;

/// <summary>Abstract base for all domain exceptions raised in the PreventiveMaintenance module.</summary>
public abstract class PreventiveMaintenanceDomainException : DomainException
{
    /// <summary>Initialises with canonical SIEM exception fields.</summary>
    protected PreventiveMaintenanceDomainException(
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
