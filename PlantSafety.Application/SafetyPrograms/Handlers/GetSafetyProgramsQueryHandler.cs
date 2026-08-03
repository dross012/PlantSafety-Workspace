// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-03
// FILE DESCRIPTION: Handler for GetSafetyProgramsQuery. Pure read: pages SafetyProgram
//   aggregates via the domain repository and projects them to the list DTO. No composition
//   with sibling modules happens here -- see GetSafetyProgramDetailQueryHandler for that.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Domain.SafetyPrograms.SafetyProgram.Enums;
using PlantSafety.Domain.SafetyPrograms.SafetyProgram.Interfaces;

using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.SafetyPrograms.Models;
using PlantSafety.Application.SafetyPrograms.Queries;

namespace PlantSafety.Application.SafetyPrograms.Handlers;

/// <summary>Handles <see cref="GetSafetyProgramsQuery"/>.</summary>
public sealed class GetSafetyProgramsQueryHandler : IRequestHandler<GetSafetyProgramsQuery, Result<PaginatedResult<SafetyProgramListItemDto>>>
{
    private readonly ISafetyProgramRepository _repository;
    private readonly ILogger<GetSafetyProgramsQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetSafetyProgramsQueryHandler(ISafetyProgramRepository repository, ILogger<GetSafetyProgramsQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<PaginatedResult<SafetyProgramListItemDto>>> Handle(GetSafetyProgramsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            SafetyProgramStatus? statusFilter = null;
            if (!string.IsNullOrWhiteSpace(request.StatusFilter))
            {
                if (!Enum.TryParse<SafetyProgramStatus>(request.StatusFilter, ignoreCase: true, out var parsed))
                {
                    return Result<PaginatedResult<SafetyProgramListItemDto>>.Failure(new ApplicationError(
                        "SafetyPrograms.SafetyProgram.InvalidStatusFilter",
                        $"Status '{request.StatusFilter}' is not a recognized SafetyProgram status.",
                        Guid.NewGuid(),
                        ErrorSeverity.Validation));
                }
                statusFilter = parsed;
            }

            var pageNumber = Math.Max(1, request.PageNumber);
            var pageSize = request.PageSize <= 0 ? 25 : request.PageSize;

            var itemsTask = _repository.ListAsync(request.TenantId, null, statusFilter, pageNumber, pageSize, cancellationToken);
            var countTask = _repository.CountAsync(request.TenantId, null, statusFilter, cancellationToken);

            await Task.WhenAll(itemsTask, countTask);

            var dtos = itemsTask.Result.Select(p => new SafetyProgramListItemDto
            {
                ProgramId = p.SafetyProgramId.Value,
                Name = p.Name,
                Category = p.Category.ToString(),
                Status = p.Status.ToString(),
                ParticipationPercent = p.ParticipationPercent,
                // NOTE: no user-directory lookup service exists in this codebase yet; the
                // owner's GUID is surfaced as a display-name fallback rather than silently
                // omitting the field. See docs/Phase5/ui-gaps-scoping.md section 8.
                OwnerName = p.OwnerId.Value.ToString(),
            }).ToList();

            var result = new PaginatedResult<SafetyProgramListItemDto>(dtos, countTask.Result, pageNumber, pageSize);

            return Result<PaginatedResult<SafetyProgramListItemDto>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve SafetyPrograms for Tenant {TenantId}", request.TenantId);

            return Result<PaginatedResult<SafetyProgramListItemDto>>.Failure(new ApplicationError(
                "SafetyPrograms.SafetyProgram.RetrievalFailed",
                "Unable to retrieve SafetyPrograms at this time.",
                Guid.NewGuid()));
        }
    }
}
