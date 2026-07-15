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
