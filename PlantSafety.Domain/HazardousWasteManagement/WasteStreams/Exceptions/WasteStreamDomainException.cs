// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HazardousWasteManagement.WasteStreams
//  Layer:          Domain
//  File:           WasteStreamDomainException.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Abstract base for all domain exceptions raised in the WasteStreams module.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.HazardousWasteManagement.WasteStreams.Exceptions;

/// <summary>Abstract base for all domain exceptions raised in the WasteStreams module.</summary>
public abstract class WasteStreamDomainException : DomainException
{
    /// <summary>Initialises with canonical SIEM exception fields.</summary>
    protected WasteStreamDomainException(
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
