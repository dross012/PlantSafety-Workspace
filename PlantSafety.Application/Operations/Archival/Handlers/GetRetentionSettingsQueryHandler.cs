// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetRetentionSettingsQuery.
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

/// <summary>Handles <see cref="GetRetentionSettingsQuery"/>.</summary>
public sealed class GetRetentionSettingsQueryHandler : IRequestHandler<GetRetentionSettingsQuery, Result<IReadOnlyList<TierPolicyDto>>>
{
    private readonly IArchivalRepository _repository;
    private readonly ILogger<GetRetentionSettingsQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetRetentionSettingsQueryHandler(IArchivalRepository repository, ILogger<GetRetentionSettingsQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<TierPolicyDto>>> Handle(GetRetentionSettingsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetRetentionSettingsAsync(cancellationToken);
            return Result<IReadOnlyList<TierPolicyDto>>.Success(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve retention settings.");

            return Result<IReadOnlyList<TierPolicyDto>>.Failure(new ApplicationError(
                "Operations.Archival.RetentionSettingsRetrievalFailed",
                "Unable to retrieve retention settings at this time.",
                Guid.NewGuid()));
        }
    }
}
