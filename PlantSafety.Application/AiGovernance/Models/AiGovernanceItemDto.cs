// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AiGovernance
//  Layer:          Application
//  File:           AiGovernanceItemDto.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Flat read-model item DTO for a single AiGovernance record. Backs
//                  GetAiGovernanceByIdQuery. Mirrors
//                  PlantSafety.Application.Compliance.Auditing.Models.AuditingItemDto.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.AiGovernance.Models;

/// <summary>A single AiGovernance read-model record.</summary>
public sealed record AiGovernanceItemDto
{
    /// <summary>Record identifier.</summary>
    public required Guid AiGovernanceId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Current lifecycle status (Created/Active/Completed/Closed/Archived).</summary>
    public required string Status { get; init; }

    /// <summary>Name of the governed AI system.</summary>
    public required string SystemName { get; init; }

    /// <summary>Narrative description of the AI system's use case.</summary>
    public required string AiUseCase { get; init; }

    /// <summary>Risk classification: Low, Medium, High, Critical.</summary>
    public required string RiskClassification { get; init; }

    /// <summary>Identity of the accountable owner for this AI system.</summary>
    public Guid? OwnerUserId { get; init; }

    /// <summary>Whether this AI system requires human oversight before autonomous action.</summary>
    public bool RequiresHumanOversight { get; init; }

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
