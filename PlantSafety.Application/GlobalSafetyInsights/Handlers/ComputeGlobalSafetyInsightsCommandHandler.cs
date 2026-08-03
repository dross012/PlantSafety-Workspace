// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         GlobalSafetyInsights
//  Layer:          Application
//  File:           ComputeGlobalSafetyInsightsCommandHandler.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 8 §2/§4 Multi-Site Safety Intelligence Network)
//  SchemaVersion:  2026.1
//  Description:    Handles ComputeGlobalSafetyInsightsCommand: resolves the tenant's Region (real
//                  ITenantRegionRouter, Phase 7 §4.3) and enforces data residency (real
//                  IDataResidencyEnforcer, Phase 7 §5.3) before running the real
//                  IGlobalSafetyKpiEngine / IGlobalSafetyAnomalyDetector / IGlobalSafetyRiskScorer
//                  pipeline (composing RiskManagement's real, tenant-scoped Risk records) and
//                  persisting a new GlobalSafetyInsightsAggregate snapshot. Publishes the
//                  dashboard-facing mesh events via IGlobalSafetyInsightsPublisher. Additive-only
//                  evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Enterprise.MultiRegion;
using PlantSafety.Application.Enterprise.MultiRegion.DataResidency;
using PlantSafety.Application.GlobalSafetyInsights.Commands;
using PlantSafety.Application.GlobalSafetyInsights.Coordination;
using PlantSafety.Application.GlobalSafetyInsights.Publishers;
using PlantSafety.Domain.GlobalSafetyInsights.Aggregates;
using PlantSafety.Domain.GlobalSafetyInsights.Interfaces;
using PlantSafety.Domain.GlobalSafetyInsights.Services;
using PlantSafety.Domain.GlobalSafetyInsights.ValueObjects;
using PlantSafety.Domain.RiskManagement.Aggregates;
using PlantSafety.Domain.RiskManagement.Interfaces;
using PlantSafety.Domain.RiskManagement.Specifications;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;

namespace PlantSafety.Application.GlobalSafetyInsights.Handlers;

/// <summary>Handles <see cref="ComputeGlobalSafetyInsightsCommand"/>.</summary>
public sealed class ComputeGlobalSafetyInsightsCommandHandler : IRequestHandler<ComputeGlobalSafetyInsightsCommand, Result<Guid>>
{
    /// <summary>Well-known system-actor UserId used for CRUD authorship on system/event-driven (non-human) computations.</summary>
    public static readonly Guid SystemUserId = Guid.Parse("00000000-0000-0000-0000-0000005a4553");

    private readonly IGlobalSafetyTelemetryNormalizer _normalizer;
    private readonly IGlobalSafetyKpiEngine _kpiEngine;
    private readonly IGlobalSafetyAnomalyDetector _anomalyDetector;
    private readonly IGlobalSafetyRiskScorer _riskScorer;
    private readonly GlobalSafetyRecommendationComposer _recommendationComposer;
    private readonly IGlobalSafetyInsightsRepository _repository;
    private readonly IRiskRepository _riskRepository;
    private readonly ITenantRegionRouter _regionRouter;
    private readonly IDataResidencyEnforcer _residencyEnforcer;
    private readonly IGlobalSafetyInsightsPublisher _publisher;
    private readonly IGlobalSafetyAnomalyEscalationReactor _escalationReactor;
    private readonly ILogger<ComputeGlobalSafetyInsightsCommandHandler> _logger;

