// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.PresenceSensingDevices
//  Layer:          Domain
//  File:           PresenceSensingDevicesDomainException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Abstract base for all domain exceptions raised in the PresenceSensingDevices module.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.MachineSafety.PresenceSensingDevices.Exceptions;

/// <summary>Abstract base for all domain exceptions raised in the PresenceSensingDevices module.</summary>
public abstract class PresenceSensingDevicesDomainException : DomainException
{
    /// <summary>Initialises with canonical SIEM exception fields.</summary>
    protected PresenceSensingDevicesDomainException(
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