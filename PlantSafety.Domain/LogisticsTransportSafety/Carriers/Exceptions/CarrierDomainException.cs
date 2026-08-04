// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         LogisticsTransportSafety.Carriers
//  Layer:          Domain
//  File:           CarrierDomainException.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Abstract base for all domain exceptions raised in the Carriers module.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.LogisticsTransportSafety.Carriers.Exceptions;

/// <summary>Abstract base for all domain exceptions raised in the Carriers module.</summary>
public abstract class CarrierDomainException : DomainException
{
    /// <summary>Initialises with canonical SIEM exception fields.</summary>
    protected CarrierDomainException(
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