    /// <summary>Initializes dependencies.</summary>
    public ComputeGlobalSafetyInsightsCommandHandler(
        IGlobalSafetyTelemetryNormalizer normalizer,
        IGlobalSafetyKpiEngine kpiEngine,
        IGlobalSafetyAnomalyDetector anomalyDetector,
        IGlobalSafetyRiskScorer riskScorer,
        GlobalSafetyRecommendationComposer recommendationComposer,
        IGlobalSafetyInsightsRepository repository,
        IRiskRepository riskRepository,
        ITenantRegionRouter regionRouter,
        IDataResidencyEnforcer residencyEnforcer,
        IGlobalSafetyInsightsPublisher publisher,
        IGlobalSafetyAnomalyEscalationReactor escalationReactor,
        ILogger<ComputeGlobalSafetyInsightsCommandHandler> logger)
    {
        _normalizer = normalizer;
        _kpiEngine = kpiEngine;
        _anomalyDetector = anomalyDetector;
        _riskScorer = riskScorer;
        _recommendationComposer = recommendationComposer;
        _repository = repository;
        _riskRepository = riskRepository;
        _regionRouter = regionRouter;
        _residencyEnforcer = residencyEnforcer;
        _publisher = publisher;
        _escalationReactor = escalationReactor;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<Guid>> Handle(ComputeGlobalSafetyInsightsCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var correlationId = CorrelationId.FromGuid(request.CorrelationId == Guid.Empty ? Guid.NewGuid() : request.CorrelationId);
            var causationId = CausationId.FromGuid(request.CausationId == Guid.Empty ? correlationId.Value : request.CausationId);

            // Phase 7 §5.3 Data Residency: reject/flag a residency mismatch loudly rather than
            // silently computing/storing intelligence data outside the tenant's configured region.
            _residencyEnforcer.EnsureSatisfied(request.TenantId, correlationId, causationId);
            var region = _regionRouter.ResolveRegion(request.TenantId);

            var nowUtc = UtcTimestamp.Now();
            var timeWindow = new SafetyAnalysisTimeWindow(UtcTimestamp.From(request.WindowStartUtc), UtcTimestamp.From(request.WindowEndUtc));

            var kpis = _kpiEngine.Compute(request.Telemetry);
            var anomalies = _anomalyDetector.DetectAnomalies(request.Telemetry, nowUtc);

            var risks = await _riskRepository.ListAsync(request.TenantId, new AllScoredRisksSpecification(), cancellationToken);
            var riskScores = _riskScorer.ScoreBySite(risks, region);

            var recommendations = _recommendationComposer.Compose(anomalies, riskScores);

            var actor = request.ActorUserId.HasValue ? PerformedByActor.User(UserId.FromGuid(request.ActorUserId.Value)) : PerformedByActor.System();
            var createdBy = request.ActorUserId.HasValue ? UserId.FromGuid(request.ActorUserId.Value) : UserId.FromGuid(SystemUserId);

            var aggregate = GlobalSafetyInsightsAggregate.Compute(
                request.TenantId, region, request.SiteIds, timeWindow, kpis, riskScores, anomalies, recommendations,
                createdBy, nowUtc, correlationId, causationId, actor);

            await _repository.AddAsync(aggregate, cancellationToken);
            await _publisher.PublishAsync(aggregate, correlationId.Value, causationId.Value, cancellationToken);

            // Phase 8 §3 Multi-Site Coordination Workflows: react to every detected anomaly,
            // auto-escalating the ones that cross IMultiSiteSafetyPolicy's severity threshold.
            await _escalationReactor.ReactAsync(aggregate, correlationId.Value, causationId.Value, cancellationToken);

            _logger.LogInformation(
                "ComputeGlobalSafetyInsights: snapshot {SnapshotId} computed for Tenant {TenantId} ({SiteCount} site(s), {AnomalyCount} anomaly(ies)).",
                aggregate.GlobalSafetyInsightsId, request.TenantId, aggregate.SiteIds.Count, aggregate.Anomalies.Count);

            return Result<Guid>.Success(aggregate.GlobalSafetyInsightsId.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to compute GlobalSafetyInsights for Tenant {TenantId}", request.TenantId);

            return Result<Guid>.Failure(new ApplicationError(
                "GlobalSafetyInsights.ComputationFailed",
                "Unable to compute global safety insights at this time.",
                Guid.NewGuid()));
        }
    }

    /// <summary>Trivial specification selecting every scored Risk record for a tenant (risk scoring is a cross-cutting rollup, not filtered by any single Risk attribute).</summary>
    private sealed class AllScoredRisksSpecification : RiskSpecification
    {
        public override System.Linq.Expressions.Expression<Func<Risk, bool>> ToExpression() => r => r.CurrentScore != null;
    }
}
