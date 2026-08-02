// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Culture
//  Layer:          Application
//  File:           GetCultureByIdQueryHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for GetCultureByIdQuery. Reads from the read-side repository (optimised
//                  stored proc; no domain-aggregate reconstruction). Returns a NotFound failure
//                  both when the row does not exist and when it belongs to a different tenant, so
//                  tenant membership is never revealed to an unauthorized caller. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Handlers.GetPermitsByIdQueryHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Analytics.Culture.Models;
using PlantSafety.Application.Analytics.Culture.Queries;
using PlantSafety.Application.Analytics.Culture.Repositories;

namespace PlantSafety.Application.Analytics.Culture.Handlers;

/// <summary>Handles <see cref="GetCultureByIdQuery"/>.</summary>
public sealed class GetCultureByIdQueryHandler : IRequestHandler<GetCultureByIdQuery, Result<CultureItemDto>>
{
    private readonly ICultureReadRepository _repository;
    private readonly ILogger<GetCultureByIdQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetCultureByIdQueryHandler(ICultureReadRepository repository, ILogger<GetCultureByIdQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<CultureItemDto>> Handle(GetCultureByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _repository.GetByIdAsync(request.CultureId, request.TenantId, cancellationToken);

            if (item is null || item.TenantId != request.TenantId)
            {
                _logger.LogDebug("Culture {CultureId} not found for Tenant {TenantId}", request.CultureId, request.TenantId);

                return Result<CultureItemDto>.Failure(new ApplicationError(
                    "Analytics.Culture.NotFound",
                    "The requested culture record could not be found.",
                    Guid.NewGuid(),
                    ErrorSeverity.NotFound));
            }

            return Result<CultureItemDto>.Success(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve Culture {CultureId}", request.CultureId);

            return Result<CultureItemDto>.Failure(new ApplicationError(
                "Analytics.Culture.RetrievalFailed",
                "Unable to retrieve culture data at this time.",
                Guid.NewGuid()));
        }
    }
}
