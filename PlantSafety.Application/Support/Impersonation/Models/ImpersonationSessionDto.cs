// AUTHOR: PlantSafety Platform Team (Phase 7 §6.3 Support Tooling)
// CREATED: 2026-08-03
// FILE DESCRIPTION: Canonical read model for a time-boxed, audited tenant-impersonation
//   session. Every session is started/ended through StartImpersonationCommand/
//   EndImpersonationCommand, both of which write a real, queryable audit record to
//   [Auditing].[PermissionUsageLog] via the existing IPermissionUsageLogRepository --
//   there is no silent/unaudited impersonation path in this platform.
// SchemaVersion: 1

namespace PlantSafety.Application.Support.Impersonation.Models;

/// <summary>A single time-boxed tenant-impersonation session, active or ended.</summary>
/// <param name="SessionId">Unique identifier for this impersonation session.</param>
/// <param name="TenantId">The tenant being impersonated.</param>
/// <param name="AdminUserId">The platform administrator who started the session.</param>
/// <param name="Reason">The operator-supplied business reason for impersonating this tenant.</param>
/// <param name="StartedAtUtc">UTC instant the session began.</param>
/// <param name="ExpiresAtUtc">UTC instant the session will automatically expire (time-box), unless ended sooner.</param>
/// <param name="EndedAtUtc">UTC instant the session was explicitly ended, or <see langword="null"/> if still active/expired-only.</param>
/// <param name="CorrelationId">Correlation identifier shared by this session's start/end audit entries.</param>
public sealed record ImpersonationSessionDto(
    Guid SessionId,
    Guid TenantId,
    Guid AdminUserId,
    string Reason,
    DateTime StartedAtUtc,
    DateTime ExpiresAtUtc,
    DateTime? EndedAtUtc,
    Guid CorrelationId)
{
    /// <summary>Gets a value indicating whether the session is still live (not ended and not past its time-box).</summary>
    public bool IsActive(DateTime nowUtc) => EndedAtUtc is null && nowUtc < ExpiresAtUtc;
}
