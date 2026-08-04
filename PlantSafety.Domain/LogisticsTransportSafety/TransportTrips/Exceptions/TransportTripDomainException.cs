// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         LogisticsTransportSafety.TransportTrips
//  Layer:          Domain
//  File:           TransportTripDomainException.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Abstract base for all domain exceptions raised in the TransportTrips module.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.LogisticsTransportSafety.TransportTrips.Exceptions;

/// <summary>Abstract base for all domain exceptions raised in the TransportTrips module.</summary>
public abstract class TransportTripDomainException : DomainException
{
    /// <summary>Initialises with canonical SIEM exception fields.</summary>
    protected TransportTripDomainException(
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
