// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         GlobalSafetyInsights
//  Layer:          Application
//  File:           GetGlobalSafetyInsightsQueryHandler.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 8 §2 Multi-Site Safety Intelligence Network)
//  SchemaVersion:  2026.1
//  Description:    Handles GetGlobalSafetyInsightsQuery. Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.GlobalSafetyInsights.Queries;
using PlantSafety.Application.GlobalSafetyInsights.Repositories;

namespace PlantSafety.Application.GlobalSafetyInsights.Handlers;

/// <summary>Handles <see cref="GetGlobalSafetyInsightsQuery"/>.</summary>
public sealed class GetGlobalSafetyInsightsQueryHandler : IRequestHandler<GetGlobalSafetyInsightsQuery, Result<GlobalSafetyInsightsQueryResult>>
{
    private readonly IGlobalSafetyInsightsReadRepository _repository;
    private readonly ILogger<GetGlobalSafetyInsightsQueryHandler> _logger;

    /// <summary>Initializes dependencies.</summary>
    public GetGlobalSafetyInsightsQueryHandler(IGlobalSafetyInsightsReadRepository repository, ILogger<GetGlobalSafetyInsightsQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<GlobalSafetyInsightsQueryResult>> Handle(GetGlobalSafetyInsightsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var latest = await _repository.GetLatestAsync(request.TenantId, cancellationToken);
            var history = await _repository.GetHistoryAsync(request.TenantId, request.From, request.To, cancellationToken);

            return Result<GlobalSafetyInsightsQueryResult>.Success(new GlobalSafetyInsightsQueryResult(
                request.TenantId, DateTime.UtcNow, latest, history));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve GlobalSafetyInsights for Tenant {TenantId}", request.TenantId);

            return Result<GlobalSafetyInsightsQueryResult>.Failure(new ApplicationError(
                "GlobalSafetyInsights.RetrievalFailed",
                "Unable to retrieve global safety insights at this time.",
                Guid.NewGuid()));
        }
    }
}
