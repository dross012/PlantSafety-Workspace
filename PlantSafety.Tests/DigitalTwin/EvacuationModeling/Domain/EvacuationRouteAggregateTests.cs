// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.DigitalTwin.EvacuationModeling.Domain
//  Layer:          Tests (Unit)
//  File:           EvacuationRouteAggregateTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Lifecycle/invariant unit tests for the EvacuationRoute aggregate root
//                  (DigitalTwin.EvacuationModeling.EvacuationRoutes). Extra-careful regression
//                  coverage around obstruction recording/clearing and the RouteObstructionEntry
//                  WithCleared() record-with-expression, per the module hand-off note that this
//                  sub-domain's obstruction lifecycle was reconciled by hand after a mid-write
//                  collision between two build-out agents (duplicate RouteObstructionItemDto,
//                  a read-only-property bug on ClearedAtUtc, a missing GlobalUsing.cs entry).
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationRoutes.Aggregates;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationRoutes.Enums;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationRoutes.Events;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationRoutes.Exceptions;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationRoutes.ValueObjects.Ids;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationZones.ValueObjects.Ids;
using PlantSafety.Tests.Common.Extensions;

namespace PlantSafety.Tests.DigitalTwin.EvacuationModeling.Domain;

/// <summary>Unit tests for <see cref="EvacuationRoute"/> in <c>DigitalTwin.EvacuationModeling.EvacuationRoutes</c>.</summary>
public sealed class EvacuationRouteAggregateTests : UnitTestBase
{
    private static EvacuationRoute CreateRoute(int estimatedEgressTimeSeconds = 120) =>
        EvacuationRoute.Create(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            tenantId: TestTenantId.Value,
            siteId: null,
            routeName: "Corridor A -> Assembly Point 1",
            zoneId: EvacuationZoneId.New(),
            assemblyPointId: AssemblyPointId.New(),
            pathGeometryRef: "geo://twin/path/1",
            twinModelVersion: "v2026.1.0",
            estimatedEgressTimeSeconds: estimatedEgressTimeSeconds);

    // ---- Create ----------------------------------------------------------------

    [Fact]
    public void Create_ShouldReturnAggregateWithActiveStatus()
    {
        var route = CreateRoute();
        route.Status.Should().Be(EvacuationRouteStatus.Active);
    }

