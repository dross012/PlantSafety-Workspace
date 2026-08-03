// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Application.GlobalSafetyInsights.Coordination
//  Layer:          Tests (Unit)
//  File:           GlobalSafetyAnomalyEscalationReactorTests.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 8 §3 Multi-Site Coordination Workflows)
//  SchemaVersion:  2026.1
//  Description:    Unit tests proving the real, working end-to-end chain: a computed
//                  GlobalSafetyInsightsAggregate carrying a high-deviation anomaly (already raising
//                  GlobalSafetyAnomalyDetectedEvent per Phase 8 §2) causes
//                  GlobalSafetyAnomalyEscalationReactor to route a CrossSiteIncidentEscalationCommand
//                  through IMultiSiteActionRouter, while a low-deviation anomaly does not. The
//                  router itself is mocked here (its own validate->policy->route->audit sequence is
//                  covered by MultiSiteActionRouterTests) -- this test isolates and proves the
//                  event-to-router reaction/threshold logic. Isolated, deterministic, <50ms per
//                  test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.GlobalSafetyInsights.Coordination;
using PlantSafety.Application.GlobalSafetyInsights.Coordination.Models;
using PlantSafety.Domain.GlobalSafetyInsights.Aggregates;
using PlantSafety.Domain.GlobalSafetyInsights.Coordination.Enums;
using PlantSafety.Domain.GlobalSafetyInsights.Coordination.Policies;
using PlantSafety.Domain.GlobalSafetyInsights.Enums;
using PlantSafety.Domain.GlobalSafetyInsights.ValueObjects;
using PlantSafety.SharedKernel.Contracts.Enterprise.MultiRegion;

namespace PlantSafety.Tests.Application.GlobalSafetyInsights.Coordination;

/// <summary>Unit tests for <see cref="GlobalSafetyAnomalyEscalationReactor"/>.</summary>
public sealed class GlobalSafetyAnomalyEscalationReactorTests : UnitTestBase
{
    private static readonly GlobalSafetyKpiSnapshot ZeroKpis = new()
    {
        Trir = 0m,
        Ltir = 0m,
        ExposureRate = 0m,
        IncidentDensity = 0m,
        MachineStateIncidentCorrelation = 0.0,
        TotalRecordableIncidents = 0,
        TotalLostTimeIncidents = 0,
        TotalHoursWorked = 0m,
    };

    private static readonly SafetyAnalysisTimeWindow Window = new(
        UtcTimestamp.From(FixedUtcNow.AddDays(-1)), FixedTimestamp);

    private readonly MultiSiteSafetyPolicy _realPolicy = new();
    private readonly IMultiSiteActionRouter _router = Substitute.For<IMultiSiteActionRouter>();

    private GlobalSafetyAnomalyEscalationReactor Sut() => new(_realPolicy, _router, NullLogger<GlobalSafetyAnomalyEscalationReactor>());

    private GlobalSafetyInsightsAggregate ComputeWithAnomaly(Guid siteId, double deviationMagnitude)
        => GlobalSafetyInsightsAggregate.Compute(
            TestTenantId.Value, Region.UsEast, [siteId], Window, ZeroKpis, [],
            [
                new DetectedAnomaly
                {
                    Kind = GlobalSafetyAnomalyKind.CrossSiteIncidentSpike,
                    SiteId = siteId,
                    DeviationMagnitude = deviationMagnitude,
                    Message = "Test anomaly",
                    DetectedAtUtc = FixedTimestamp,
                },
            ],
            [], TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor);

    [Fact]
    public async Task ReactAsync_WithHighDeviationAnomaly_RoutesIncidentEscalation()
    {
        var site = Guid.NewGuid();
        var aggregate = ComputeWithAnomaly(site, deviationMagnitude: 5.0);

        _router.RouteAsync(Arg.Any<MultiSiteActionRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result<MultiSiteActionRouteResult>.Success(new MultiSiteActionRouteResult(
                [site], [], RequiresApproval: false, Executed: true, DispatchSucceeded: true, DispatchErrorMessage: null, WorkflowInstanceId: null)));

        await Sut().ReactAsync(aggregate, TestCorrelationId.Value, TestCausationId.Value, CancellationToken.None);

        await _router.Received(1).RouteAsync(
            Arg.Is<MultiSiteActionRequest>(r => r.ActionType == CrossSiteActionType.IncidentEscalation && r.CandidateSiteIds.Contains(site)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ReactAsync_WithLowDeviationAnomaly_DoesNotRoute()
    {
        var site = Guid.NewGuid();
        var aggregate = ComputeWithAnomaly(site, deviationMagnitude: 1.0);

        await Sut().ReactAsync(aggregate, TestCorrelationId.Value, TestCausationId.Value, CancellationToken.None);

        await _router.DidNotReceiveWithAnyArgs().RouteAsync(default!, default);
    }
}
