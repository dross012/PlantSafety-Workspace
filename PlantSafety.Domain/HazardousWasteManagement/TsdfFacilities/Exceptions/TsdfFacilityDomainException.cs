// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HazardousWasteManagement.TsdfFacilities
//  Layer:          Domain
//  File:           TsdfFacilityDomainException.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Abstract base for all domain exceptions raised in the TsdfFacilities module.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.HazardousWasteManagement.TsdfFacilities.Exceptions;

/// <summary>Abstract base for all domain exceptions raised in the TsdfFacilities module.</summary>
public abstract class TsdfFacilityDomainException : DomainException
{
    /// <summary>Initialises with canonical SIEM exception fields.</summary>
    protected TsdfFacilityDomainException(
        string message, string errorCode, string errorCategory, string severity,
        CorrelationId correlationId, CausationId causationId)
        : base(message, errorCode, errorCategory, severity, correlationId, causationId)
    {
    }
}
