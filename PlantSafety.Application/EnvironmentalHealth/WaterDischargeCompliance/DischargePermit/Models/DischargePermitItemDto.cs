// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Read-model item DTO for a single DischargePermit record.
// SchemaVersion: 1

namespace PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.DischargePermit.Models;

/// <summary>A single DischargePermit read-model record.</summary>
public sealed record DischargePermitItemDto
{
    /// <summary>Record identifier.</summary>
    public required Guid DischargePermitId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Regulator-assigned permit number.</summary>
    public required string PermitNumber { get; init; }

    /// <summary>Regulatory program classification.</summary>
    public required string RegulatoryProgram { get; init; }

    /// <summary>Issuing authority.</summary>
    public required string IssuingAuthority { get; init; }

    /// <summary>UTC effective date.</summary>
    public required DateTime EffectiveAtUtc { get; init; }

    /// <summary>UTC expiration date.</summary>
    public required DateTime ExpiresAtUtc { get; init; }

    /// <summary>Number of renewals recorded.</summary>
    public required int RenewalCount { get; init; }

    /// <summary>Lifecycle status.</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when this record was created.</summary>
    public required DateTime CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public required Guid CreatedBy { get; init; }
}