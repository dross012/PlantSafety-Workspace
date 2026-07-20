// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetDatabaseHealthReportQuery.
// SchemaVersion: 1

using System;
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

/// <summary>Handles <see cref="GetDatabaseHealthReportQuery"/>.</summary>
public sealed class GetDatabaseHealthReportQueryHandler : IRequestHandler<GetDatabaseHealthReportQuery, Result<DatabaseHealthReportDto>>
{
    private readonly IDatabaseHealthReportProvider _provider;
    private readonly ILogger<GetDatabaseHealthReportQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetDatabaseHealthReportQueryHandler(IDatabaseHealthReportProvider provider, ILogger<GetDatabaseHealthReportQueryHandler> logger)
    {
        _provider = provider;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<DatabaseHealthReportDto>> Handle(GetDatabaseHealthReportQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var report = await _provider.GetHealthReportAsync(cancellationToken);
            return Result<DatabaseHealthReportDto>.Success(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve the aggregated database health report.");

            return Result<DatabaseHealthReportDto>.Failure(new ApplicationError(
                "Operations.Monitoring.HealthReportRetrievalFailed",
                "Unable to retrieve database health at this time.",
                Guid.NewGuid()));
        }
    }
}
