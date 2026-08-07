// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Classification
//  Layer:          Domain
//  File:           InvalidTIHZoneAssignmentException.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Thrown when a HazmatMaterial's IsTIH flag and TIHZone assignment are
//                  inconsistent (a TIH zone is required if and only if IsTIH is true).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.PHMSA.Classification.Exceptions;

/// <summary>
/// Thrown when a <c>HazmatMaterial</c>'s <c>IsTIH</c> flag and <c>TIHZone</c> assignment are
/// inconsistent -- a TIH zone must be supplied if and only if the material is flagged TIH.
/// </summary>
public sealed class InvalidTIHZoneAssignmentException : HazmatMaterialDomainException
{
    /// <summary>Creates an invalid TIH-zone-assignment exception.</summary>
    public InvalidTIHZoneAssignmentException(bool isTIH, string? tihZone, CorrelationId correlationId, CausationId causationId)
        : base(
            isTIH
                ? "IsTIH is true but no TIHZone was supplied; a TIH material requires a zone (A-D)."
                : $"IsTIH is false but TIHZone '{tihZone}' was supplied; a non-TIH material must not have a zone.",
            "HazmatMaterial.InvalidTIHZone",
            "Validation",
            "High",
            correlationId,
            causationId)
    {
    }
}
