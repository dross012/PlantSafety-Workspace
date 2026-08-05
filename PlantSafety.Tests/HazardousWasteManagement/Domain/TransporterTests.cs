// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.HazardousWasteManagement.Domain
//  Layer:          Tests (Unit)
//  File:           TransporterTests.cs
//  Created:        2026-08-04
//  Author:         Testing Agent
//  SchemaVersion:  2026.2
//  Description:    Lifecycle/invariant tests for the Transporter aggregate root: register,
//                  record pickup, suspend, and invalid transition guards.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Domain.HazardousWasteManagement.Transporters.Enums;
using PlantSafety.Domain.HazardousWasteManagement.Transporters.Events;
using Agg = PlantSafety.Domain.HazardousWasteManagement.Transporters.Aggregates.Transporter;

namespace PlantSafety.Tests.HazardousWasteManagement.Domain;

public sealed class TransporterTests : UnitTestBase
{
    private static Agg RegisterTransporter() =>
        Agg.Create(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            TestTenantId.Value, siteId: null,
            companyName: "Acme Waste Hauling LLC",
            epaTransporterId: "EPA987654321",
            licenseExpiresAtUtc: UtcTimestamp.From(FixedUtcNow.AddYears(1)));

    [Fact]
    public void Create_WithValidData_RegistersActiveTransporterAndRaisesEvent()
    {
        var transporter = RegisterTransporter();

        transporter.Status.Should().Be(TransporterStatus.Active);
        transporter.CompanyName.Should().Be("Acme Waste Hauling LLC");
        transporter.EpaTransporterId.Should().Be("EPA987654321");

        transporter.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<TransporterRegisteredEvent>();
    }

    [Fact]
    public void RecordPickup_WhileActive_RaisesEventWithoutChangingStatus()
    {
        var transporter = RegisterTransporter();
        transporter.ClearDomainEvents();
        var manifestId = Guid.NewGuid();

        transporter.RecordPickup(manifestId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        transporter.Status.Should().Be(TransporterStatus.Active);
        transporter.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<TransporterPickupEvent>();
    }

    [Fact]
    public void RecordPickup_WhileSuspended_ThrowsInvalidOperationException()
    {
        var transporter = RegisterTransporter();
        transporter.Suspend(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => transporter.RecordPickup(Guid.NewGuid(), FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Suspend_FromActive_TransitionsAndRaisesEvent()
    {
        var transporter = RegisterTransporter();
        transporter.ClearDomainEvents();

        transporter.Suspend(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        transporter.Status.Should().Be(TransporterStatus.Suspended);
        transporter.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<TransporterSuspendedEvent>();
    }

    [Fact]
    public void Suspend_WhenAlreadySuspended_ThrowsInvalidOperationException()
    {
        var transporter = RegisterTransporter();
        transporter.Suspend(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => transporter.Suspend(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<InvalidOperationException>();
    }
}
