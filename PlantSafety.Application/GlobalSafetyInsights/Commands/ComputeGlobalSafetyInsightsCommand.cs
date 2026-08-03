// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         GlobalSafetyInsights
//  Layer:          Application
//  File:           ComputeGlobalSafetyInsightsCommand.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 8 §2/§4 Multi-Site Safety Intelligence Network)
//  SchemaVersion:  2026.1
//  Description:    Command that runs the Normalizer -> KpiEngine/AnomalyDetector/RiskScorer
//                  pipeline and persists a new GlobalSafetyInsightsAggregate snapshot. Dispatched by
//                  GlobalSafetyTelemetryConsumer (Phase 8 §4 event-mesh extension) via MediatR ISender,
//                  mirroring the real Phase 5 §3 Cross-Module Orchestration consumer pattern
//                  (CrossModuleIdempotentConsumerBase.DispatchAsync). Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Domain.GlobalSafetyInsights.ValueObjects;

namespace PlantSafety.Application.GlobalSafetyInsights.Commands;

/// <summary>
/// Runs the full GlobalSafetyInsights compute pipeline for the given tenant/sites/window and
/// persists a new snapshot.
/// </summary>
/// <param name="TenantId">Tenant identifier.</param>
/// <param name="SiteIds">Sites covered by this computation.</param>
/// <param name="WindowStartUtc">Inclusive UTC start of the analysis window.</param>
/// <param name="WindowEndUtc">Inclusive UTC end of the analysis window.</param>
/// <param name="Telemetry">Already-normalized telemetry records (produced by IGlobalSafetyTelemetryNormalizer at the consumer boundary).</param>
/// <param name="ActorUserId">User that triggered this computation, if a human-initiated recompute; <see langword="null"/> for system-triggered (event-driven) computation.</param>
/// <param name="CorrelationId">Correlation identifier propagated from the triggering event/request.</param>
/// <param name="CausationId">Causation identifier propagated from the triggering event/request.</param>
public sealed record ComputeGlobalSafetyInsightsCommand(
    Guid TenantId,
    IReadOnlyList<Guid> SiteIds,
    DateTime WindowStartUtc,
    DateTime WindowEndUtc,
    IReadOnlyList<SafetyTelemetryEvent> Telemetry,
    Guid? ActorUserId,
    Guid CorrelationId,
    Guid CausationId) : IRequest<Result<Guid>>;
