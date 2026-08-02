// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HumanFactors
//  Layer:          Application
//  File:           HumanFactorsItemDto.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Flat read-model item DTO for a single HumanFactors record. Backs
//                  GetHumanFactorsByIdQuery. Mirrors
//                  PlantSafety.Application.Compliance.Auditing.Models.AuditingItemDto.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.HumanFactors.Models;

/// <summary>A single HumanFactors read-model record.</summary>
public sealed record HumanFactorsItemDto
{
    /// <summary>Record identifier.</summary>
    public required Guid HumanFactorsId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Current lifecycle status (Created/Active/Completed/Closed/Archived).</summary>
    public required string Status { get; init; }

    /// <summary>Human-factors classification (e.g. Fatigue, CognitiveLoad, WorkloadStress, Communication).</summary>
    public required string FactorType { get; init; }

    /// <summary>Identity of the worker affected by this assessment, if applicable.</summary>
    public Guid? AffectedUserId { get; init; }

    /// <summary>Severity rating on a 1-5 scale.</summary>
    public int SeverityRating { get; init; }

    /// <summary>Narrative description of the assessed human factor.</summary>
    public required string Description { get; init; }

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
