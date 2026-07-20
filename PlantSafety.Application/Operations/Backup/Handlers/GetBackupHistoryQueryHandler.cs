// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-19
// FILE DESCRIPTION: Handler for GetBackupHistoryQuery.
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

/// <summary>Handles <see cref="GetBackupHistoryQuery"/>.</summary>
public sealed class GetBackupHistoryQueryHandler : IRequestHandler<GetBackupHistoryQuery, Result<IReadOnlyList<BackupHistoryItemDto>>>
{
    private readonly IBackupRepository _repository;
    private readonly ILogger<GetBackupHistoryQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetBackupHistoryQueryHandler(IBackupRepository repository, ILogger<GetBackupHistoryQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<BackupHistoryItemDto>>> Handle(GetBackupHistoryQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetBackupHistoryAsync(request.DatabaseName, request.Top, cancellationToken);
            return Result<IReadOnlyList<BackupHistoryItemDto>>.Success(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve backup history for {DatabaseName}.", request.DatabaseName ?? "(all)");

            return Result<IReadOnlyList<BackupHistoryItemDto>>.Failure(new ApplicationError(
                "Operations.Backup.HistoryRetrievalFailed",
                "Unable to retrieve backup history at this time.",
                Guid.NewGuid()));
        }
    }
}
