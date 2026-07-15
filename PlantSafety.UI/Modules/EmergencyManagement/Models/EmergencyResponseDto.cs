// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Client-side DTO for an Emergency Response record. Mirrors
//   PlantSafety.Application.Features.SafetyEvents.Models.EmergencyManagement
//   .EmergencyResponseDto. The backend currently exposes a Create (activate)
//   endpoint only — List/GetById are modeled against this best-inferable
//   DTO shape.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §1.4
// ============================================================

using System;

namespace PlantSafety.UI.Modules.EmergencyManagement.Models;

/// <summary>Read-side projection of an Emergency Response — activation and execution of an emergency response plan.</summary>
public sealed class EmergencyResponseDto
{
    /// <summary>Gets the unique emergency response identifier (UUIDv7).</summary>
    public Guid EmergencyResponseId { get; init; }

    /// <summary>Gets the owning tenant identifier.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Gets the site where the emergency occurred.</summary>
    public Guid SiteId { get; init; }

    /// <summary>Gets the linked Crisis Event identifier, if this response is tied to a declared crisis.</summary>
    public Guid? LinkedCrisisEventId { get; init; }

    /// <summary>Gets the emergency response plan that was activated.</summary>
    public string ResponsePlanName { get; init; } = string.Empty;

    /// <summary>Gets the UTC timestamp when the response was activated.</summary>
    public DateTimeOffset ActivatedAtUtc { get; init; }

    /// <summary>Gets the current status (e.g., Active, Standdown, Completed).</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Gets the response coordinator.</summary>
    public Guid CoordinatorId { get; init; }

    /// <summary>Gets the UTC timestamp when stand-down was declared, if applicable.</summary>
    public DateTimeOffset? StandDownAtUtc { get; init; }

    /// <summary>Gets the UTC timestamp of last modification.</summary>
    public DateTimeOffset LastModifiedAtUtc { get; init; }

    /// <summary>Gets the row version for concurrency.</summary>
    public string ETag { get; init; } = string.Empty;
}
