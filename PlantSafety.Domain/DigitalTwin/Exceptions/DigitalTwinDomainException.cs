// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         DigitalTwin
//  Layer:          Domain
//  File:           DigitalTwinDomainException.cs
//  Created:        2026-08-02
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Abstract base for all domain exceptions raised in the DigitalTwin module. Phase
//                  4 full build-out. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.DigitalTwin.Exceptions;

/// <summary>Abstract base for all domain exceptions raised in the DigitalTwin module.</summary>
public abstract class DigitalTwinDomainException : DomainException
{
    /// <summary>Initialises with canonical SIEM exception fields.</summary>
    protected DigitalTwinDomainException(
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
