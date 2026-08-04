// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-04
// FILE DESCRIPTION: Query to retrieve a single CommunityConcern by identity.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.CommunitySafety.CommunityConcern.Models;

namespace PlantSafety.Application.CommunitySafety.CommunityConcern.Queries;

/// <summary>Retrieves a single CommunityConcern record by identity.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="CommunityConcernId">Identity of the CommunityConcern to retrieve.</param>
public sealed record GetCommunityConcernByIdQuery(
    Guid TenantId,
    Guid CommunityConcernId) : IRequest<Result<CommunityConcernItemDto>>;
