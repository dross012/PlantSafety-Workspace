// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Classification
//  Layer:          Domain
//  File:           InvalidUNNumberException.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Thrown when a candidate UN number does not match the canonical
//                  "UN" + 4-digit format (e.g. "UN1090").
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.PHMSA.Classification.Exceptions;

/// <summary>Thrown when a candidate UN number does not match the "UN" + 4-digit format.</summary>
public sealed class InvalidUNNumberException : HazmatMaterialDomainException
{
    /// <summary>Creates an invalid-UN-number exception for the supplied raw value.</summary>
    public InvalidUNNumberException(string? value, CorrelationId correlationId, CausationId causationId)
        : base(
            $"'{value}' is not a valid UN number. Expected format: 'UN' followed by exactly 4 digits (e.g. 'UN1090').",
            "HazmatMaterial.InvalidUNNumber",
            "Validation",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
