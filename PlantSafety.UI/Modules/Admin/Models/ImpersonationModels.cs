// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-08-03
// FILE DESCRIPTION: Client-side DTOs for the Admin Portal's Support Console impersonation
//   feature (Phase 7 §6.3 Support Tooling). Mirrors
//   PlantSafety.Application.Support.Impersonation.Models.ImpersonationSessionDto and the
//   ImpersonationV1Controller request contract.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;

namespace PlantSafety.UI.Modules.Admin.Models;

/// <summary>A single time-boxed tenant-impersonation session, active or ended (mirrors <c>ImpersonationSessionDto</c>).</summary>
public sealed class ImpersonationSessionDto
{
    /// <summary>Gets the unique identifier for this impersonation session.</summary>
    public Guid SessionId { get; init; }

    /// <summary>Gets the tenant being impersonated.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Gets the platform administrator who started the session.</summary>
    public Guid AdminUserId { get; init; }

    /// <summary>Gets the operator-supplied business reason for impersonating this tenant.</summary>
    public string Reason { get; init; } = string.Empty;

    /// <summary>Gets the UTC instant the session began.</summary>
    public DateTime StartedAtUtc { get; init; }

    /// <summary>Gets the UTC instant the session will automatically expire (time-box).</summary>
    public DateTime ExpiresAtUtc { get; init; }

    /// <summary>Gets the UTC instant the session was explicitly ended, or null.</summary>
    public DateTime? EndedAtUtc { get; init; }

    /// <summary>Gets the correlation identifier shared by this session's start/end audit entries.</summary>
    public Guid CorrelationId { get; init; }
}

/// <summary>Request body for starting a tenant-impersonation session.</summary>
public sealed class StartImpersonationRequest
{
    /// <summary>Gets or sets the mandatory, human-readable business reason for the impersonation.</summary>
    public string Reason { get; set; } = string.Empty;
}
