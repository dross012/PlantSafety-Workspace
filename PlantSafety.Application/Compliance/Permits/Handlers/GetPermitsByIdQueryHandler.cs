// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Permits
//  Layer:          Application
//  File:           GetPermitsByIdQueryHandler.cs
//  Created:        2026-07-30
//  SchemaVersion:  2026.1
//  Description:    Handler for GetPermitsByIdQuery. Reads from the read-side repository
//                  (optimised stored proc; no domain-aggregate reconstruction). Returns a
//                  NotFound failure both when the row does not exist and when it belongs to a
//                  different tenant, so tenant membership is never revealed to an unauthorized
//                  caller. tenantId is pushed into the repository call itself
//                  (usp_Permits_GetById's WHERE clause), not just checked here after the fact --
//                  the post-fetch TenantId comparison below is defense-in-depth, not the primary
//                  control. Mirrors
//                  PlantSafety.Application.RiskManagement.Handlers.GetRiskByIdQueryHandler /
//                  PlantSafety.Application.AccidentManagement.Handlers.GetAccidentByIdQueryHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.Permits.Models;
using PlantSafety.Application.Compliance.Permits.Queries;
using PlantSafety.Application.Compliance.Permits.Repositories;

namespace PlantSafety.Application.Compliance.Permits.Handlers;

/// <summary>Handles <see cref="GetPermitsByIdQuery"/>.</summary>
public sealed class GetPermitsByIdQueryHandler : IRequestHandler<GetPermitsByIdQuery, Result<PermitsItemDto>>
{
    private readonly IPermitsRepository _repository;
    private readonly ILogger<GetPermitsByIdQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetPermitsByIdQueryHandler(IPermitsRepository repository, ILogger<GetPermitsByIdQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<PermitsItemDto>> Handle(GetPermitsByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _repository.GetByIdAsync(request.PermitsId, request.TenantId, cancellationToken);

            if (item is null || item.TenantId != request.TenantId)
            {
                _logger.LogDebug("Permits {PermitsId} not found for Tenant {TenantId}", request.PermitsId, request.TenantId);

                return Result<PermitsItemDto>.Failure(new ApplicationError(
                    "Compliance.Permits.NotFound",
                    "The requested permit could not be found.",
                    Guid.NewGuid(),
                    ErrorSeverity.NotFound));
            }

            return Result<PermitsItemDto>.Success(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve Permits {PermitsId}", request.PermitsId);

            return Result<PermitsItemDto>.Failure(new ApplicationError(
                "Compliance.Permits.RetrievalFailed",
                "Unable to retrieve permit data at this time.",
                Guid.NewGuid()));
        }
    }
}
