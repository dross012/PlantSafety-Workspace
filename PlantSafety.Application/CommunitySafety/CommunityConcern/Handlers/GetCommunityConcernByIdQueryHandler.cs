// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for GetCommunityConcernByIdQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.CommunitySafety.CommunityConcern.Models;
using PlantSafety.Application.CommunitySafety.CommunityConcern.Queries;
using PlantSafety.Application.CommunitySafety.CommunityConcern.Repositories;

namespace PlantSafety.Application.CommunitySafety.CommunityConcern.Handlers;

/// <summary>Handles <see cref="GetCommunityConcernByIdQuery"/>.</summary>
public sealed class GetCommunityConcernByIdQueryHandler : IRequestHandler<GetCommunityConcernByIdQuery, Result<CommunityConcernItemDto>>
{
    private readonly ICommunityConcernRepository _repository;
    private readonly ILogger<GetCommunityConcernByIdQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetCommunityConcernByIdQueryHandler(ICommunityConcernRepository repository, ILogger<GetCommunityConcernByIdQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<CommunityConcernItemDto>> Handle(GetCommunityConcernByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _repository.GetCommunityConcernItemByIdAsync(request.TenantId, request.CommunityConcernId, cancellationToken);
            if (item is null)
            {
                return Result<CommunityConcernItemDto>.Failure(new ApplicationError(
                    "CommunitySafety.CommunityConcern.NotFound",
                    "The requested CommunityConcern record could not be found.",
                    Guid.NewGuid()));
            }

            return Result<CommunityConcernItemDto>.Success(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve CommunityConcern {CommunityConcernId}", request.CommunityConcernId);

            return Result<CommunityConcernItemDto>.Failure(new ApplicationError(
                "CommunitySafety.CommunityConcern.RetrievalFailed",
                "Unable to retrieve the CommunityConcern record at this time.",
                Guid.NewGuid()));
        }
    }
}
