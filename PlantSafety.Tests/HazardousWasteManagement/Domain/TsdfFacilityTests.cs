// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.HazardousWasteManagement.Domain
//  Layer:          Tests (Unit)
//  File:           TsdfFacilityTests.cs
//  Created:        2026-08-04
//  Author:         Testing Agent
//  SchemaVersion:  2026.2
//  Description:    Lifecycle/invariant tests for the TsdfFacility aggregate root: register,
//                  record receipt, suspend, and invalid transition guards.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Domain.HazardousWasteManagement.TsdfFacilities.Enums;
using PlantSafety.Domain.HazardousWasteManagement.TsdfFacilities.Events;
using Agg = PlantSafety.Domain.HazardousWasteManagement.TsdfFacilities.Aggregates.TsdfFacility;

namespace PlantSafety.Tests.HazardousWasteManagement.Domain;

public sealed class TsdfFacilityTests : UnitTestBase
{
    private static Agg RegisterFacility() =>
        Agg.Create(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            TestTenantId.Value, siteId: null,
            facilityName: "Regional TSDF Center",
            epaFacilityId: "EPA555000111",
            permitExpiresAtUtc: UtcTimestamp.From(FixedUtcNow.AddYears(2)));

    [Fact]
    public void Create_WithValidData_RegistersActiveFacilityAndRaisesEvent()
    {
        var facility = RegisterFacility();

        facility.Status.Should().Be(TsdfFacilityStatus.Active);
        facility.FacilityName.Should().Be("Regional TSDF Center");
        facility.EpaFacilityId.Should().Be("EPA555000111");

        facility.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<TsdfFacilityRegisteredEvent>();
    }

    [Fact]
    public void RecordReceipt_WhileActive_RaisesEventWithoutChangingStatus()
    {
        var facility = RegisterFacility();
        facility.ClearDomainEvents();
        var manifestId = Guid.NewGuid();

        facility.RecordReceipt(manifestId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        facility.Status.Should().Be(TsdfFacilityStatus.Active);
        facility.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<TsdfReceiptEvent>();
    }

    [Fact]
    public void RecordReceipt_WhileSuspended_ThrowsInvalidOperationException()
    {
        var facility = RegisterFacility();
        facility.Suspend(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => facility.RecordReceipt(Guid.NewGuid(), FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Suspend_FromActive_TransitionsAndRaisesEvent()
    {
        var facility = RegisterFacility();
        facility.ClearDomainEvents();

        facility.Suspend(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        facility.Status.Should().Be(TsdfFacilityStatus.Suspended);
        facility.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<TsdfFacilitySuspendedEvent>();
    }

    [Fact]
    public void Suspend_WhenAlreadySuspended_ThrowsInvalidOperationException()
    {
        var facility = RegisterFacility();
        facility.Suspend(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => facility.Suspend(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<InvalidOperationException>();
    }
}
