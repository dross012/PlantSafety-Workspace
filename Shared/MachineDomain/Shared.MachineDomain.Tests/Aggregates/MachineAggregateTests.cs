// ============================================================================
//  Project:        Shared.MachineDomain.Tests
//  File:           MachineAggregateTests.cs
//  Created:        2026-08-08
//  Description:    Phase C.2 unit tests for MachineAggregate: command
//                  correctness, immutable state replacement, safety-score
//                  recomputation, and domain event collection/clearing.
// ============================================================================
using FluentAssertions;

using Shared.FacilityDomain.Abstractions.Hazards;
using Shared.FacilityDomain.Abstractions.ValueObjects;

using Shared.MachineDomain.Abstractions.Aggregates;
using Shared.MachineDomain.Abstractions.Enums;
using Shared.MachineDomain.Abstractions.Events;
using Shared.MachineDomain.Abstractions.Location;
using Shared.MachineDomain.Abstractions.ValueObjects;

using Xunit;

namespace Shared.MachineDomain.Tests.Aggregates;

public sealed class MachineAggregateTests
{
    private static readonly MachineLocation Location = new(FacilityId.New(), "Building 2, Bay 4");

    [Fact]
    public void CreateMachine_SetsInitialStateAndRaisesMachineCreatedEvent()
    {
        var machineId = MachineId.New();
        var now = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        MachineAggregate aggregate = MachineAggregate.CreateMachine(machineId, "Press 1", MachineType.PressureVessel, Location, now);

        aggregate.State.MachineId.Should().Be(machineId);
        aggregate.State.Name.Should().Be("Press 1");
        aggregate.State.FailureRecords.Should().BeEmpty();
        aggregate.State.SafetyScore.ScoreValue.Should().Be(100);
        aggregate.State.CreatedAtUtc.Should().Be(now);

        aggregate.DomainEvents.Should().ContainSingle();
        aggregate.DomainEvents[0].Should().BeOfType<MachineCreatedEvent>()
            .Which.MachineId.Should().Be(machineId);
    }

    [Fact]
    public void CreateMachine_BlankName_Throws()
    {
        Action act = () => MachineAggregate.CreateMachine(MachineId.New(), "   ", MachineType.PressureVessel, Location, DateTime.UtcNow);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void UpdateMachineMetadata_ReplacesStateAndRaisesUpdatedEvent()
    {
        MachineAggregate aggregate = MachineAggregate.CreateMachine(MachineId.New(), "Press 1", MachineType.PressureVessel, Location, DateTime.UtcNow);

        aggregate.UpdateMachineMetadata("Press 1 Renamed", MachineType.Boiler, Location, DateTime.UtcNow);

        aggregate.State.Name.Should().Be("Press 1 Renamed");
        aggregate.State.MachineType.Should().Be(MachineType.Boiler);
        aggregate.DomainEvents.OfType<MachineUpdatedEvent>().Should().Contain(e => e.Kind == MachineUpdateKind.Metadata);
    }

    [Fact]
    public void UpdateMaintenanceProfile_OverdueSchedule_LowersScore()
    {
        MachineAggregate aggregate = MachineAggregate.CreateMachine(MachineId.New(), "Press 1", MachineType.PressureVessel, Location, DateTime.UtcNow);

        aggregate.UpdateMaintenanceProfile(DateTime.UtcNow.AddDays(-100), DateTime.UtcNow.AddDays(-10), 90, DateTime.UtcNow);

        aggregate.State.MaintenanceProfile.IsOverdue(DateTime.UtcNow).Should().BeTrue();
        aggregate.State.SafetyScore.ScoreValue.Should().Be(90);
        aggregate.DomainEvents.Should().Contain(e => e is MachineMaintenanceUpdatedEvent);
    }

    [Fact]
    public void RecordFailure_AppendsRecordAndLowersScore()
    {
        MachineAggregate aggregate = MachineAggregate.CreateMachine(MachineId.New(), "Press 1", MachineType.PressureVessel, Location, DateTime.UtcNow);

        aggregate.RecordFailure("Hydraulic line rupture", HazardType.ChemicalSpill, DateTime.UtcNow);

        aggregate.State.FailureRecords.Should().ContainSingle().Which.Description.Should().Be("Hydraulic line rupture");
        aggregate.State.SafetyScore.ScoreValue.Should().Be(80);
        aggregate.DomainEvents.Should().Contain(e => e is MachineFailureRecordedEvent);
    }

    [Fact]
    public void RecordFailure_BlankDescription_Throws()
    {
        MachineAggregate aggregate = MachineAggregate.CreateMachine(MachineId.New(), "Press 1", MachineType.PressureVessel, Location, DateTime.UtcNow);

        Action act = () => aggregate.RecordFailure(" ", HazardType.Other, DateTime.UtcNow);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ClearDomainEvents_EmptiesTheList()
    {
        MachineAggregate aggregate = MachineAggregate.CreateMachine(MachineId.New(), "Press 1", MachineType.PressureVessel, Location, DateTime.UtcNow);
        aggregate.DomainEvents.Should().NotBeEmpty();

        aggregate.ClearDomainEvents();

        aggregate.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void Rehydrate_StartsWithNoDomainEvents()
    {
        MachineAggregate original = MachineAggregate.CreateMachine(MachineId.New(), "Press 1", MachineType.PressureVessel, Location, DateTime.UtcNow);

        MachineAggregate rehydrated = MachineAggregate.Rehydrate(original.State);

        rehydrated.State.Should().Be(original.State);
        rehydrated.DomainEvents.Should().BeEmpty();
    }
}
