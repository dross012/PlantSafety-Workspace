// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-19
// FILE DESCRIPTION: Handler for GetBackupHealthQuery.
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

/// <summary>Handles <see cref="GetBackupHealthQuery"/>.</summary>
public sealed class GetBackupHealthQueryHandler : IRequestHandler<GetBackupHealthQuery, Result<IReadOnlyList<BackupHealthDto>>>
{
    private readonly IBackupRepository _repository;
    private readonly ILogger<GetBackupHealthQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetBackupHealthQueryHandler(IBackupRepository repository, ILogger<GetBackupHealthQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<BackupHealthDto>>> Handle(GetBackupHealthQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetBackupHealthAsync(cancellationToken);
            return Result<IReadOnlyList<BackupHealthDto>>.Success(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to derive backup health.");

            return Result<IReadOnlyList<BackupHealthDto>>.Failure(new ApplicationError(
                "Operations.Backup.HealthRetrievalFailed",
                "Unable to retrieve backup health at this time.",
                Guid.NewGuid()));
        }
    }
}
