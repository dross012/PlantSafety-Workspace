// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         LogisticsTransportSafety.DriverQualifications
//  Layer:          Domain
//  File:           IDriverQualificationFactory.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Factory interface for constructing DriverQualification aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.LogisticsTransportSafety.DriverQualifications.Interfaces;

/// <summary>Factory interface for constructing <see cref="DriverQualification"/> aggregates.</summary>
public interface IDriverQualificationFactory
{
    /// <summary>Creates a new DriverQualification aggregate with the supplied parameters.</summary>
    PlantSafety.Domain.LogisticsTransportSafety.DriverQualifications.Aggregates.DriverQualification Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor);
}
