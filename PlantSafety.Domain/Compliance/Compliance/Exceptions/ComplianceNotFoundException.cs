// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Compliance
//  Layer:          Domain
//  File:           ComplianceNotFoundException.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.Compliance.Exceptions;

/// <summary>Thrown when a <see cref="Compliance"/> aggregate lookup finds no match.</summary>
public sealed class ComplianceNotFoundException : ComplianceDomainException
{
    /// <summary>Creates a not-found exception for the given Compliance identity.</summary>
    public ComplianceNotFoundException(ComplianceId id, CorrelationId correlationId, CausationId causationId)
        : base(
            "Compliance with id '{id}' was not found.",
            "Compliance.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
