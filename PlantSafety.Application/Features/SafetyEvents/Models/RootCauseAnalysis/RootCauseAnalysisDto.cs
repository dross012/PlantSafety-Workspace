// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-side DTO for a Root Cause Analysis (RCA) record.
// SchemaVersion: 1

namespace PlantSafety.Application.Features.SafetyEvents.Models.RootCauseAnalysis;

/// <summary>
/// Read-side projection of a RootCauseAnalysis record.
/// An RCA is linked to an Incident or NearMiss and documents the causal chain.
/// </summary>
public sealed record RootCauseAnalysisDto
{
    /// <summary>Unique identifier (UUIDv7).</summary>
    public required Guid RcaId { get; init; }

    /// <summary>Tenant owning this record.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>The Incident or NearMiss this RCA is linked to.</summary>
    public required Guid LinkedEventId { get; init; }

    /// <summary>Type of linked event: Incident, NearMiss.</summary>
    public required string LinkedEventType { get; init; }

    /// <summary>RCA methodology used (e.g., 5Why, FishboneDiagram, FaultTree).</summary>
    public required string Methodology { get; init; }

    /// <summary>Identified root causes (structured as JSON or delimited text).</summary>
    public required string RootCauses { get; init; }

    /// <summary>Corrective actions recommended as output of this RCA.</summary>
    public required string CorrectiveActions { get; init; }

    /// <summary>Workflow status (e.g., Draft, UnderReview, Approved, Closed).</summary>
    public required string Status { get; init; }

    /// <summary>Worker who led this RCA.</summary>
    public required Guid LeadInvestigatorId { get; init; }

    /// <summary>UTC timestamp when the RCA was completed, if applicable.</summary>
    public DateTimeOffset? CompletedAtUtc { get; init; }

    /// <summary>UTC timestamp of last modification.</summary>
    public required DateTimeOffset LastModifiedAtUtc { get; init; }

    /// <summary>Row version for concurrency.</summary>
    public required string ETag { get; init; }
}
