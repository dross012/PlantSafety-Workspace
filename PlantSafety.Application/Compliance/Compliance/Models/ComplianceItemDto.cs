// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Compliance
//  Layer:          Application
//  File:           ComplianceItemDto.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Flat read-model item DTO for a single (self-named) Compliance record. Backs
//                  GetComplianceByIdQuery. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Models.PermitsItemDto.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Compliance.Compliance.Models;

/// <summary>A single Compliance read-model record.</summary>
public sealed record ComplianceItemDto
{
    /// <summary>Record identifier.</summary>
    public required Guid ComplianceId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Current lifecycle status (Created/Active/Completed/Closed/Archived).</summary>
    public required string Status { get; init; }

    /// <summary>Regulation/standard reference this record evaluates.</summary>
    public required string RegulationReference { get; init; }

    /// <summary>Narrative description of the rule being evaluated.</summary>
    public required string RuleDescription { get; init; }

    /// <summary>Computed compliance score (0-100) for this rule/regulation.</summary>
    public decimal ComplianceScore { get; init; }

    /// <summary>Count of open non-conformances tied to this compliance record.</summary>
    public int NonConformanceCount { get; init; }

    /// <summary>Identity of the party accountable for maintaining compliance.</summary>
    public Guid? ResponsiblePartyId { get; init; }

    /// <summary>UTC timestamp this compliance record was last evaluated.</summary>
    public DateTime? LastEvaluatedAtUtc { get; init; }

    /// <summary>UTC timestamp when the record was created.</summary>
    public required DateTime CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public required Guid CreatedBy { get; init; }

    /// <summary>
    /// Optimistic-concurrency token (native RowVersion projected as a hex string). Callers must
    /// echo this value back on a subsequent mutating command's ETag field; a stale/mismatched
    /// value results in a concurrency-conflict error.
    /// </summary>
    public string? ETag { get; init; }
}
