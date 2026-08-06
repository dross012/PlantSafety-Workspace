// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-08-05
// FILE DESCRIPTION: Client-side DTOs for two distinct RCA write surfaces:
//   1) Features.SafetyEvents.RootCauseAnalysis's real Create endpoint
//      (POST v1/features/safety-events/root-cause-analysis), the same
//      backend already used for the read-only RcaListPage/RcaDetailPage.
//   2) The separate, generic ContinuousImprovement.RCA aggregate
//      (api/v1/continuous-improvement/rca) — Create -> StartAnalysis ->
//      SubmitForApproval -> Close -> Archive, plus a real List (with
//      SiteId/From/To filters) and by-id GET. This is where the module's
//      real Search/Edit/Delete-equivalent lifecycle actions actually live,
//      since the Incident/NearMiss-linked RCA above has neither.
//   Mirrors the shapes in
//   PlantSafety.Api.Features.SafetyEvents.RootCauseAnalysis.Contracts and
//   PlantSafety.Api.ContinuousImprovement.RCA.Contracts.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PlantSafety.UI.Modules.Rca.Models;

// ── Features.SafetyEvents.RootCauseAnalysis Create surface ────────────────────

/// <summary>Request body for initiating an Incident/NearMiss-linked RCA (POST v1/features/safety-events/root-cause-analysis). Mirrors <c>CreateRootCauseAnalysisRequest</c>.</summary>
public sealed class CreateRcaRequest
{
    /// <summary>Gets or sets the Incident or NearMiss this RCA is linked to.</summary>
    [Required]
    public Guid LinkedEventId { get; set; }

    /// <summary>Gets or sets the type of linked event: Incident, NearMiss.</summary>
    [Required]
    [StringLength(50)]
    public string LinkedEventType { get; set; } = string.Empty;

    /// <summary>Gets or sets the RCA methodology (e.g., 5Why, FishboneDiagram, FaultTree).</summary>
    [Required]
    [StringLength(100)]
    public string Methodology { get; set; } = string.Empty;

    /// <summary>Gets or sets the worker leading this RCA.</summary>
    [Required]
    public Guid LeadInvestigatorId { get; set; }

    /// <summary>Gets or sets the root causes identified (serialised). Max 4000 characters.</summary>
    [Required]
    [StringLength(4000)]
    public string RootCauses { get; set; } = string.Empty;

    /// <summary>Gets or sets the corrective actions recommended (serialised). Max 4000 characters.</summary>
    [Required]
    [StringLength(4000)]
    public string CorrectiveActions { get; set; } = string.Empty;
}

/// <summary>Result of initiating a new Incident/NearMiss-linked RCA. Mirrors <c>CreateRootCauseAnalysisResponse</c>.</summary>
public sealed record CreateRcaResultDto(Guid RcaId);

// ── ContinuousImprovement.RCA write/lifecycle surface ──────────────────────────

/// <summary>A single ContinuousImprovement RCA list-row record. Mirrors <c>RCAItemDto</c>.</summary>
public sealed record RcaRecordItemDto
{
    /// <summary>RCA record identifier.</summary>
    public Guid RcaId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Problem statement being analyzed.</summary>
    public string ProblemStatement { get; init; } = string.Empty;

    /// <summary>Analysis methodology used (e.g., FiveWhys/FishboneDiagram/FaultTree/Bowtie).</summary>
    public string Methodology { get; init; } = string.Empty;

    /// <summary>Current RCA status (Initiated/InProgress/PendingApproval/Closed/Archived).</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>UTC timestamp when the RCA was initiated.</summary>
    public DateTime InitiatedAtUtc { get; init; }

    /// <summary>Identity of the user who owns the RCA.</summary>
    public Guid OwnerUserId { get; init; }
}

/// <summary>Response payload wrapping the ContinuousImprovement RCA list result set. Mirrors <c>RCAResponse</c>.</summary>
public sealed record RcaRecordsResponseDto
{
    /// <summary>Gets the tenant the data belongs to.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Gets the site the data is scoped to, or null for tenant-wide results.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Gets the UTC timestamp when this read-model snapshot was generated.</summary>
    public DateTime GeneratedAt { get; init; }

    /// <summary>Gets the matching RCA records. Never null; empty when no results.</summary>
    public IReadOnlyList<RcaRecordItemDto> Items { get; init; } = [];
}

