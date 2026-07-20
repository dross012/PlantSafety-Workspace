// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-19
// FILE DESCRIPTION: Handler for TriggerBackupCommand.
// SchemaVersion: 1

using System;
using System.Threading;
using System.Threading.Tasks;

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Operations.Backup.Commands;
using PlantSafety.Application.Operations.Backup.Models;
using PlantSafety.Application.Operations.Backup.Repositories;

namespace PlantSafety.Application.Operations.Backup.Handlers;

/// <summary>Handles <see cref="TriggerBackupCommand"/>.</summary>
public sealed class TriggerBackupCommandHandler : IRequestHandler<TriggerBackupCommand, Result<BackupTriggerResultDto>>
{
    private readonly IBackupRepository _repository;
    private readonly ILogger<TriggerBackupCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public TriggerBackupCommandHandler(IBackupRepository repository, ILogger<TriggerBackupCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<BackupTriggerResultDto>> Handle(TriggerBackupCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _repository.TriggerBackupAsync(request.BackupType, cancellationToken);
            return Result<BackupTriggerResultDto>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to trigger {BackupType} backup.", request.BackupType);

            return Result<BackupTriggerResultDto>.Failure(new ApplicationError(
                "Operations.Backup.TriggerFailed",
                "Unable to trigger the backup at this time.",
                Guid.NewGuid()));
        }
    }
}
