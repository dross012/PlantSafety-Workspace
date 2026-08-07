// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Packaging
//  Layer:          Application
//  File:           GetPackagingSpecQueryHandler.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Handler for GetPackagingSpecQuery. Reads from the read-side repository
//                  (optimised stored proc; no domain-aggregate reconstruction). Returns a NotFound
//                  failure both when the row does not exist and when it belongs to a different
//                  tenant. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Handlers.GetPermitsByIdQueryHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.PHMSA.Packaging.Models;
using PlantSafety.Application.Compliance.PHMSA.Packaging.Queries;
using PlantSafety.Application.Compliance.PHMSA.Packaging.Repositories;

namespace PlantSafety.Application.Compliance.PHMSA.Packaging.Handlers;

/// <summary>Handles <see cref="GetPackagingSpecQuery"/>.</summary>
public sealed class GetPackagingSpecQueryHandler : IRequestHandler<GetPackagingSpecQuery, Result<PackagingSpecificationDto>>
{
    private readonly IPackagingSpecificationRepository _repository;
    private readonly ILogger<GetPackagingSpecQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetPackagingSpecQueryHandler(IPackagingSpecificationRepository repository, ILogger<GetPackagingSpecQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<PackagingSpecificationDto>> Handle(GetPackagingSpecQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _repository.GetByIdAsync(request.PackagingSpecificationId, request.TenantId, cancellationToken);

            if (item is null || item.TenantId != request.TenantId)
            {
                _logger.LogDebug("PackagingSpecification {PackagingSpecificationId} not found for Tenant {TenantId}", request.PackagingSpecificationId, request.TenantId);

                return Result<PackagingSpecificationDto>.Failure(new ApplicationError(
                    "Compliance.PHMSA.Packaging.NotFound",
                    "The requested packaging specification could not be found.",
                    Guid.NewGuid(),
                    ErrorSeverity.NotFound));
            }

            return Result<PackagingSpecificationDto>.Success(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve PackagingSpecification {PackagingSpecificationId}", request.PackagingSpecificationId);

            return Result<PackagingSpecificationDto>.Failure(new ApplicationError(
                "Compliance.PHMSA.Packaging.RetrievalFailed",
                "Unable to retrieve packaging specification data at this time.",
                Guid.NewGuid()));
        }
    }
}