/// <summary>Full read-model record for a single ContinuousImprovement RCA (mirrors <c>RCADetailDto</c>), backing GET v1/continuous-improvement/rca/{id}.</summary>
public sealed record RcaRecordDetailDto
{
    /// <summary>RCA record identifier.</summary>
    public Guid RcaId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Problem statement being analyzed.</summary>
    public string ProblemStatement { get; init; } = string.Empty;

    /// <summary>Analysis methodology used (e.g., FiveWhys/FishboneDiagram/FaultTree/Bowtie).</summary>
    public string Methodology { get; init; } = string.Empty;

    /// <summary>Current lifecycle status (Initiated/InProgress/PendingApproval/Closed/Archived).</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>UTC timestamp when the RCA was initiated.</summary>
    public DateTime InitiatedAtUtc { get; init; }

    /// <summary>Identity of the user who owns the RCA.</summary>
    public Guid OwnerUserId { get; init; }

    /// <summary>Identity of the user who started the analysis.</summary>
    public Guid? StartedBy { get; init; }

    /// <summary>UTC instant analysis work began.</summary>
    public DateTime? StartedAtUtc { get; init; }

    /// <summary>Identity of the user who submitted the RCA for approval.</summary>
    public Guid? SubmittedBy { get; init; }

    /// <summary>Findings recorded when the RCA was submitted for approval, if any.</summary>
    public string? Findings { get; init; }

    /// <summary>UTC instant the RCA was submitted for approval.</summary>
    public DateTime? SubmittedAtUtc { get; init; }

    /// <summary>Identity of the user who formally closed the RCA.</summary>
    public Guid? ClosedBy { get; init; }

    /// <summary>Root cause summary recorded when the RCA was closed, if any.</summary>
    public string? RootCauseSummary { get; init; }

    /// <summary>UTC instant the RCA was closed.</summary>
    public DateTime? ClosedAtUtc { get; init; }

    /// <summary>UTC instant the RCA was archived for compliance retention.</summary>
    public DateTime? ArchivedAtUtc { get; init; }

    /// <summary>UTC timestamp when the record was created.</summary>
    public DateTime CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public Guid CreatedBy { get; init; }

    /// <summary>Optimistic-concurrency token. Must be echoed back on subsequent mutating commands.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for creating a new generic ContinuousImprovement RCA record (POST v1/continuous-improvement/rca). Mirrors <c>CreateRCARequest</c>.</summary>
public sealed class CreateRcaRecordRequest
{
    /// <summary>Gets or sets the optional site scope. Null applies tenant-wide.</summary>
    public Guid? SiteId { get; set; }

    /// <summary>Gets or sets the problem statement being analyzed.</summary>
    [Required]
    [StringLength(4000)]
    public string ProblemStatement { get; set; } = string.Empty;

    /// <summary>Gets or sets the analysis methodology used (e.g., FiveWhys/FishboneDiagram/FaultTree/Bowtie).</summary>
    [Required]
    [StringLength(50)]
    public string Methodology { get; set; } = string.Empty;
}

/// <summary>Result payload shared by the ContinuousImprovement RCA lifecycle commands. Mirrors <c>RCAStatusResult</c>.</summary>
public sealed record RcaRecordStatusResultDto(Guid RcaId, string Status);

/// <summary>Request body carrying only an optimistic-concurrency ETag (start-analysis/archive).</summary>
public sealed class RcaEtagRequest
{
    /// <summary>Gets or sets the optimistic-concurrency token from the last read of this record. Null skips the check.</summary>
    public string? ETag { get; set; }
}

/// <summary>Request body for submitting the RCA record for approval (POST .../submit-for-approval). Mirrors <c>SubmitForApprovalRCARequest</c>.</summary>
public sealed class SubmitRcaRecordForApprovalRequest
{
    /// <summary>Gets or sets findings recorded at submission time, if any.</summary>
    [StringLength(4000)]
    public string? Findings { get; set; }

    /// <summary>Gets or sets the optimistic-concurrency token from the last read of this record. Null skips the check.</summary>
    public string? ETag { get; set; }
}

/// <summary>Request body for approving and formally closing the RCA record (POST .../close). Mirrors <c>CloseRCARequest</c>.</summary>
public sealed class CloseRcaRecordRequest
{
    /// <summary>Gets or sets the root cause summary recorded at closure time, if any.</summary>
    [StringLength(4000)]
    public string? RootCauseSummary { get; set; }

    /// <summary>Gets or sets the optimistic-concurrency token from the last read of this record. Null skips the check.</summary>
    public string? ETag { get; set; }
}
