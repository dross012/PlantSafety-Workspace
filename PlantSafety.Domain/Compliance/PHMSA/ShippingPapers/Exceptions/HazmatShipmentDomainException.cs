// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.ShippingPapers
//  Layer:          Domain
//  File:           HazmatShipmentDomainException.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Abstract base for all domain exceptions raised in the PHMSA ShippingPapers module.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.PHMSA.ShippingPapers.Exceptions;

/// <summary>Abstract base for all domain exceptions raised in the PHMSA ShippingPapers module.</summary>
public abstract class HazmatShipmentDomainException : DomainException
{
    /// <summary>Initialises with canonical SIEM exception fields.</summary>
    protected HazmatShipmentDomainException(
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
