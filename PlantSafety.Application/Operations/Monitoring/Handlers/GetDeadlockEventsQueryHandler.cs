// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetDeadlockEventsQuery.
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

/// <summary>Handles <see cref="GetDeadlockEventsQuery"/>.</summary>
public sealed class GetDeadlockEventsQueryHandler : IRequestHandler<GetDeadlockEventsQuery, Result<IReadOnlyList<DeadlockEventDto>>>
{
    private readonly IMonitoringRepository _repository;
    private readonly ILogger<GetDeadlockEventsQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetDeadlockEventsQueryHandler(IMonitoringRepository repository, ILogger<GetDeadlockEventsQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<DeadlockEventDto>>> Handle(GetDeadlockEventsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetRecentDeadlocksAsync(request.Top, cancellationToken);
            return Result<IReadOnlyList<DeadlockEventDto>>.Success(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve recent deadlock events.");

            return Result<IReadOnlyList<DeadlockEventDto>>.Failure(new ApplicationError(
                "Operations.Monitoring.DeadlockRetrievalFailed",
                "Unable to retrieve recent deadlock events at this time.",
                Guid.NewGuid()));
        }
    }
}
