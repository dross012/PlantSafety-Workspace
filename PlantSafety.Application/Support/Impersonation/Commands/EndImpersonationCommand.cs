// AUTHOR: PlantSafety Platform Team (Phase 7 §6.3 Support Tooling)
// CREATED: 2026-08-03
// FILE DESCRIPTION: Command ending an active tenant-impersonation session. Writes a real audit
//   record via IPermissionUsageLogRepository, mirroring StartImpersonationCommand.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Support.Impersonation.Models;

namespace PlantSafety.Application.Support.Impersonation.Commands;

/// <summary>Ends an active impersonation session (either explicit operator sign-out, or reaching its time-box).</summary>
/// <param name="SessionId">The session to end.</param>
/// <param name="ActorId">The platform administrator ending the session. Must match the session's own <c>AdminUserId</c>.</param>
public sealed record EndImpersonationCommand(Guid SessionId, Guid ActorId) : IRequest<Result<ImpersonationSessionDto>>;
