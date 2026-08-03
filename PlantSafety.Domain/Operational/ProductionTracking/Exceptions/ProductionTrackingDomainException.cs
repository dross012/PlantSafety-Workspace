// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.ProductionTracking
//  Layer:          Domain
//  File:           ProductionTrackingDomainException.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Abstract base for all domain exceptions raised in the
//                  Operational.ProductionTracking module. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Operational.ProductionTracking.Exceptions;

/// <summary>Abstract base for all domain exceptions raised in the Operational.ProductionTracking module.</summary>
public abstract class ProductionTrackingDomainException : DomainException
{
    /// <summary>Initialises with canonical SIEM exception fields.</summary>
    protected ProductionTrackingDomainException(
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
