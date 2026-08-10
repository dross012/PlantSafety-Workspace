// ============================================================================
//  Project:        Shared.MachineDomain.Tests
//  File:           MachineRepositoryTests.cs
//  Created:        2026-08-08
//  Description:    Phase C.2 unit tests for MachineRepository/MachineStore
//                  and MachineService's orchestration over them.
// ============================================================================
using FluentAssertions;

using Shared.Deterministic.Abstractions.Clock;
using Shared.FacilityDomain.Abstractions.Hazards;
using Shared.FacilityDomain.Abstractions.ValueObjects;

using Shared.MachineDomain.Abstractions.Aggregates;
using Shared.MachineDomain.Abstractions.Enums;
using Shared.MachineDomain.Abstractions.Location;
using Shared.MachineDomain.Abstractions.Repositories;
using Shared.MachineDomain.Abstractions.ValueObjects;
using Shared.MachineDomain.Infrastructure.Persistence;
using Shared.MachineDomain.Infrastructure.Services;
using Shared.ModuleContracts.Abstractions.Events;
using Shared.Telemetry.Abstractions.Correlation;
using Shared.Telemetry.Abstractions.Events;

using Xunit;

namespace Shared.MachineDomain.Tests.Repositories;

public sealed class MachineRepositoryTests
{
    private static readonly MachineLocation Location = new(FacilityId.New(), "Building 2, Bay 4");

    private sealed class NoOpLogEventWriter : ILogEventWriter
    {
        public void Write(ILogEvent logEvent)
        {
        }
    }

    private sealed class FixedCorrelationContext : ICorrelationContext
    {
        public string CorrelationId => "test-correlation";
    }

    private static MachineService CreateService(IMachineRepository repository) =>
        new(repository, new SystemClock(), new ModuleEventGateway(), new NoOpLogEventWriter(), new FixedCorrelationContext());

    [Fact]
    public async Task FindAsync_UnknownMachineId_ReturnsNull()
    {
        var repository = new MachineRepository(new MachineStore());

        MachineAggregate? result = await repository.FindAsync(MachineId.New());

        result.Should().BeNull();
    }

    [Fact]
    public async Task SaveAsync_ThenFindAsync_RoundTripsState()
    {
        var repository = new MachineRepository(new MachineStore());
        MachineAggregate created = MachineAggregate.CreateMachine(MachineId.New(), "Press 1", MachineType.PressureVessel, Location, DateTime.UtcNow);

        await repository.SaveAsync(created);
        MachineAggregate? found = await repository.FindAsync(created.State.MachineId);

        found.Should().NotBeNull();
        found!.State.Should().Be(created.State);
    }

    [Fact]
    public async Task SaveAsync_ExistingMachineId_Overwrites()
    {
        var store = new MachineStore();
        var repository = new MachineRepository(store);
        MachineAggregate aggregate = MachineAggregate.CreateMachine(MachineId.New(), "Press 1", MachineType.PressureVessel, Location, DateTime.UtcNow);
        await repository.SaveAsync(aggregate);

        aggregate.UpdateMachineMetadata("Press 1 Renamed", MachineType.Boiler, Location, DateTime.UtcNow);
        await repository.SaveAsync(aggregate);

        MachineAggregate? found = await repository.FindAsync(aggregate.State.MachineId);
        found!.State.Name.Should().Be("Press 1 Renamed");
    }

    [Fact]
    public async Task GetAllAsync_ReturnsEveryPersistedMachine()
    {
        var repository = new MachineRepository(new MachineStore());
        MachineAggregate first = MachineAggregate.CreateMachine(MachineId.New(), "Press 1", MachineType.PressureVessel, Location, DateTime.UtcNow);
        MachineAggregate second = MachineAggregate.CreateMachine(MachineId.New(), "Conveyor 1", MachineType.ConveyorSystem, Location, DateTime.UtcNow);
        await repository.SaveAsync(first);
        await repository.SaveAsync(second);

        IReadOnlyList<MachineAggregate> all = await repository.GetAllAsync();

        all.Should().HaveCount(2);
        all.Select(a => a.State.MachineId).Should().BeEquivalentTo([first.State.MachineId, second.State.MachineId]);
    }

    [Fact]
    public async Task MachineService_CreateThenRecordFailure_PersistsChange()
    {
        IMachineService service = CreateService(new MachineRepository(new MachineStore()));

        MachineState created = await service.CreateMachineAsync("Press 1", MachineType.PressureVessel, Location);
        MachineState? updated = await service.RecordFailureAsync(created.MachineId, "Seal failure", HazardType.ChemicalSpill);

        updated.Should().NotBeNull();
        updated!.FailureRecords.Should().ContainSingle();
    }

    [Fact]
    public async Task MachineService_UpdateUnknownMachine_ReturnsNull()
    {
        IMachineService service = CreateService(new MachineRepository(new MachineStore()));

        MachineState? result = await service.UpdateMaintenanceProfileAsync(MachineId.New(), null, null, 30);

        result.Should().BeNull();
    }

    [Fact]
    public async Task MachineService_GetAllMachinesAsync_ReflectsCreatedMachines()
    {
        IMachineService service = CreateService(new MachineRepository(new MachineStore()));
        await service.CreateMachineAsync("Press 1", MachineType.PressureVessel, Location);
        await service.CreateMachineAsync("Conveyor 1", MachineType.ConveyorSystem, Location);

        IReadOnlyList<MachineState> all = await service.GetAllMachinesAsync();

        all.Should().HaveCount(2);
    }
}
