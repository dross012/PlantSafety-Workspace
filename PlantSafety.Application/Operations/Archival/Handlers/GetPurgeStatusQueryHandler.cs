// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetPurgeStatusQuery.
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

/// <summary>Handles <see cref="GetPurgeStatusQuery"/>.</summary>
public sealed class GetPurgeStatusQueryHandler : IRequestHandler<GetPurgeStatusQuery, Result<IReadOnlyList<ArchivalJobDto>>>
{
    private readonly IArchivalRepository _repository;
    private readonly ILogger<GetPurgeStatusQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetPurgeStatusQueryHandler(IArchivalRepository repository, ILogger<GetPurgeStatusQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<ArchivalJobDto>>> Handle(GetPurgeStatusQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetPurgeStatusAsync(request.Zone, request.Top, cancellationToken);
            return Result<IReadOnlyList<ArchivalJobDto>>.Success(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve purge status for zone {Zone}.", request.Zone ?? "(all)");

            return Result<IReadOnlyList<ArchivalJobDto>>.Failure(new ApplicationError(
                "Operations.Archival.StatusRetrievalFailed",
                "Unable to retrieve purge status at this time.",
                Guid.NewGuid()));
        }
    }
}
