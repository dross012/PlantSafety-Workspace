// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-model item DTO for a single root cause analysis record.
// SchemaVersion: 1

namespace PlantSafety.Application.SafetyEvents.RootCauseAnalysis.Models;

/// <summary>A single root cause analysis performed against a safety event.</summary>
public sealed record RootCauseAnalysisItemDto
{
    /// <summary>Root cause analysis identifier.</summary>
    public required Guid AnalysisId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Identifier of the related incident, near-miss, or other safety event, if any.</summary>
    public Guid? RelatedEventId { get; init; }

    /// <summary>Analysis methodology used (e.g., FiveWhys/FishboneDiagram/FaultTree/Bowtie).</summary>
    public required string Methodology { get; init; }

    /// <summary>Current analysis status (e.g., InProgress/PendingReview/Completed).</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when the analysis was completed.</summary>
    public required DateTime CompletedAtUtc { get; init; }

    /// <summary>Identity of the lead analyst.</summary>
    public required Guid LeadAnalyst { get; init; }
}
