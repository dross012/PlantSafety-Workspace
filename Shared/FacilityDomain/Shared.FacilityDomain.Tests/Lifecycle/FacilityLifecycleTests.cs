// ============================================================================
//  Project:        Shared.FacilityDomain.Tests
//  File:           FacilityLifecycleTests.cs
//  Created:        2026-08-08
//  Description:    Phase C.12 unit tests for FacilityAggregate's lifecycle
//                  commands (ActivateFacility/ShutdownFacility/
//                  ReopenFacility/DecommissionFacility): valid transitions,
//                  raised events, and guard-clause rejections of invalid
//                  transitions.
// ============================================================================
using FluentAssertions;

using Shared.FacilityDomain.Abstractions.Aggregates;
using Shared.FacilityDomain.Abstractions.Events;
using Shared.FacilityDomain.Abstractions.Lifecycle;
using Shared.FacilityDomain.Abstractions.ValueObjects;

using Xunit;

namespace Shared.FacilityDomain.Tests.Lifecycle;

public sealed class FacilityLifecycleTests
{
    private static readonly FacilityIndustry Industry = new("Chemical Manufacturing");
    private static readonly FacilityLocation Location = new("100 Main St", "Springfield", "IL", "62701", "US");

    private static FacilityAggregate CreateFacility() =>
        FacilityAggregate.CreateFacility(FacilityId.New(), "Plant 1", Industry, Location, DateTime.UtcNow);

    [Fact]
    public void CreateFacility_StartsPendingActivation()
    {
        FacilityAggregate aggregate = CreateFacility();

        aggregate.State.LifecycleStatus.Should().Be(FacilityLifecycleStatus.PendingActivation);
    }

    [Fact]
    public void ActivateFacility_FromPending_TransitionsToActiveAndRaisesEvent()
    {
        FacilityAggregate aggregate = CreateFacility();

        aggregate.ActivateFacility(DateTime.UtcNow);

        aggregate.State.LifecycleStatus.Should().Be(FacilityLifecycleStatus.Active);
        aggregate.DomainEvents.Should().Contain(e => e is FacilityActivatedEvent);
    }

    [Fact]
    public void ActivateFacility_AlreadyActive_Throws()
    {
        FacilityAggregate aggregate = CreateFacility();
        aggregate.ActivateFacility(DateTime.UtcNow);

        Action act = () => aggregate.ActivateFacility(DateTime.UtcNow);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ShutdownFacility_FromActive_TransitionsToShutDownAndRaisesEvent()
    {
        FacilityAggregate aggregate = CreateFacility();
        aggregate.ActivateFacility(DateTime.UtcNow);

        aggregate.ShutdownFacility("Scheduled maintenance", DateTime.UtcNow);

        aggregate.State.LifecycleStatus.Should().Be(FacilityLifecycleStatus.ShutDown);
        aggregate.DomainEvents.OfType<FacilityShutdownEvent>().Should().Contain(e => e.Reason == "Scheduled maintenance");
    }

    [Fact]
    public void ShutdownFacility_FromPending_Throws()
    {
        FacilityAggregate aggregate = CreateFacility();

        Action act = () => aggregate.ShutdownFacility("Reason", DateTime.UtcNow);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ShutdownFacility_BlankReason_Throws()
    {
        FacilityAggregate aggregate = CreateFacility();
        aggregate.ActivateFacility(DateTime.UtcNow);

        Action act = () => aggregate.ShutdownFacility(" ", DateTime.UtcNow);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ReopenFacility_FromShutDown_TransitionsToActiveAndRaisesEvent()
    {
        FacilityAggregate aggregate = CreateFacility();
        aggregate.ActivateFacility(DateTime.UtcNow);
        aggregate.ShutdownFacility("Reason", DateTime.UtcNow);

        aggregate.ReopenFacility(DateTime.UtcNow);

        aggregate.State.LifecycleStatus.Should().Be(FacilityLifecycleStatus.Active);
        aggregate.DomainEvents.Should().Contain(e => e is FacilityReopenedEvent);
    }

    [Fact]
    public void ReopenFacility_FromActive_Throws()
    {
        FacilityAggregate aggregate = CreateFacility();
        aggregate.ActivateFacility(DateTime.UtcNow);

        Action act = () => aggregate.ReopenFacility(DateTime.UtcNow);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void DecommissionFacility_FromActive_TransitionsToDecommissionedAndRaisesEvent()
    {
        FacilityAggregate aggregate = CreateFacility();
        aggregate.ActivateFacility(DateTime.UtcNow);

        aggregate.DecommissionFacility("End of life", DateTime.UtcNow);

        aggregate.State.LifecycleStatus.Should().Be(FacilityLifecycleStatus.Decommissioned);
        aggregate.DomainEvents.OfType<FacilityDecommissionedEvent>().Should().Contain(e => e.Reason == "End of life");
    }

    [Fact]
    public void DecommissionFacility_FromShutDown_Succeeds()
    {
        FacilityAggregate aggregate = CreateFacility();
        aggregate.ActivateFacility(DateTime.UtcNow);
        aggregate.ShutdownFacility("Reason", DateTime.UtcNow);

        aggregate.DecommissionFacility("End of life", DateTime.UtcNow);

        aggregate.State.LifecycleStatus.Should().Be(FacilityLifecycleStatus.Decommissioned);
    }

    [Fact]
    public void DecommissionFacility_AlreadyDecommissioned_Throws()
    {
        FacilityAggregate aggregate = CreateFacility();
        aggregate.ActivateFacility(DateTime.UtcNow);
        aggregate.DecommissionFacility("End of life", DateTime.UtcNow);

        Action act = () => aggregate.DecommissionFacility("Again", DateTime.UtcNow);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void DecommissionFacility_BlankReason_Throws()
    {
        FacilityAggregate aggregate = CreateFacility();
        aggregate.ActivateFacility(DateTime.UtcNow);

        Action act = () => aggregate.DecommissionFacility(" ", DateTime.UtcNow);

        act.Should().Throw<ArgumentException>();
    }
}
