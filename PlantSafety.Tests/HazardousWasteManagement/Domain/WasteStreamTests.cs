// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.HazardousWasteManagement.Domain
//  Layer:          Tests (Unit)
//  File:           WasteStreamTests.cs
//  Created:        2026-08-04
//  Author:         Testing Agent
//  SchemaVersion:  2026.2
//  Description:    Lifecycle/invariant tests for the WasteStream aggregate root: characterization,
//                  versioned re-characterization, deactivation, and invalid transition guards.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Domain.HazardousWasteManagement.WasteStreams.Enums;
using PlantSafety.Domain.HazardousWasteManagement.WasteStreams.Events;
using Agg = PlantSafety.Domain.HazardousWasteManagement.WasteStreams.Aggregates.WasteStream;

namespace PlantSafety.Tests.HazardousWasteManagement.Domain;

public sealed class WasteStreamTests : UnitTestBase
{
    private static Agg CreateStream(WasteHazardClass hazardClass = WasteHazardClass.Ignitable) =>
        Agg.Create(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            TestTenantId.Value, siteId: null,
            streamName: "Spent Solvent Stream",
            epaWasteCode: "D001",
            hazardClass: hazardClass,
            generatorEpaId: "EPA123456789");

    [Fact]
    public void Create_WithValidData_InitializesCharacterizedStreamAndRaisesEvent()
    {
        var stream = CreateStream();

        stream.Status.Should().Be(WasteStreamStatus.Characterized);
        stream.StreamName.Should().Be("Spent Solvent Stream");
        stream.EpaWasteCode.Should().Be("D001");
        stream.HazardClass.Should().Be(WasteHazardClass.Ignitable);
        stream.CharacterizationVersion.Should().Be(1);
        stream.TenantId.Should().Be(TestTenantId.Value);
        stream.CreatedBy.Should().Be(TestUserId);

        stream.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<WasteStreamCharacterizedEvent>();
    }

    [Fact]
    public void Create_TrimsWhitespaceFromTextFields()
    {
        var stream = Agg.Create(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            TestTenantId.Value, null, "  Padded Stream  ", "  D001  ", WasteHazardClass.Corrosive, "  EPA999  ");

        stream.StreamName.Should().Be("Padded Stream");
        stream.EpaWasteCode.Should().Be("D001");
        stream.GeneratorEpaId.Should().Be("EPA999");
    }

    [Fact]
    public void Recharacterize_FromCharacterized_IncrementsVersionAndUpdatesFields()
    {
        var stream = CreateStream();
        stream.ClearDomainEvents();

        stream.Recharacterize("D002", WasteHazardClass.Corrosive, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        stream.EpaWasteCode.Should().Be("D002");
        stream.HazardClass.Should().Be(WasteHazardClass.Corrosive);
        stream.CharacterizationVersion.Should().Be(2);
        stream.Status.Should().Be(WasteStreamStatus.Characterized);
        stream.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<WasteStreamRecharacterizedEvent>();
    }

    [Fact]
    public void Recharacterize_CalledTwice_KeepsIncrementingVersionMonotonically()
    {
        var stream = CreateStream();

        stream.Recharacterize("D002", WasteHazardClass.Corrosive, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        stream.Recharacterize("D003", WasteHazardClass.Reactive, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        stream.CharacterizationVersion.Should().Be(3);
        stream.EpaWasteCode.Should().Be("D003");
    }

    [Fact]
    public void Deactivate_FromCharacterized_TransitionsAndRaisesEvent()
    {
        var stream = CreateStream();
        stream.ClearDomainEvents();

        stream.Deactivate(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        stream.Status.Should().Be(WasteStreamStatus.Deactivated);
        stream.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<WasteStreamDeactivatedEvent>();
    }

    [Fact]
    public void Deactivate_WhenAlreadyDeactivated_ThrowsInvalidOperationException()
    {
        var stream = CreateStream();
        stream.Deactivate(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => stream.Deactivate(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Invalid lifecycle transition*");
    }

    [Fact]
    public void Recharacterize_AfterDeactivation_ThrowsInvalidOperationException()
    {
        var stream = CreateStream();
        stream.Deactivate(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => stream.Recharacterize("D004", WasteHazardClass.Toxic, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ClearDomainEvents_RemovesAllPendingEvents()
    {
        var stream = CreateStream();
        stream.DomainEvents.Should().NotBeEmpty();

        stream.ClearDomainEvents();

        stream.DomainEvents.Should().BeEmpty();
    }
}
