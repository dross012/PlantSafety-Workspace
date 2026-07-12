// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetDrillsAndExercisesQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.SafetyEvents.DrillsAndExercises.Queries;
using PlantSafety.Application.SafetyEvents.DrillsAndExercises.Repositories;

namespace PlantSafety.Application.SafetyEvents.DrillsAndExercises.Handlers;

/// <summary>Handles <see cref="GetDrillsAndExercisesQuery"/>.</summary>
public sealed class GetDrillsAndExercisesQueryHandler : IRequestHandler<GetDrillsAndExercisesQuery, Result<DrillsAndExercisesQueryResult>>
{
    private readonly IDrillsAndExercisesRepository _repository;
    private readonly ILogger<GetDrillsAndExercisesQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetDrillsAndExercisesQueryHandler(IDrillsAndExercisesRepository repository, ILogger<GetDrillsAndExercisesQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<DrillsAndExercisesQueryResult>> Handle(GetDrillsAndExercisesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetDrillsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<DrillsAndExercisesQueryResult>.Success(new DrillsAndExercisesQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve DrillsAndExercises for Tenant {TenantId}", request.TenantId);

            return Result<DrillsAndExercisesQueryResult>.Failure(new ApplicationError(
                "SafetyEvents.DrillsAndExercises.RetrievalFailed",
                "Unable to retrieve drills and exercises data at this time.",
                Guid.NewGuid()));
        }
    }
}
