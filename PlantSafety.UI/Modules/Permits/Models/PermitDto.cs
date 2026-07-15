// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Client-side DTO for a single Permits to Work read-model
//   record. Mirrors PlantSafety.Application.Operational.PermitsToWork.Models.
//   PermitsToWorkItemDto field-for-field so System.Text.Json can deserialize
//   the API response without a manual mapping step. No domain logic present
//   — display only.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §1.4
// ============================================================

using System;

namespace PlantSafety.UI.Modules.Permits.Models;

/// <summary>A single Permits to Work read-model record returned by the API.</summary>
public sealed class PermitDto
{
    /// <summary>Gets the Permit record identifier.</summary>
    public Guid PermitId { get; init; }

    /// <summary>Gets the tenant identifier.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Gets the site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Gets the type of work permit issued.</summary>
    public string PermitType { get; init; } = string.Empty;

    /// <summary>Gets the description of the work being performed.</summary>
    public string WorkDescription { get; init; } = string.Empty;

    /// <summary>Gets the current status of this record.</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Gets the UTC timestamp when this record was last actioned.</summary>
    public DateTime IssuedAtUtc { get; init; }

    /// <summary>Gets the identity of the user who last actioned this record.</summary>
    public Guid IssuedBy { get; init; }
}
