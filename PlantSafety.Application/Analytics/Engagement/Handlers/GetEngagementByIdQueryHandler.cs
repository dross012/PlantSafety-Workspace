// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Engagement
//  Layer:          Application
//  File:           GetEngagementByIdQueryHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for GetEngagementByIdQuery. Reads from the read-side repository
//                  (optimised stored proc; no domain-aggregate reconstruction). Returns a
//                  NotFound failure both when the row does not exist and when it belongs to a
//                  different tenant, so tenant membership is never revealed to an unauthorized
//                  caller. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Handlers.GetPermitsByIdQueryHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Analytics.Engagement.Models;
using PlantSafety.Application.Analytics.Engagement.Queries;
using PlantSafety.Application.Analytics.Engagement.Repositories;

namespace PlantSafety.Application.Analytics.Engagement.Handlers;

/// <summary>Handles <see cref="GetEngagementByIdQuery"/>.</summary>
public sealed class GetEngagementByIdQueryHandler : IRequestHandler<GetEngagementByIdQuery, Result<EngagementItemDto>>
{
    private readonly IEngagementRepository _repository;
    private readonly ILogger<GetEngagementByIdQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetEngagementByIdQueryHandler(IEngagementRepository repository, ILogger<GetEngagementByIdQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<EngagementItemDto>> Handle(GetEngagementByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _repository.GetByIdAsync(request.EngagementId, request.TenantId, cancellationToken);

            if (item is null || item.TenantId != request.TenantId)
            {
                _logger.LogDebug("Engagement {EngagementId} not found for Tenant {TenantId}", request.EngagementId, request.TenantId);

                return Result<EngagementItemDto>.Failure(new ApplicationError(
                    "Analytics.Engagement.NotFound",
                    "The requested engagement record could not be found.",
                    Guid.NewGuid(),
                    ErrorSeverity.NotFound));
            }

            return Result<EngagementItemDto>.Success(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve Engagement {EngagementId}", request.EngagementId);

            return Result<EngagementItemDto>.Failure(new ApplicationError(
                "Analytics.Engagement.RetrievalFailed",
                "Unable to retrieve engagement data at this time.",
                Guid.NewGuid()));
        }
    }
}
