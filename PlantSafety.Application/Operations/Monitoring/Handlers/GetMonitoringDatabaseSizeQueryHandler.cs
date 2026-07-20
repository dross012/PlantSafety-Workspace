// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetMonitoringDatabaseSizeQuery. Delegates directly to
//   IBackupRepository.GetDatabaseSizeHistoryAsync (Phase 18) instead of duplicating its
//   msdb.dbo.PlantSafety_DatabaseSizeLog query.
// SchemaVersion: 1

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Operations.Backup.Models;
using PlantSafety.Application.Operations.Backup.Repositories;
using PlantSafety.Application.Operations.Monitoring.Queries;

namespace PlantSafety.Application.Operations.Monitoring.Handlers;

/// <summary>Handles <see cref="GetMonitoringDatabaseSizeQuery"/>.</summary>
public sealed class GetMonitoringDatabaseSizeQueryHandler : IRequestHandler<GetMonitoringDatabaseSizeQuery, Result<IReadOnlyList<DatabaseSizeSnapshotDto>>>
{
    private readonly IBackupRepository _backupRepository;
    private readonly ILogger<GetMonitoringDatabaseSizeQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetMonitoringDatabaseSizeQueryHandler(IBackupRepository backupRepository, ILogger<GetMonitoringDatabaseSizeQueryHandler> logger)
    {
        _backupRepository = backupRepository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<DatabaseSizeSnapshotDto>>> Handle(GetMonitoringDatabaseSizeQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _backupRepository.GetDatabaseSizeHistoryAsync(request.DatabaseName, request.Top, cancellationToken);
            return Result<IReadOnlyList<DatabaseSizeSnapshotDto>>.Success(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve database size history for {DatabaseName}.", request.DatabaseName ?? "(all)");

            return Result<IReadOnlyList<DatabaseSizeSnapshotDto>>.Failure(new ApplicationError(
                "Operations.Monitoring.DatabaseSizeRetrievalFailed",
                "Unable to retrieve database size history at this time.",
                Guid.NewGuid()));
        }
    }
}
