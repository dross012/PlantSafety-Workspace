// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-08-04
// FILE DESCRIPTION: Client-side DTO for a mutual aid agreement. Mirrors
//   PlantSafety.Application.SafetyEvents.LocalEmergencyCoordination.Models.MutualAidAgreementItemDto.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;

namespace PlantSafety.UI.Modules.LocalEmergencyCoordination.Models;

/// <summary>Read-side projection of a mutual aid agreement with an external local emergency responder agency.</summary>
public sealed class MutualAidAgreementDto
{
    /// <summary>Gets the agreement identifier.</summary>
    public Guid MutualAidAgreementId { get; init; }

    /// <summary>Gets the identity of the parent LocalEmergencyCoordination case.</summary>
    public Guid LocalEmergencyCoordinationId { get; init; }

    /// <summary>Gets the name of the external agency.</summary>
    public string AgencyName { get; init; } = string.Empty;

    /// <summary>Gets the discipline of the responding agency (Fire/Police/EMS/MutualAid).</summary>
    public string Discipline { get; init; } = string.Empty;

    /// <summary>Gets the agreement document version.</summary>
    public int DocumentVersion { get; init; }

    /// <summary>Gets the UTC instant this agreement version became effective.</summary>
    public DateTimeOffset EffectiveFromUtc { get; init; }

    /// <summary>Gets the UTC instant this agreement is next due for review/renewal.</summary>
    public DateTimeOffset RenewalDueAtUtc { get; init; }

    /// <summary>Gets the renewal/currency status (Current/PendingRenewal/Expired) -- backs the agreement currency KPI.</summary>
    public string RenewalStatus { get; init; } = string.Empty;
}