    [Fact]
    public void Create_ShouldAssignNonEmptyId()
    {
        var route = CreateRoute();
        route.EvacuationRouteId.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Create_ShouldRaiseDefinedEvent()
    {
        var route = CreateRoute();
        var evt = route.DomainEvents.ShouldRaiseSingleEvent<EvacuationRouteDefinedEvent>();
        evt.EvacuationRouteId.Should().Be(route.EvacuationRouteId);
        evt.SchemaVersion.Should().Be("2026.1");
    }

    [Fact]
    public void Create_WithBlankTwinModelVersion_ShouldThrow()
    {
        var act = () => EvacuationRoute.Create(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            tenantId: TestTenantId.Value, twinModelVersion: "   ");

        act.Should().Throw<ArgumentException>("route geometry must come only from a validated Digital Twin model version");
    }

    [Fact]
    public void Create_ShouldStartWithZeroObstructionCount()
    {
        var route = CreateRoute();
        route.ObstructionCount.Should().Be(0);
        route.ObstructionHistory.Should().BeEmpty();
    }

    // ---- RecordObstruction -------------------------------------------------------

    [Fact]
    public void RecordObstruction_OnActiveRoute_ShouldTransitionToObstructed()
    {
        var route = CreateRoute();
        var obstructionId = RouteObstructionId.New();

        route.RecordObstruction(obstructionId, "Debris blocking corridor", ObstructionSeverity.Moderate,
            FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        route.Status.Should().Be(EvacuationRouteStatus.Obstructed);
        route.ObstructionCount.Should().Be(1);
        route.ObstructionHistory.Should().ContainSingle(o => o.RouteObstructionId.Equals(obstructionId) && o.ClearedAtUtc == null);
    }

    [Fact]
    public void RecordObstruction_ShouldRaiseObstructedEvent()
    {
        var route = CreateRoute();
        route.ClearDomainEvents();
        var obstructionId = RouteObstructionId.New();

        route.RecordObstruction(obstructionId, "Blocked fire door", ObstructionSeverity.Severe,
            FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var evt = route.DomainEvents.ShouldRaiseSingleEvent<EvacuationRouteObstructedEvent>();
        evt.RouteObstructionId.Should().Be(obstructionId);
        evt.Severity.Should().Be(ObstructionSeverity.Severe);
    }

    [Fact]
    public void RecordObstruction_Twice_ShouldAccumulateCountAndRemainObstructed()
    {
        var route = CreateRoute();

        route.RecordObstruction(RouteObstructionId.New(), "First obstruction", ObstructionSeverity.Minor,
            FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        route.RecordObstruction(RouteObstructionId.New(), "Second obstruction", ObstructionSeverity.Moderate,
            FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        route.ObstructionCount.Should().Be(2);
        route.ObstructionHistory.Should().HaveCount(2);
        route.Status.Should().Be(EvacuationRouteStatus.Obstructed);
    }

    [Fact]
    public void RecordObstruction_OnRetiredRoute_ShouldThrow()
    {
        var route = CreateRoute();
        route.Retire(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => route.RecordObstruction(RouteObstructionId.New(), "Too late", ObstructionSeverity.Minor,
            FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<InvalidOperationException>();
    }

    // ---- ClearObstruction (regression-focused: reconciliation area) --------------

    [Fact]
    public void ClearObstruction_OnOpenObstruction_ShouldMarkClearedAndReturnToActive()
    {
        var route = CreateRoute();
        var obstructionId = RouteObstructionId.New();
        route.RecordObstruction(obstructionId, "Blocked door", ObstructionSeverity.Moderate,
            FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var clearedAt = UtcTimestamp.From(FixedUtcNow.AddMinutes(10));
        route.ClearObstruction(obstructionId, clearedAt, TestActor, TestCorrelationId, TestCausationId);

        route.Status.Should().Be(EvacuationRouteStatus.Active, "clearing the sole open obstruction must return the route to Active");
        var entry = route.ObstructionHistory.Single(o => o.RouteObstructionId.Equals(obstructionId));
        entry.ClearedAtUtc.Should().Be(clearedAt, "WithCleared() must produce a record whose ClearedAtUtc reflects the supplied timestamp");
    }

    [Fact]
    public void ClearObstruction_ShouldPreserveOriginalObstructionFields()
    {
        // Regression: RouteObstructionEntry.WithCleared() must be a true `with`-expression copy --
        // every field except ClearedAtUtc must be unchanged after clearing (guards against the
        // previously-reconciled read-only-property bug on ClearedAtUtc regressing).
        var route = CreateRoute();
        var obstructionId = RouteObstructionId.New();
        route.RecordObstruction(obstructionId, "Blocked stairwell landing", ObstructionSeverity.Severe,
            FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var originalEntry = route.ObstructionHistory.Single();
        var clearedAt = UtcTimestamp.From(FixedUtcNow.AddMinutes(5));

        route.ClearObstruction(obstructionId, clearedAt, TestActor, TestCorrelationId, TestCausationId);

        var clearedEntry = route.ObstructionHistory.Single();
        clearedEntry.RouteObstructionId.Should().Be(originalEntry.RouteObstructionId);
        clearedEntry.EvacuationRouteId.Should().Be(originalEntry.EvacuationRouteId);
        clearedEntry.Description.Should().Be(originalEntry.Description);
        clearedEntry.Severity.Should().Be(originalEntry.Severity);
        clearedEntry.RecordedAtUtc.Should().Be(originalEntry.RecordedAtUtc);
        clearedEntry.ClearedAtUtc.Should().Be(clearedAt);
    }

    [Fact]
    public void ClearObstruction_ShouldRaiseClearedEvent()
    {
        var route = CreateRoute();
        var obstructionId = RouteObstructionId.New();
        route.RecordObstruction(obstructionId, "Obstruction", ObstructionSeverity.Minor,
            FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        route.ClearDomainEvents();

        route.ClearObstruction(obstructionId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var evt = route.DomainEvents.ShouldRaiseSingleEvent<EvacuationRouteClearedEvent>();
        evt.RouteObstructionId.Should().Be(obstructionId);
    }

    [Fact]
    public void ClearObstruction_WithMultipleOpenObstructions_ShouldClearOnlyTheTargetedOne()
    {
        var route = CreateRoute();
        var first = RouteObstructionId.New();
        var second = RouteObstructionId.New();
        route.RecordObstruction(first, "First", ObstructionSeverity.Minor, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        route.RecordObstruction(second, "Second", ObstructionSeverity.Moderate, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        route.ClearObstruction(first, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        route.Status.Should().Be(EvacuationRouteStatus.Obstructed, "a second obstruction is still open");
        route.ObstructionHistory.Single(o => o.RouteObstructionId.Equals(first)).ClearedAtUtc.Should().NotBeNull();
        route.ObstructionHistory.Single(o => o.RouteObstructionId.Equals(second)).ClearedAtUtc.Should().BeNull();
    }

    [Fact]
    public void ClearObstruction_ReRecordedAfterClear_ShouldReObstructRoute()
    {
        // Regression: obstruct -> clear -> obstruct again must correctly flip status back to
        // Obstructed (exercises the reconciled RecordObstruction/ClearObstruction interplay).
        var route = CreateRoute();
        var first = RouteObstructionId.New();
        route.RecordObstruction(first, "First", ObstructionSeverity.Minor, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        route.ClearObstruction(first, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        route.Status.Should().Be(EvacuationRouteStatus.Active);

        var second = RouteObstructionId.New();
        route.RecordObstruction(second, "Second", ObstructionSeverity.Severe, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        route.Status.Should().Be(EvacuationRouteStatus.Obstructed);
        route.ObstructionCount.Should().Be(2);
    }

    [Fact]
    public void ClearObstruction_UnknownObstructionId_ShouldThrowNotFound()
    {
        var route = CreateRoute();
        route.RecordObstruction(RouteObstructionId.New(), "Obstruction", ObstructionSeverity.Minor,
            FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => route.ClearObstruction(RouteObstructionId.New(), FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<RouteObstructionNotFoundException>();
    }

    [Fact]
    public void ClearObstruction_AlreadyClearedObstruction_WhileAnotherRemainsOpen_ShouldThrowNotFound()
    {
        // Two obstructions open, clear the first (route stays Obstructed because the second is
        // still open), then attempt to clear the first one again: EnsureStatusIsOneOf(Obstructed)
        // passes (status is still Obstructed), so the re-clear attempt must be rejected by the
        // open-entry lookup itself, not by the status guard.
        var route = CreateRoute();
        var firstId = RouteObstructionId.New();
        var secondId = RouteObstructionId.New();
        route.RecordObstruction(firstId, "First", ObstructionSeverity.Minor, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        route.RecordObstruction(secondId, "Second", ObstructionSeverity.Moderate, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        route.ClearObstruction(firstId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => route.ClearObstruction(firstId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<RouteObstructionNotFoundException>("a re-clear of an already-cleared obstruction must not silently succeed");
    }

    [Fact]
    public void ClearObstruction_AlreadyClearedSoleObstruction_ShouldThrowInvalidOperation()
    {
        // When the only obstruction has already been cleared, the route has returned to Active, so
        // the re-clear attempt is rejected earlier by EnsureStatusIsOneOf(Obstructed) rather than
        // by the open-entry lookup.
        var route = CreateRoute();
        var obstructionId = RouteObstructionId.New();
        route.RecordObstruction(obstructionId, "Obstruction", ObstructionSeverity.Minor,
            FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        route.ClearObstruction(obstructionId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => route.ClearObstruction(obstructionId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<InvalidOperationException>("the route already returned to Active after its sole obstruction was cleared");
    }

    [Fact]
    public void ClearObstruction_OnRouteWithNoObstructions_ShouldThrowInvalidOperation()
    {
        var route = CreateRoute();

        var act = () => route.ClearObstruction(RouteObstructionId.New(), FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<InvalidOperationException>("ClearObstruction requires the route to already be Obstructed");
    }

    // ---- Retire ------------------------------------------------------------------

    [Fact]
    public void Retire_OnActiveRoute_ShouldTransitionToRetired()
    {
        var route = CreateRoute();
        route.Retire(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        route.Status.Should().Be(EvacuationRouteStatus.Retired);
    }

    [Fact]
    public void Retire_ShouldRaiseRetiredEvent()
    {
        var route = CreateRoute();
        route.ClearDomainEvents();

        route.Retire(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        route.DomainEvents.ShouldRaiseSingleEvent<EvacuationRouteRetiredEvent>();
    }

    [Fact]
    public void Retire_AlreadyRetired_ShouldThrow()
    {
        var route = CreateRoute();
        route.Retire(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => route.Retire(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<EvacuationRouteInvalidStatusTransitionException>();
    }

    [Fact]
    public void Retire_ObstructedRoute_ShouldStillTransitionToRetired()
    {
        var route = CreateRoute();
        route.RecordObstruction(RouteObstructionId.New(), "Obstruction", ObstructionSeverity.Minor,
            FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        route.Retire(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        route.Status.Should().Be(EvacuationRouteStatus.Retired);
    }

    // ---- ClearDomainEvents --------------------------------------------------------

    [Fact]
    public void ClearDomainEvents_ShouldEmptyList()
    {
        var route = CreateRoute();
        route.ClearDomainEvents();
        route.DomainEvents.Should().BeEmpty();
    }
}
