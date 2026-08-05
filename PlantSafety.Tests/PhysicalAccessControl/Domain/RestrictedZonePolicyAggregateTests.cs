// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Domain.PhysicalAccessControl
//  Layer:          Tests (Unit)
//  File:           RestrictedZonePolicyAggregateTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    Unit tests for the RestrictedZonePolicy aggregate root: definition,
//                  activation, suspension, and retirement lifecycle, invariant-guard tests for
//                  illegal transitions, and domain-event assertions.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Domain.PhysicalAccessControl.RestrictedZonePolicies.Aggregates;
using PlantSafety.Domain.PhysicalAccessControl.RestrictedZonePolicies.Enums;
using PlantSafety.Domain.PhysicalAccessControl.RestrictedZonePolicies.Events;
using PlantSafety.Tests.Common.Extensions;

namespace PlantSafety.Tests.PhysicalAccessControl.Domain;

/// <summary>Unit tests for <see cref="RestrictedZonePolicy"/> in the PhysicalAccessControl bounded context.</summary>
public sealed class RestrictedZonePolicyAggregateTests : UnitTestBase
{
    private static RestrictedZonePolicy CreateDraft(bool enforceAfterHoursLockout = false) =>
        RestrictedZonePolicy.Create(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            tenantId: TestTenantId.Value,
            safetyZoneId: Guid.NewGuid(),
            policyName: "High-Voltage Room Policy",
            allowedAccessLevel: "RestrictedZoneA",
            enforceAfterHoursLockout: enforceAfterHoursLockout);

    // ---- Create ----------------------------------------------------------------

    [Fact]
    public void Create_ShouldReturnAggregate() =>
        CreateDraft().Should().NotBeNull();

    [Fact]
    public void Create_ShouldSetStatusDraft()
    {
        var agg = CreateDraft();
        agg.Status.Should().Be(RestrictedZonePolicyStatus.Draft);
    }

    [Fact]
    public void Create_ShouldTrimPolicyNameAndAccessLevel()
    {
        var agg = RestrictedZonePolicy.Create(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            tenantId: TestTenantId.Value,
            safetyZoneId: Guid.NewGuid(),
            policyName: "  Confined Space Policy  ",
            allowedAccessLevel: "  PermitRequired  ");

        agg.PolicyName.Should().Be("Confined Space Policy");
        agg.AllowedAccessLevel.Should().Be("PermitRequired");
    }

    [Fact]
    public void Create_ShouldRaiseRestrictedZonePolicyDefinedEvent()
    {
        var agg = CreateDraft();
        agg.DomainEvents.ShouldRaiseSingleEvent<RestrictedZonePolicyDefinedEvent>();
    }

    [Fact]
    public void Create_WithEnforceAfterHoursLockout_ShouldPersistFlag()
    {
        var agg = CreateDraft(enforceAfterHoursLockout: true);
        agg.EnforceAfterHoursLockout.Should().BeTrue();
    }

    [Fact]
    public void Create_TwoAggregates_ShouldHaveDifferentIds()
    {
        var a1 = CreateDraft();
        var a2 = CreateDraft();
        a1.RestrictedZonePolicyId.Should().NotBe(a2.RestrictedZonePolicyId);
    }

    // ---- Activate ----------------------------------------------------------------

    [Fact]
    public void Activate_WhenDraft_ShouldTransitionToActive()
    {
        var agg = CreateDraft();
        agg.Activate(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.Status.Should().Be(RestrictedZonePolicyStatus.Active);
    }

    [Fact]
    public void Activate_ShouldRaiseEvent()
    {
        var agg = CreateDraft();
        agg.ClearDomainEvents();
        agg.Activate(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.DomainEvents.ShouldRaiseSingleEvent<RestrictedZonePolicyActivatedEvent>();
    }

    [Fact]
    public void Activate_FromSuspended_ShouldTransitionToActive()
    {
        var agg = CreateDraft();
        agg.Activate(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.Suspend(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        agg.Activate(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        agg.Status.Should().Be(RestrictedZonePolicyStatus.Active);
    }

    [Fact]
    public void Activate_WhenRetired_ShouldThrowInvalidOperationException()
    {
        var agg = CreateDraft();
        agg.Retire(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => agg.Activate(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<InvalidOperationException>();
    }

    // ---- Suspend -------------------------------------------------------------------

    [Fact]
    public void Suspend_WhenActive_ShouldTransitionToSuspended()
    {
        var agg = CreateDraft();
        agg.Activate(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        agg.Suspend(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        agg.Status.Should().Be(RestrictedZonePolicyStatus.Suspended);
    }

    [Fact]
    public void Suspend_ShouldRaiseEvent()
    {
        var agg = CreateDraft();
        agg.Activate(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.ClearDomainEvents();

        agg.Suspend(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        agg.DomainEvents.ShouldRaiseSingleEvent<RestrictedZonePolicySuspendedEvent>();
    }

    [Fact]
    public void Suspend_WhenDraft_ShouldThrowInvalidOperationException()
    {
        var agg = CreateDraft();
        var act = () => agg.Suspend(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        act.Should().Throw<InvalidOperationException>();
    }

    // ---- Retire --------------------------------------------------------------------

    [Theory]
    [InlineData(RestrictedZonePolicyStatus.Draft)]
    [InlineData(RestrictedZonePolicyStatus.Active)]
    [InlineData(RestrictedZonePolicyStatus.Suspended)]
    public void Retire_FromAnyNonTerminalState_ShouldTransitionToRetired(RestrictedZonePolicyStatus fromStatus)
    {
        var agg = CreateDraft();
        if (fromStatus is RestrictedZonePolicyStatus.Active or RestrictedZonePolicyStatus.Suspended)
            agg.Activate(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        if (fromStatus is RestrictedZonePolicyStatus.Suspended)
            agg.Suspend(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        agg.Retire(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        agg.Status.Should().Be(RestrictedZonePolicyStatus.Retired);
    }

    [Fact]
    public void Retire_ShouldRaiseEvent()
    {
        var agg = CreateDraft();
        agg.ClearDomainEvents();
        agg.Retire(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.DomainEvents.ShouldRaiseSingleEvent<RestrictedZonePolicyRetiredEvent>();
    }

    [Fact]
    public void Retire_WhenAlreadyRetired_ShouldThrowInvalidOperationException()
    {
        var agg = CreateDraft();
        agg.Retire(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => agg.Retire(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<InvalidOperationException>();
    }
}
