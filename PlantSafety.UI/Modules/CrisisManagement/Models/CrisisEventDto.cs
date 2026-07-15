// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Client-side DTO for a Crisis Management event. Mirrors
//   PlantSafety.Application.Features.SafetyEvents.Models.CrisisManagement
//   .CrisisEventDto. The backend currently exposes a Create endpoint only
//   (POST /v1/features/safety-events/crisis-events) — List/GetById are
//   modeled here against this best-inferable DTO shape so the UI is ready
//   the moment those read endpoints ship.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §1.4
// ============================================================

using System;

namespace PlantSafety.UI.Modules.CrisisManagement.Models;

/// <summary>Read-side projection of a Crisis Event — a high-severity, multi-stakeholder safety emergency.</summary>
public sealed class CrisisEventDto
{
    /// <summary>Gets the unique crisis event identifier (UUIDv7).</summary>
    public Guid CrisisEventId { get; init; }

    /// <summary>Gets the owning tenant identifier.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Gets the site affected by the crisis.</summary>
    public Guid SiteId { get; init; }

    /// <summary>Gets the short title.</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>Gets the detailed description of the crisis.</summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>Gets the crisis category (e.g., Fire, ChemicalSpill, StructuralFailure, MassInjury).</summary>
    public string CrisisCategory { get; init; } = string.Empty;

    /// <summary>Gets the UTC timestamp when the crisis was declared.</summary>
    public DateTimeOffset DeclaredAtUtc { get; init; }

    /// <summary>Gets the current workflow status (e.g., Active, Contained, Resolved, ClosedOut).</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Gets the Incident Commander for this crisis.</summary>
    public Guid IncidentCommanderId { get; init; }

    /// <summary>Gets the UTC timestamp when the crisis was resolved, if applicable.</summary>
    public DateTimeOffset? ResolvedAtUtc { get; init; }

    /// <summary>Gets the UTC timestamp of last modification.</summary>
    public DateTimeOffset LastModifiedAtUtc { get; init; }

    /// <summary>Gets the row version for concurrency.</summary>
    public string ETag { get; init; } = string.Empty;
}
