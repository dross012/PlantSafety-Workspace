// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.CraneHoistSystems
//  Layer:          Domain
//  File:           CraneHoistSystemsDomainException.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Abstract base for all domain exceptions raised in the CraneHoistSystems module.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.MachineSafety.CraneHoistSystems.Exceptions;

/// <summary>Abstract base for all domain exceptions raised in the CraneHoistSystems module.</summary>
public abstract class CraneHoistSystemsDomainException : DomainException
{
    /// <summary>Initialises with canonical SIEM exception fields.</summary>
    protected CraneHoistSystemsDomainException(
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
