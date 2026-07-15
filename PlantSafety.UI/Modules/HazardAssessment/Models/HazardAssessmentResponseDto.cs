// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Client-side DTO mirroring PlantSafety.Api.Operational.
//   HazardAssessments.Contracts.Responses.HazardAssessmentsResponse — the
//   payload nested inside the server's ApiResponse&lt;T&gt; envelope for
//   GET api/v1/operational/hazard-assessments.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §1.4
// ============================================================

using System;
using System.Collections.Generic;

namespace PlantSafety.UI.Modules.HazardAssessment.Models;

/// <summary>Response payload for Hazard Assessment read-model queries.</summary>
public sealed class HazardAssessmentResponseDto
{
    /// <summary>Gets the tenant the data belongs to.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Gets the site the data is scoped to, or null for tenant-wide results.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Gets the UTC timestamp when this read-model snapshot was generated.</summary>
    public DateTime GeneratedAt { get; init; }

    /// <summary>Gets the matching Hazard Assessment records. Never null; empty when no results.</summary>
    public IReadOnlyList<HazardAssessmentDto> Items { get; init; } = [];
}
