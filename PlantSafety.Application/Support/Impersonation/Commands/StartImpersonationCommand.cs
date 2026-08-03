// AUTHOR: PlantSafety Platform Team (Phase 7 §6.3 Support Tooling)
// CREATED: 2026-08-03
// FILE DESCRIPTION: Command starting a real, time-boxed, audited tenant-impersonation session
//   for the platform Support Console. Security-sensitive by design: gated at PlatformAdmin tier
//   at the controller (ImpersonationAuthorizationPolicies.Manage), writes a real audit record via
//   IPermissionUsageLogRepository for every session start, and the resulting session always
//   carries a hard expiry -- there is no silent/unaudited/unbounded impersonation path.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Support.Impersonation.Models;

namespace PlantSafety.Application.Support.Impersonation.Commands;

/// <summary>Starts a time-boxed impersonation session of <paramref name="TenantId"/> by <paramref name="ActorId"/>.</summary>
/// <param name="TenantId">The tenant to impersonate.</param>
/// <param name="ActorId">The platform administrator starting the session.</param>
/// <param name="Reason">Mandatory, human-readable business reason for the impersonation (recorded in the audit trail).</param>
/// <param name="CorrelationId">Correlation identifier for this request, shared with the eventual EndImpersonationCommand's audit entry.</param>
public sealed record StartImpersonationCommand(
    Guid TenantId,
    Guid ActorId,
    string Reason,
    Guid CorrelationId) : IRequest<Result<ImpersonationSessionDto>>;
