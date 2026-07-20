// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetQueryPerformanceQuery.
// SchemaVersion: 1

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Operations.Monitoring.Models;
using PlantSafety.Application.Operations.Monitoring.Queries;
using PlantSafety.Application.Operations.Monitoring.Repositories;

namespace PlantSafety.Application.Operations.Monitoring.Handlers;

/// <summary>Handles <see cref="GetQueryPerformanceQuery"/>.</summary>
public sealed class GetQueryPerformanceQueryHandler : IRequestHandler<GetQueryPerformanceQuery, Result<IReadOnlyList<QueryPerformanceItemDto>>>
{
    private readonly IMonitoringRepository _repository;
    private readonly ILogger<GetQueryPerformanceQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetQueryPerformanceQueryHandler(IMonitoringRepository repository, ILogger<GetQueryPerformanceQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<QueryPerformanceItemDto>>> Handle(GetQueryPerformanceQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetTopQueriesByCpuAsync(request.DatabaseKey, request.Top, request.LookbackHours, cancellationToken);
            return Result<IReadOnlyList<QueryPerformanceItemDto>>.Success(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve Query Store performance data for {DatabaseKey}.", request.DatabaseKey);

            return Result<IReadOnlyList<QueryPerformanceItemDto>>.Failure(new ApplicationError(
                "Operations.Monitoring.QueryPerformanceRetrievalFailed",
                "Unable to retrieve query performance data at this time.",
                Guid.NewGuid()));
        }
    }
}
