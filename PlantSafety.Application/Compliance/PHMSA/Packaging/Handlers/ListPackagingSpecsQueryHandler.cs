// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Packaging
//  Layer:          Application
//  File:           ListPackagingSpecsQueryHandler.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Handler for ListPackagingSpecsQuery. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Handlers.GetPermitsQueryHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.PHMSA.Packaging.Queries;
using PlantSafety.Application.Compliance.PHMSA.Packaging.Repositories;

namespace PlantSafety.Application.Compliance.PHMSA.Packaging.Handlers;

/// <summary>Handles <see cref="ListPackagingSpecsQuery"/>.</summary>
public sealed class ListPackagingSpecsQueryHandler : IRequestHandler<ListPackagingSpecsQuery, Result<ListPackagingSpecsResult>>
{
    private readonly IPackagingSpecificationRepository _repository;
    private readonly ILogger<ListPackagingSpecsQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ListPackagingSpecsQueryHandler(IPackagingSpecificationRepository repository, ILogger<ListPackagingSpecsQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ListPackagingSpecsResult>> Handle(ListPackagingSpecsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.ListAsync(request.TenantId, request.SiteId, request.DOTSpec, cancellationToken);

            return Result<ListPackagingSpecsResult>.Success(new ListPackagingSpecsResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve PackagingSpecification list for Tenant {TenantId}", request.TenantId);

            return Result<ListPackagingSpecsResult>.Failure(new ApplicationError(
                "Compliance.PHMSA.Packaging.RetrievalFailed",
                "Unable to retrieve packaging specification data at this time.",
                Guid.NewGuid()));
        }
    }
}
