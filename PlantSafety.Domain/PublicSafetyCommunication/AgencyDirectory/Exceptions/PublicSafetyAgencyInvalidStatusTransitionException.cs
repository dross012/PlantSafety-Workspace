// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PublicSafetyCommunication.AgencyDirectory
//  Layer:          Domain
//  File:           PublicSafetyAgencyInvalidStatusTransitionException.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a PublicSafetyAgency lifecycle transition is not permitted.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PublicSafetyCommunication.AgencyDirectory.Exceptions;

/// <summary>Thrown when a <see cref="PublicSafetyAgency"/> lifecycle transition is not permitted.</summary>
public sealed class PublicSafetyAgencyInvalidStatusTransitionException : PublicSafetyAgencyDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public PublicSafetyAgencyInvalidStatusTransitionException(
        PublicSafetyAgencyId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            "Cannot transition 'PublicSafetyAgency' {id} from '{from}' to '{to}'.",
            "PublicSafetyAgency.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
