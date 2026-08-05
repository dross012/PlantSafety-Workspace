// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.LogisticsTransportSafety.DriverQualifications
//  Layer:          Tests (Unit)
//  File:           DriverQualificationAggregateTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    Unit tests for the DriverQualification aggregate root: qualification, suspension,
//                  reinstatement, and retirement lifecycle, plus invariant-guard tests for illegal
//                  transitions.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Domain.LogisticsTransportSafety.DriverQualifications.Aggregates;
using PlantSafety.Domain.LogisticsTransportSafety.DriverQualifications.Enums;
using PlantSafety.Domain.LogisticsTransportSafety.DriverQualifications.Events;
using PlantSafety.Domain.LogisticsTransportSafety.DriverQualifications.Exceptions;
using PlantSafety.Tests.Common.Extensions;

namespace PlantSafety.Tests.LogisticsTransportSafety.DriverQualifications;

/// <summary>Unit tests for <see cref="DriverQualification"/> in the LogisticsTransportSafety bounded context.</summary>
public sealed class DriverQualificationAggregateTests : UnitTestBase
{
    private static readonly Guid DriverUserId = new("00000005-0000-0000-0000-000000000005");

    private static DriverQualification CreateQualified() =>
        DriverQualification.Create(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            tenantId: TestTenantId.Value,
            driverUserId: DriverUserId,
            qualificationType: DriverQualificationType.CommercialDriverLicense,
            licenseNumber: "CDL-88421",
            issuedAtUtc: UtcTimestamp.From(FixedUtcNow.AddYears(-1)),
            expiresAtUtc: UtcTimestamp.From(FixedUtcNow.AddYears(1)));

    // ---- Create --------------------------------------------------------------

    [Fact]
    public void Create_ShouldReturnAggregate() =>
        CreateQualified().Should().NotBeNull();

    [Fact]
    public void Create_ShouldSetStatusQualified()
    {
        var agg = CreateQualified();
        agg.Status.Should().Be(DriverQualificationStatus.Qualified);
    }

    [Fact]
    public void Create_ShouldTrimLicenseNumber()
    {
        var agg = DriverQualification.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            tenantId: TestTenantId.Value, driverUserId: DriverUserId, licenseNumber: "  CDL-1  ");
        agg.LicenseNumber.Should().Be("CDL-1");
    }

    [Fact]
    public void Create_ShouldRaiseDriverQualifiedEvent()
    {
        var agg = CreateQualified();
        var evt = agg.DomainEvents.ShouldRaiseSingleEvent<DriverQualifiedEvent>();
        evt.DriverUserId.Should().Be(DriverUserId);
    }

    // ---- Suspend ----------------------------------------------------------------

    [Fact]
    public void Suspend_WhenQualified_ShouldTransitionToSuspended()
    {
        var agg = CreateQualified();
        agg.Suspend("Failed drug test", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.Status.Should().Be(DriverQualificationStatus.Suspended);
    }

    [Fact]
    public void Suspend_ShouldSetSuspensionReason()
    {
        var agg = CreateQualified();
        agg.Suspend("  Failed drug test  ", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.SuspensionReason.Should().Be("Failed drug test");
    }

    [Fact]
    public void Suspend_ShouldRaiseEvent()
    {
        var agg = CreateQualified();
        agg.ClearDomainEvents();
        agg.Suspend("Failed drug test", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.DomainEvents.ShouldRaiseSingleEvent<DriverQualificationSuspendedEvent>();
    }

    [Fact]
    public void Suspend_WhenAlreadySuspended_ShouldThrowInvalidOperationException()
    {
        var agg = CreateQualified();
        agg.Suspend("Failed drug test", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => agg.Suspend("Second reason", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<InvalidOperationException>();
    }

    // ---- Reinstate ----------------------------------------------------------------

    private static DriverQualification CreateSuspended()
    {
        var agg = CreateQualified();
        agg.Suspend("Failed drug test", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        return agg;
    }

    [Fact]
    public void Reinstate_WhenSuspended_ShouldTransitionToQualified()
    {
        var agg = CreateSuspended();
        agg.Reinstate(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.Status.Should().Be(DriverQualificationStatus.Qualified);
    }

    [Fact]
    public void Reinstate_ShouldClearSuspensionReason()
    {
        var agg = CreateSuspended();
        agg.Reinstate(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.SuspensionReason.Should().BeNull();
    }

    [Fact]
    public void Reinstate_ShouldRaiseEvent()
    {
        var agg = CreateSuspended();
        agg.ClearDomainEvents();
        agg.Reinstate(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.DomainEvents.ShouldRaiseSingleEvent<DriverQualificationReinstatedEvent>();
    }

    [Fact]
    public void Reinstate_WhenQualified_ShouldThrowInvalidOperationException()
    {
        var agg = CreateQualified();
        var act = () => agg.Reinstate(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        act.Should().Throw<InvalidOperationException>();
    }

    // ---- Retire -----------------------------------------------------------------

    [Fact]
    public void Retire_WhenQualified_ShouldTransitionToRetired()
    {
        var agg = CreateQualified();
        agg.Retire(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.Status.Should().Be(DriverQualificationStatus.Retired);
    }

    [Fact]
    public void Retire_ShouldRaiseEvent()
    {
        var agg = CreateQualified();
        agg.ClearDomainEvents();
        agg.Retire(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.DomainEvents.ShouldRaiseSingleEvent<DriverQualificationRetiredEvent>();
    }

    [Fact]
    public void Retire_WhenAlreadyRetired_ShouldThrowInvalidStatusTransition()
    {
        var agg = CreateQualified();
        agg.Retire(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => agg.Retire(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<DriverQualificationInvalidStatusTransitionException>();
    }
}
