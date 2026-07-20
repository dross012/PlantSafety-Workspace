// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-19
// FILE DESCRIPTION: Handler for RunRestoreVerifyCommand.
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

/// <summary>Handles <see cref="RunRestoreVerifyCommand"/>.</summary>
public sealed class RunRestoreVerifyCommandHandler : IRequestHandler<RunRestoreVerifyCommand, Result<RestoreVerifyResultDto>>
{
    private readonly IBackupRepository _repository;
    private readonly ILogger<RunRestoreVerifyCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public RunRestoreVerifyCommandHandler(IBackupRepository repository, ILogger<RunRestoreVerifyCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<RestoreVerifyResultDto>> Handle(RunRestoreVerifyCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.DatabaseName))
        {
            return Result<RestoreVerifyResultDto>.Failure(new ApplicationError(
                "Operations.Backup.RestoreVerify.InvalidDatabaseName",
                "A database name is required to run a restore-verify.",
                Guid.NewGuid(),
                ErrorSeverity.Validation));
        }

        try
        {
            var result = await _repository.RunRestoreVerifyAsync(request.DatabaseName, cancellationToken);
            return Result<RestoreVerifyResultDto>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to run restore-verify for {DatabaseName}.", request.DatabaseName);

            return Result<RestoreVerifyResultDto>.Failure(new ApplicationError(
                "Operations.Backup.RestoreVerifyFailed",
                "Unable to run the restore-verify at this time.",
                Guid.NewGuid()));
        }
    }
}
