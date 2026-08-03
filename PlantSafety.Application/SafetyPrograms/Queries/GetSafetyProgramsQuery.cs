// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-03
// FILE DESCRIPTION: Query to retrieve a paginated, optionally status-filtered page of
//   SafetyProgram list rows for a tenant.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.SafetyPrograms.Models;

namespace PlantSafety.Application.SafetyPrograms.Queries;

/// <summary>Retrieves a paginated page of SafetyPrograms for the given tenant.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="SiteId">Optional site filter. Null returns data for all accessible sites.</param>
/// <param name="StatusFilter">Optional status filter (Planned/Active/OnHold/Completed).</param>
/// <param name="PageNumber">One-based page number.</param>
/// <param name="PageSize">Items per page.</param>
public sealed record GetSafetyProgramsQuery(
    Guid TenantId,
    Guid? SiteId,
    string? StatusFilter,
    int PageNumber,
    int PageSize) : IRequest<Result<PaginatedResult<SafetyProgramListItemDto>>>;
