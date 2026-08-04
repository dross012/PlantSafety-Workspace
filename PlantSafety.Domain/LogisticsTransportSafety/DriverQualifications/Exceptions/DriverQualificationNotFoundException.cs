// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         LogisticsTransportSafety.DriverQualifications
//  Layer:          Domain
//  File:           DriverQualificationNotFoundException.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Thrown when a DriverQualification aggregate lookup finds no match.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.LogisticsTransportSafety.DriverQualifications.Exceptions;

/// <summary>Thrown when a <see cref="DriverQualification"/> aggregate lookup finds no match.</summary>
public sealed class DriverQualificationNotFoundException : DriverQualificationDomainException
{
    /// <summary>Creates a not-found exception for the given DriverQualification identity.</summary>
    public DriverQualificationNotFoundException(DriverQualificationId id, CorrelationId correlationId, CausationId causationId)
        : base(
            $"DriverQualification with id '{id}' was not found.",
            "DriverQualification.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
