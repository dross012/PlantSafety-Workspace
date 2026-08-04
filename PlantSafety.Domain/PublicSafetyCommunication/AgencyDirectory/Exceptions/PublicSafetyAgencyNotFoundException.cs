// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PublicSafetyCommunication.AgencyDirectory
//  Layer:          Domain
//  File:           PublicSafetyAgencyNotFoundException.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a PublicSafetyAgency aggregate lookup finds no match.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PublicSafetyCommunication.AgencyDirectory.Exceptions;

/// <summary>Thrown when a <see cref="PublicSafetyAgency"/> aggregate lookup finds no match.</summary>
public sealed class PublicSafetyAgencyNotFoundException : PublicSafetyAgencyDomainException
{
    /// <summary>Creates a not-found exception for the given PublicSafetyAgency identity.</summary>
    public PublicSafetyAgencyNotFoundException(PublicSafetyAgencyId id, CorrelationId correlationId, CausationId causationId)
        : base(
            "PublicSafetyAgency with id '{id}' was not found.",
            "PublicSafetyAgency.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
