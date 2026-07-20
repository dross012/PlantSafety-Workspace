// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-19
// FILE DESCRIPTION: Handler for GetDatabaseSizeHistoryQuery.
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
using PlantSafety.Application.Operations.Backup.Queries;
using PlantSafety.Application.Operations.Backup.Repositories;

namespace PlantSafety.Application.Operations.Backup.Handlers;

/// <summary>Handles <see cref="GetDatabaseSizeHistoryQuery"/>.</summary>
public sealed class GetDatabaseSizeHistoryQueryHandler : IRequestHandler<GetDatabaseSizeHistoryQuery, Result<IReadOnlyList<DatabaseSizeSnapshotDto>>>
{
    private readonly IBackupRepository _repository;
    private readonly ILogger<GetDatabaseSizeHistoryQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetDatabaseSizeHistoryQueryHandler(IBackupRepository repository, ILogger<GetDatabaseSizeHistoryQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<DatabaseSizeSnapshotDto>>> Handle(GetDatabaseSizeHistoryQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetDatabaseSizeHistoryAsync(request.DatabaseName, request.Top, cancellationToken);
            return Result<IReadOnlyList<DatabaseSizeSnapshotDto>>.Success(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve database size history for {DatabaseName}.", request.DatabaseName ?? "(all)");

            return Result<IReadOnlyList<DatabaseSizeSnapshotDto>>.Failure(new ApplicationError(
                "Operations.Backup.DatabaseSizeRetrievalFailed",
                "Unable to retrieve database size history at this time.",
                Guid.NewGuid()));
        }
    }
}
