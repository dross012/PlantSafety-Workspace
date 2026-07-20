// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetPurgeTargetsQuery.
// SchemaVersion: 1

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Operations.Archival.Models;
using PlantSafety.Application.Operations.Archival.Queries;
using PlantSafety.Application.Operations.Archival.Repositories;

namespace PlantSafety.Application.Operations.Archival.Handlers;

/// <summary>Handles <see cref="GetPurgeTargetsQuery"/>.</summary>
public sealed class GetPurgeTargetsQueryHandler : IRequestHandler<GetPurgeTargetsQuery, Result<IReadOnlyList<PurgeTargetDto>>>
{
    private readonly IArchivalRepository _repository;
    private readonly ILogger<GetPurgeTargetsQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetPurgeTargetsQueryHandler(IArchivalRepository repository, ILogger<GetPurgeTargetsQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public Task<Result<IReadOnlyList<PurgeTargetDto>>> Handle(GetPurgeTargetsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = _repository.GetPurgeTargets();
            return Task.FromResult(Result<IReadOnlyList<PurgeTargetDto>>.Success(items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve purge targets.");

            return Task.FromResult(Result<IReadOnlyList<PurgeTargetDto>>.Failure(new ApplicationError(
                "Operations.Archival.TargetsRetrievalFailed",
                "Unable to retrieve purge targets at this time.",
                Guid.NewGuid())));
        }
    }
}
