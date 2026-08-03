// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         GlobalSafetyInsights
//  Layer:          Application
//  File:           GetGlobalSafetyInsightsDetailQueryHandler.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 8 §5 Global Safety Dashboard)
//  SchemaVersion:  2026.1
//  Description:    Handles GetGlobalSafetyInsightsDetailQuery by hydrating the latest
//                  GlobalSafetyInsightsAggregate via the Domain repository (which already
//                  round-trips full RiskScores/Anomalies/Recommendations detail through JSON
//                  columns) and projecting it into the dashboard's detail DTO. Additive-only
//                  evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.GlobalSafetyInsights.Models;
using PlantSafety.Application.GlobalSafetyInsights.Queries;
using PlantSafety.Domain.GlobalSafetyInsights.Interfaces;

namespace PlantSafety.Application.GlobalSafetyInsights.Handlers;

/// <summary>Handles <see cref="GetGlobalSafetyInsightsDetailQuery"/>.</summary>
public sealed class GetGlobalSafetyInsightsDetailQueryHandler
    : IRequestHandler<GetGlobalSafetyInsightsDetailQuery, Result<GlobalSafetyInsightsDetailDto?>>
{
    private readonly IGlobalSafetyInsightsRepository _repository;
    private readonly ILogger<GetGlobalSafetyInsightsDetailQueryHandler> _logger;

    /// <summary>Initializes dependencies.</summary>
    public GetGlobalSafetyInsightsDetailQueryHandler(
        IGlobalSafetyInsightsRepository repository, ILogger<GetGlobalSafetyInsightsDetailQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<GlobalSafetyInsightsDetailDto?>> Handle(GetGlobalSafetyInsightsDetailQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var latest = await _repository.GetLatestAsync(request.TenantId, cancellationToken);
            if (latest is null)
                return Result<GlobalSafetyInsightsDetailDto?>.Success(null);

            var dto = new GlobalSafetyInsightsDetailDto
            {
                GlobalSafetyInsightsId = latest.GlobalSafetyInsightsId.Value,
                TenantId = latest.TenantId,
                RegionCode = latest.Region.Code,
                SiteIds = latest.SiteIds,
                WindowStartUtc = latest.TimeWindow.StartUtc.Value,
                WindowEndUtc = latest.TimeWindow.EndUtc.Value,
                Trir = latest.Kpis.Trir,
                Ltir = latest.Kpis.Ltir,
                ExposureRate = latest.Kpis.ExposureRate,
                IncidentDensity = latest.Kpis.IncidentDensity,
                MachineStateIncidentCorrelation = latest.Kpis.MachineStateIncidentCorrelation,
                RiskScores = latest.RiskScores.Select(s => new GlobalSafetyRiskScoreDetailDto
                {
                    ScopeType = s.ScopeType.ToString(),
                    ScopeKey = s.ScopeKey,
                    Severity = s.AggregatedScore.Severity,
                    Likelihood = s.AggregatedScore.Likelihood,
                    Score = s.AggregatedScore.Score,
                    RiskLevel = s.Level.Code,
                    SourceRiskCount = s.SourceRiskCount,
                }).ToList(),
                Anomalies = latest.Anomalies.Select(a => new GlobalSafetyAnomalyDetailDto
                {
                    Kind = a.Kind.ToString(),
                    SiteId = a.SiteId,
                    MachineFamily = a.MachineFamily,
                    DeviationMagnitude = a.DeviationMagnitude,
                    Message = a.Message,
                    DetectedAtUtc = a.DetectedAtUtc.Value,
                }).ToList(),
                Recommendations = latest.Recommendations.Select(r => new GlobalSafetyRecommendationDetailDto
                {
                    Priority = r.Priority.ToString(),
                    Category = r.Category,
                    Description = r.Description,
                    TargetSiteId = r.TargetSiteId,
                    RelatedAnomalyKind = r.RelatedAnomalyKind?.ToString(),
                }).ToList(),
                ComputedAtUtc = latest.ComputedAtUtc.Value,
            };

            return Result<GlobalSafetyInsightsDetailDto?>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve GlobalSafetyInsights detail for Tenant {TenantId}", request.TenantId);

            return Result<GlobalSafetyInsightsDetailDto?>.Failure(new ApplicationError(
                "GlobalSafetyInsights.DetailRetrievalFailed",
                "Unable to retrieve global safety insight detail at this time.",
                Guid.NewGuid()));
        }
    }
}
