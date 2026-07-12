// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetSchedulesQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Analytics.Schedules.Queries;
using PlantSafety.Application.Analytics.Schedules.Repositories;

namespace PlantSafety.Application.Analytics.Schedules.Handlers;

/// <summary>Handles <see cref="GetSchedulesQuery"/>.</summary>
public sealed class GetSchedulesQueryHandler : IRequestHandler<GetSchedulesQuery, Result<SchedulesQueryResult>>
{
    private readonly ISchedulesRepository _repository;
    private readonly ILogger<GetSchedulesQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetSchedulesQueryHandler(ISchedulesRepository repository, ILogger<GetSchedulesQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<SchedulesQueryResult>> Handle(GetSchedulesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetScheduleItemsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<SchedulesQueryResult>.Success(new SchedulesQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve Schedules for Tenant {TenantId}", request.TenantId);

            return Result<SchedulesQueryResult>.Failure(new ApplicationError(
                "Analytics.Schedules.RetrievalFailed",
                "Unable to retrieve Schedules data at this time.",
                Guid.NewGuid()));
        }
    }
}
