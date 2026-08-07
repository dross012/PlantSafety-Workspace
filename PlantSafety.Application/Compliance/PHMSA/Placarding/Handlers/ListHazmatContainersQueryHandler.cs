// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Placarding
//  Layer:          Application
//  File:           ListHazmatContainersQueryHandler.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Handler for ListHazmatContainersQuery. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Handlers.GetPermitsQueryHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.PHMSA.Placarding.Queries;
using PlantSafety.Application.Compliance.PHMSA.Placarding.Repositories;

namespace PlantSafety.Application.Compliance.PHMSA.Placarding.Handlers;

/// <summary>Handles <see cref="ListHazmatContainersQuery"/>.</summary>
public sealed class ListHazmatContainersQueryHandler : IRequestHandler<ListHazmatContainersQuery, Result<ListHazmatContainersResult>>
{
    private readonly IHazmatContainerRepository _repository;
    private readonly ILogger<ListHazmatContainersQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ListHazmatContainersQueryHandler(IHazmatContainerRepository repository, ILogger<ListHazmatContainersQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ListHazmatContainersResult>> Handle(ListHazmatContainersQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.ListAsync(request.TenantId, request.SiteId, request.MaterialId, cancellationToken);

            return Result<ListHazmatContainersResult>.Success(new ListHazmatContainersResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list HazmatContainers for Tenant {TenantId}", request.TenantId);

            return Result<ListHazmatContainersResult>.Failure(new ApplicationError(
                "Compliance.PHMSA.Placarding.RetrievalFailed",
                "Unable to retrieve hazmat container data at this time.",
                Guid.NewGuid()));
        }
    }
}
