// AUTHOR: PlantSafety Platform Team (Phase 7 §6.3 Support Tooling)
// CREATED: 2026-08-03
// FILE DESCRIPTION: Query resolving a single impersonation session's current state, used by the
//   Admin Portal's persistent impersonation banner to poll whether a session is still active
//   and how much time-box remains.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Support.Impersonation.Models;

namespace PlantSafety.Application.Support.Impersonation.Queries;

/// <summary>Retrieves a single impersonation session's current state by identity.</summary>
/// <param name="SessionId">The session to retrieve.</param>
public sealed record GetActiveImpersonationSessionQuery(Guid SessionId) : IRequest<Result<ImpersonationSessionDto>>;
