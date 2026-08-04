// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PublicSafetyCommunication.AgencyDirectory
//  Layer:          Domain
//  File:           IPublicSafetyAgencyFactory.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Factory interface for constructing PublicSafetyAgency aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PublicSafetyCommunication.AgencyDirectory.Interfaces;

/// <summary>Factory interface for constructing <see cref="PublicSafetyAgency"/> aggregates.</summary>
public interface IPublicSafetyAgencyFactory
{
    /// <summary>Creates a new PublicSafetyAgency aggregate with the supplied parameters.</summary>
    PlantSafety.Domain.PublicSafetyCommunication.AgencyDirectory.Aggregates.PublicSafetyAgency Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor);
}
