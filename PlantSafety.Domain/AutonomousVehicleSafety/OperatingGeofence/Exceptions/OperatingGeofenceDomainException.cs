// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AutonomousVehicleSafety.OperatingGeofence
//  Layer:          Domain
//  File:           OperatingGeofenceDomainException.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Abstract base for all domain exceptions raised in the OperatingGeofence module.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.AutonomousVehicleSafety.OperatingGeofence.Exceptions;

/// <summary>Abstract base for all domain exceptions raised in the OperatingGeofence module.</summary>
public abstract class OperatingGeofenceDomainException : DomainException
{
    /// <summary>Initialises with canonical SIEM exception fields.</summary>
    protected OperatingGeofenceDomainException(
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
