// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.LocalEmergencyCoordination
//  Layer:          Application
//  File:           MutualAidAgreementItemDto.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Read-model item DTO for a single mutual aid agreement, backing the Mutual Aid
//                  Agreement Registry dashboard and ListMutualAidAgreementsQuery.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.SafetyEvents.LocalEmergencyCoordination.Models;

/// <summary>A single mutual aid agreement tracked by the LocalEmergencyCoordination module.</summary>
public sealed record MutualAidAgreementItemDto
{
    /// <summary>Agreement identifier.</summary>
    public required Guid MutualAidAgreementId { get; init; }

    /// <summary>Identity of the parent LocalEmergencyCoordination case.</summary>
    public required Guid LocalEmergencyCoordinationId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Name of the external agency.</summary>
    public required string AgencyName { get; init; }

    /// <summary>Discipline of the responding agency (Fire/Police/EMS/MutualAid).</summary>
    public required string Discipline { get; init; }

    /// <summary>Agreement document version.</summary>
    public int DocumentVersion { get; init; }

    /// <summary>UTC instant this agreement version became effective.</summary>
    public required DateTime EffectiveFromUtc { get; init; }

    /// <summary>UTC instant this agreement is next due for review/renewal.</summary>
    public required DateTime RenewalDueAtUtc { get; init; }

    /// <summary>Renewal/currency status (Current/PendingRenewal/Expired) -- backs the agreement currency KPI.</summary>
    public required string RenewalStatus { get; init; }

    /// <summary>UTC instant the agreement was established.</summary>
    public required DateTime EstablishedAtUtc { get; init; }

    /// <summary>Identity of the user who established the agreement.</summary>
    public required Guid EstablishedBy { get; init; }
}
