// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Client-side DTO for a single Hazard Assessment read-model
//   record. Mirrors PlantSafety.Application.Operational.HazardAssessments.
//   Models.HazardAssessmentsItemDto field-for-field so System.Text.Json can
//   deserialize the API response without a manual mapping step. No domain
//   logic present — display only.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §1.4
// ============================================================

using System;

namespace PlantSafety.UI.Modules.HazardAssessment.Models;

/// <summary>A single Hazard Assessment read-model record returned by the API.</summary>
public sealed class HazardAssessmentDto
{
    /// <summary>Gets the Hazard Assessment record identifier.</summary>
    public Guid HazardAssessmentId { get; init; }

    /// <summary>Gets the tenant identifier.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Gets the site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Gets the description of the identified hazard.</summary>
    public string HazardDescription { get; init; } = string.Empty;

    /// <summary>Gets the risk level classification (e.g., Low/Medium/High/Critical).</summary>
    public string RiskLevel { get; init; } = string.Empty;

    /// <summary>Gets the current status of this record.</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Gets the UTC timestamp when this record was last actioned.</summary>
    public DateTime AssessedAtUtc { get; init; }

    /// <summary>Gets the identity of the user who last actioned this record.</summary>
    public Guid AssessedBy { get; init; }
}
