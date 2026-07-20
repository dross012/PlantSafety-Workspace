// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetFeatureFlagsQuery.
// SchemaVersion: 1

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Operations.Configuration.Models;
using PlantSafety.Application.Operations.Configuration.Queries;
using PlantSafety.Application.Operations.Configuration.Repositories;

namespace PlantSafety.Application.Operations.Configuration.Handlers;

/// <summary>Handles <see cref="GetFeatureFlagsQuery"/>.</summary>
public sealed class GetFeatureFlagsQueryHandler : IRequestHandler<GetFeatureFlagsQuery, Result<IReadOnlyList<FeatureFlagDto>>>
{
    private readonly IFeatureFlagRepository _repository;
    private readonly ILogger<GetFeatureFlagsQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetFeatureFlagsQueryHandler(IFeatureFlagRepository repository, ILogger<GetFeatureFlagsQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<FeatureFlagDto>>> Handle(GetFeatureFlagsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetAllAsync(request.DatabaseKey, cancellationToken);
            return Result<IReadOnlyList<FeatureFlagDto>>.Success(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve feature flags for database {DatabaseKey}.", request.DatabaseKey);

            return Result<IReadOnlyList<FeatureFlagDto>>.Failure(new ApplicationError(
                "Operations.Configuration.RetrievalFailed",
                "Unable to retrieve feature flags at this time.",
                Guid.NewGuid()));
        }
    }
}
