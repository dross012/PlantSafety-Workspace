// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Classification
//  Layer:          Application
//  File:           ListClassifiedMaterialsQueryHandler.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Handler for ListClassifiedMaterialsQuery. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Handlers.GetPermitsQueryHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.PHMSA.Classification.Queries;
using PlantSafety.Application.Compliance.PHMSA.Classification.Repositories;

namespace PlantSafety.Application.Compliance.PHMSA.Classification.Handlers;

/// <summary>Handles <see cref="ListClassifiedMaterialsQuery"/>.</summary>
public sealed class ListClassifiedMaterialsQueryHandler : IRequestHandler<ListClassifiedMaterialsQuery, Result<ListClassifiedMaterialsResult>>
{
    private readonly IHazmatMaterialRepository _repository;
    private readonly ILogger<ListClassifiedMaterialsQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ListClassifiedMaterialsQueryHandler(IHazmatMaterialRepository repository, ILogger<ListClassifiedMaterialsQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ListClassifiedMaterialsResult>> Handle(ListClassifiedMaterialsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.ListAsync(request.TenantId, request.SiteId, request.UNNumber, cancellationToken);

            return Result<ListClassifiedMaterialsResult>.Success(new ListClassifiedMaterialsResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve HazmatMaterial list for Tenant {TenantId}", request.TenantId);

            return Result<ListClassifiedMaterialsResult>.Failure(new ApplicationError(
                "Compliance.PHMSA.Classification.RetrievalFailed",
                "Unable to retrieve hazmat classification data at this time.",
                Guid.NewGuid()));
        }
    }
}
