// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Classification
//  Layer:          Application
//  File:           GetHazmatMaterialQueryHandler.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Handler for GetHazmatMaterialQuery. Reads from the read-side repository
//                  (optimised stored proc; no domain-aggregate reconstruction). Returns a NotFound
//                  failure both when the row does not exist and when it belongs to a different
//                  tenant, so tenant membership is never revealed to an unauthorized caller.
//                  Mirrors
//                  PlantSafety.Application.Compliance.Permits.Handlers.GetPermitsByIdQueryHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.PHMSA.Classification.Models;
using PlantSafety.Application.Compliance.PHMSA.Classification.Queries;
using PlantSafety.Application.Compliance.PHMSA.Classification.Repositories;

namespace PlantSafety.Application.Compliance.PHMSA.Classification.Handlers;

/// <summary>Handles <see cref="GetHazmatMaterialQuery"/>.</summary>
public sealed class GetHazmatMaterialQueryHandler : IRequestHandler<GetHazmatMaterialQuery, Result<HazmatMaterialDto>>
{
    private readonly IHazmatMaterialRepository _repository;
    private readonly ILogger<GetHazmatMaterialQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetHazmatMaterialQueryHandler(IHazmatMaterialRepository repository, ILogger<GetHazmatMaterialQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<HazmatMaterialDto>> Handle(GetHazmatMaterialQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _repository.GetByIdAsync(request.HazmatMaterialId, request.TenantId, cancellationToken);

            if (item is null || item.TenantId != request.TenantId)
            {
                _logger.LogDebug("HazmatMaterial {HazmatMaterialId} not found for Tenant {TenantId}", request.HazmatMaterialId, request.TenantId);

                return Result<HazmatMaterialDto>.Failure(new ApplicationError(
                    "Compliance.PHMSA.Classification.NotFound",
                    "The requested hazmat classification could not be found.",
                    Guid.NewGuid(),
                    ErrorSeverity.NotFound));
            }

            return Result<HazmatMaterialDto>.Success(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve HazmatMaterial {HazmatMaterialId}", request.HazmatMaterialId);

            return Result<HazmatMaterialDto>.Failure(new ApplicationError(
                "Compliance.PHMSA.Classification.RetrievalFailed",
                "Unable to retrieve hazmat classification data at this time.",
                Guid.NewGuid()));
        }
    }
}
