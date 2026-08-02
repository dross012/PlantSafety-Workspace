// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Engagement
//  Layer:          Application
//  File:           GetEngagementRecordsQueryHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for GetEngagementRecordsQuery. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Handlers.GetPermitsQueryHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Analytics.Engagement.Queries;
using PlantSafety.Application.Analytics.Engagement.Repositories;

namespace PlantSafety.Application.Analytics.Engagement.Handlers;

/// <summary>Handles <see cref="GetEngagementRecordsQuery"/>.</summary>
public sealed class GetEngagementRecordsQueryHandler : IRequestHandler<GetEngagementRecordsQuery, Result<EngagementRecordsQueryResult>>
{
    private readonly IEngagementRepository _repository;
    private readonly ILogger<GetEngagementRecordsQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetEngagementRecordsQueryHandler(IEngagementRepository repository, ILogger<GetEngagementRecordsQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<EngagementRecordsQueryResult>> Handle(GetEngagementRecordsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetEngagementRecordsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<EngagementRecordsQueryResult>.Success(new EngagementRecordsQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve Engagement records for Tenant {TenantId}", request.TenantId);

            return Result<EngagementRecordsQueryResult>.Failure(new ApplicationError(
                "Analytics.Engagement.RetrievalFailed",
                "Unable to retrieve Engagement data at this time.",
                Guid.NewGuid()));
        }
    }
}
