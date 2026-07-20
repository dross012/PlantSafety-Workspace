// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetErrorLogQuery.
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

/// <summary>Handles <see cref="GetErrorLogQuery"/>.</summary>
public sealed class GetErrorLogQueryHandler : IRequestHandler<GetErrorLogQuery, Result<IReadOnlyList<ErrorLogItemDto>>>
{
    private readonly IMonitoringRepository _repository;
    private readonly ILogger<GetErrorLogQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetErrorLogQueryHandler(IMonitoringRepository repository, ILogger<GetErrorLogQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<ErrorLogItemDto>>> Handle(GetErrorLogQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetRecentErrorsAsync(
                request.MinSeverity,
                request.SinceUtc,
                request.Top,
                request.Skip,
                cancellationToken,
                request.MaxSeverity,
                request.SourceDatabase,
                request.Module,
                request.ErrorProcedure);
            return Result<IReadOnlyList<ErrorLogItemDto>>.Success(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve recent errors from ErrorLog.");

            return Result<IReadOnlyList<ErrorLogItemDto>>.Failure(new ApplicationError(
                "Operations.Monitoring.ErrorLogRetrievalFailed",
                "Unable to retrieve recent database errors at this time.",
                Guid.NewGuid()));
        }
    }
}
