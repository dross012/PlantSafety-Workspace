// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Client-side DTOs for the AI Investigation module. Mirrors
//   the real PlantSafety.Api read-model shapes (AllInvestigationsItemDto /
//   AllInvestigationsResponse at GET v1/intelligence/allinvestigations).
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §1.4
// ============================================================

using System;
using System.Collections.Generic;

namespace PlantSafety.UI.Modules.AiInvestigation.Models;

/// <summary>Represents a single AI-assisted safety investigation record (mirrors <c>AllInvestigationsItemDto</c>).</summary>
public sealed class AiInvestigationItemDto
{
    /// <summary>Gets the unique identifier of the AI-assisted investigation record.</summary>
    public Guid InvestigationId { get; init; }

    /// <summary>Gets the tenant identifier.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Gets the site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Gets the reference identifier of the related safety incident.</summary>
    public string IncidentReference { get; init; } = string.Empty;

    /// <summary>Gets the AI model confidence score (0.0-1.0) for the investigation findings.</summary>
    public double AiConfidenceScore { get; init; }

    /// <summary>Gets the AI-generated summary of the investigation.</summary>
    public string? Summary { get; init; }

    /// <summary>Gets the current investigation status (e.g., Open/InProgress/Closed).</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Gets the UTC timestamp when the investigation was opened.</summary>
    public DateTime OpenedAtUtc { get; init; }

    /// <summary>Gets the identity of the user or process assigned to the investigation.</summary>
    public Guid AssignedTo { get; init; }
}

/// <summary>Envelope returned by GET v1/intelligence/allinvestigations (mirrors <c>AllInvestigationsResponse</c>).</summary>
public sealed class AiInvestigationResponseDto
{
    /// <summary>Gets the tenant the data belongs to.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Gets the site the data is scoped to, or null for tenant-wide results.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Gets the UTC timestamp when this read-model snapshot was generated.</summary>
    public DateTime GeneratedAt { get; init; }

    /// <summary>Gets the investigation items in the requested window.</summary>
    public IReadOnlyList<AiInvestigationItemDto> Items { get; init; } = [];
}

/// <summary>Full read-model record for a single AIInvestigation (mirrors <c>AIInvestigationDetailDto</c>), backing GET v1/intelligence/ai-investigations/{id}.</summary>
public sealed class AiInvestigationDetailDto
{
    /// <summary>Gets the record identifier.</summary>
    public Guid AIInvestigationId { get; init; }

    /// <summary>Gets the tenant identifier.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Gets the site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Gets the current lifecycle status (Open/InProgress/Closed/Archived).</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Gets the reference identifier of the related safety incident.</summary>
    public string IncidentReference { get; init; } = string.Empty;

    /// <summary>Gets the AI model confidence score (0.0-1.0) for the investigation findings.</summary>
    public double AiConfidenceScore { get; init; }

    /// <summary>Gets the AI-generated summary of the investigation.</summary>
    public string? Summary { get; init; }

    /// <summary>Gets the identity of the user or process assigned to the investigation.</summary>
    public Guid AssignedTo { get; init; }

    /// <summary>Gets the UTC timestamp when the investigation was opened.</summary>
    public DateTime OpenedAtUtc { get; init; }

    /// <summary>Gets the identity of the user who started review, if applicable.</summary>
    public Guid? ReviewStartedBy { get; init; }

    /// <summary>Gets the UTC instant review started.</summary>
    public DateTime? ReviewStartedAtUtc { get; init; }

    /// <summary>Gets the identity of the user who closed the investigation, if applicable.</summary>
    public Guid? ClosedBy { get; init; }

    /// <summary>Gets the resolution summary supplied when closing the investigation.</summary>
    public string? ResolutionSummary { get; init; }

    /// <summary>Gets the UTC instant the investigation was closed.</summary>
    public DateTime? ClosedAtUtc { get; init; }

    /// <summary>Gets the UTC instant the investigation was archived for compliance retention.</summary>
    public DateTime? ArchivedAtUtc { get; init; }

    /// <summary>Gets the UTC timestamp when the record was created.</summary>
    public DateTime CreatedAtUtc { get; init; }

    /// <summary>Gets the identity of the user who created this record.</summary>
    public Guid CreatedBy { get; init; }

    /// <summary>Gets the optimistic-concurrency token. Must be echoed back on subsequent mutating commands.</summary>
    public string? ETag { get; init; }
}

/// <summary>Result payload shared by AIInvestigation lifecycle commands (mirrors <c>AIInvestigationStatusResult</c>).</summary>
public sealed class AiInvestigationStatusResultDto
{
    /// <summary>Gets the identity of the affected AIInvestigation aggregate.</summary>
    public Guid AIInvestigationId { get; init; }

    /// <summary>Gets the lifecycle status after the transition.</summary>
    public string Status { get; init; } = string.Empty;
}

/// <summary>Request body for opening a new AI-assisted safety investigation (POST v1/intelligence/ai-investigations).</summary>
public sealed class CreateAiInvestigationRequest
{
    /// <summary>Gets or sets the optional site scope. Null applies tenant-wide.</summary>
    public Guid? SiteId { get; set; }

    /// <summary>Gets or sets the reference identifier of the related safety incident.</summary>
    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.StringLength(100)]
    public string IncidentReference { get; set; } = string.Empty;

    /// <summary>Gets or sets the AI model confidence score (0.0-1.0) for the investigation findings.</summary>
    [System.ComponentModel.DataAnnotations.Range(0.0, 1.0)]
    public double AiConfidenceScore { get; set; }

    /// <summary>Gets or sets the AI-generated summary of the investigation.</summary>
    public string? Summary { get; set; }
}

/// <summary>Request body carrying only an optimistic-concurrency ETag (start-review/archive).</summary>
public sealed class EtagRequest
{
    /// <summary>Gets or sets the optimistic-concurrency token from the last read of this record. Null skips the check.</summary>
    public string? ETag { get; set; }
}

/// <summary>Request body for closing an AIInvestigation (POST .../close).</summary>
public sealed class CloseAiInvestigationRequest
{
    /// <summary>Gets or sets the resolution summary for the investigation.</summary>
    public string? ResolutionSummary { get; set; }

    /// <summary>Gets or sets the optimistic-concurrency token from the last read of this record. Null skips the check.</summary>
    public string? ETag { get; set; }
}
