// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Client-side DTO for a single Job Safety Analysis (JSA)
//   read-model record. Mirrors PlantSafety.Application.Operational.
//   JobSafetyAnalysis.Models.JobSafetyAnalysisItemDto field-for-field so
//   System.Text.Json can deserialize the API response without a manual
//   mapping step. No domain logic present — display only.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §1.4
// ============================================================

using System;

namespace PlantSafety.UI.Modules.Jsa.Models;

/// <summary>A single JSA read-model record returned by the API.</summary>
public sealed class JsaDto
{
    /// <summary>Gets the JSA record identifier.</summary>
    public Guid JobSafetyAnalysisId { get; init; }

    /// <summary>Gets the tenant identifier.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Gets the site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Gets the name of the task being analysed.</summary>
    public string TaskName { get; init; } = string.Empty;

    /// <summary>Gets the risk level classification (e.g., Low/Medium/High/Critical).</summary>
    public string RiskLevel { get; init; } = string.Empty;

    /// <summary>Gets the current status of this record.</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Gets the UTC timestamp when this record was last actioned.</summary>
    public DateTime AnalyzedAtUtc { get; init; }

    /// <summary>Gets the identity of the user who last actioned this record.</summary>
    public Guid AnalyzedBy { get; init; }
}
