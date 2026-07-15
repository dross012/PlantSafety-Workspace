// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Client-side DTO for a Root Cause Analysis (RCA) record.
//   Mirrors PlantSafety.Application.Features.SafetyEvents.Models.RootCauseAnalysis
//   .RootCauseAnalysisDto returned by GET /v1/features/safety-events/root-cause-analysis/{id}.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §1.4
// ============================================================

using System;

namespace PlantSafety.UI.Modules.Rca.Models;

/// <summary>Read-side projection of a Root Cause Analysis, linked to an Incident or Near Miss.</summary>
public sealed class RootCauseAnalysisDto
{
    /// <summary>Gets the unique RCA identifier (UUIDv7).</summary>
    public Guid RcaId { get; init; }

    /// <summary>Gets the owning tenant identifier.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Gets the identifier of the Incident or Near Miss this RCA is linked to.</summary>
    public Guid LinkedEventId { get; init; }

    /// <summary>Gets the type of linked event: Incident, NearMiss.</summary>
    public string LinkedEventType { get; init; } = string.Empty;

    /// <summary>Gets the RCA methodology used (e.g., 5Why, FishboneDiagram, FaultTree).</summary>
    public string Methodology { get; init; } = string.Empty;

    /// <summary>Gets the identified root causes (structured as JSON or delimited text).</summary>
    public string RootCauses { get; init; } = string.Empty;

    /// <summary>Gets the corrective actions recommended as output of this RCA.</summary>
    public string CorrectiveActions { get; init; } = string.Empty;

    /// <summary>Gets the workflow status (e.g., Draft, UnderReview, Approved, Closed).</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Gets the worker who led this RCA.</summary>
    public Guid LeadInvestigatorId { get; init; }

    /// <summary>Gets the UTC timestamp when the RCA was completed, if applicable.</summary>
    public DateTimeOffset? CompletedAtUtc { get; init; }

    /// <summary>Gets the UTC timestamp of last modification.</summary>
    public DateTimeOffset LastModifiedAtUtc { get; init; }

    /// <summary>Gets the row version for concurrency.</summary>
    public string ETag { get; init; } = string.Empty;
}
