// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for TriggerPurgeCommand.
// SchemaVersion: 1

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Operations.Archival.Commands;
using PlantSafety.Application.Operations.Archival.Models;
using PlantSafety.Application.Operations.Archival.Repositories;

namespace PlantSafety.Application.Operations.Archival.Handlers;

/// <summary>Handles <see cref="TriggerPurgeCommand"/>.</summary>
public sealed class TriggerPurgeCommandHandler : IRequestHandler<TriggerPurgeCommand, Result<IReadOnlyList<PurgeTriggerResultDto>>>
{
    private readonly IArchivalRepository _repository;
    private readonly ILogger<TriggerPurgeCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public TriggerPurgeCommandHandler(IArchivalRepository repository, ILogger<TriggerPurgeCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<PurgeTriggerResultDto>>> Handle(TriggerPurgeCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var results = await _repository.TriggerPurgeAsync(request.Target, cancellationToken);
            return Result<IReadOnlyList<PurgeTriggerResultDto>>.Success(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to trigger retention purge for target {Target}.", request.Target?.ToString() ?? "(all)");

            return Result<IReadOnlyList<PurgeTriggerResultDto>>.Failure(new ApplicationError(
                "Operations.Archival.TriggerFailed",
                "Unable to trigger the retention purge at this time.",
                Guid.NewGuid()));
        }
    }
}
