// ============================================================================
//  Project:        Shared.MachineDomain.Tests
//  File:           MachineServiceEventPublishingTests.cs
//  Created:        2026-08-10
//  Description:    Phase 4 (Machine domain pass), Section 9 "Domain
//                  Testing": proves MachineService actually publishes an
//                  integration event, of the correct concrete type, for
//                  every command -- through a real ModuleEventGateway, not
//                  a mock, so a subscriber registered via
//                  Subscribe<TConcreteEvent>() actually receives it.
//                  Mirrors Shared.FacilityDomain.Tests'/Shared.
//                  EmployeeDomain.Tests' equivalent, added there after a
//                  real bug (generic type inference erasing the concrete
//                  event type when routed through a common
//                  IModuleEvent?-typed helper) was caught only by live
//                  `dotnet run` verification, not by a compile error.
// ============================================================================
using FluentAssertions;

using Shared.Deterministic.Abstractions.Clock;
using Shared.FacilityDomain.Abstractions.Hazards;
using Shared.FacilityDomain.Abstractions.ValueObjects;
using Shared.MachineDomain.Abstractions.Aggregates;
using Shared.MachineDomain.Abstractions.Enums;
using Shared.MachineDomain.Abstractions.Location;
using Shared.MachineDomain.Abstractions.Repositories;
using Shared.MachineDomain.Infrastructure.Events;
using Shared.MachineDomain.Infrastructure.Persistence;
using Shared.MachineDomain.Infrastructure.Services;
using Shared.ModuleContracts.Abstractions.Events;
using Shared.Telemetry.Abstractions.Correlation;
using Shared.Telemetry.Abstractions.Events;

using Xunit;

namespace Shared.MachineDomain.Tests.Services;

public sealed class MachineServiceEventPublishingTests
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

    private static (MachineService Service, IModuleEventGateway Gateway) CreateService()
    {
        var gateway = new ModuleEventGateway();
        var repository = new MachineRepository(new MachineStore());
        var service = new MachineService(repository, new SystemClock(), gateway, new NoOpLogEventWriter(), new FixedCorrelationContext());
        return (service, gateway);
    }

    [Fact]
    public async Task CreateMachineAsync_PublishesMachineCreatedEvent_ToAConcretelyTypedSubscriber()
    {
        (MachineService service, IModuleEventGateway gateway) = CreateService();
        MachineCreatedEvent? received = null;
        using IDisposable subscription = gateway.Subscribe<MachineCreatedEvent>((evt, _) => { received = evt; return Task.CompletedTask; });

        await service.CreateMachineAsync("Press 1", MachineType.PressureVessel, Location);

        received.Should().NotBeNull();
        received!.Name.Should().Be("Press 1");
        received.SourceModule.Should().Be("Machine");
    }

    [Fact]
    public async Task UpdateMachineMetadataAsync_PublishesMachineUpdatedEvent()
    {
        (MachineService service, IModuleEventGateway gateway) = CreateService();
        MachineState created = await service.CreateMachineAsync("Press 1", MachineType.PressureVessel, Location);
        MachineUpdatedEvent? received = null;
        using IDisposable subscription = gateway.Subscribe<MachineUpdatedEvent>((evt, _) => { received = evt; return Task.CompletedTask; });

        await service.UpdateMachineMetadataAsync(created.MachineId, "Press 1 Renamed", MachineType.Boiler, Location);

        received.Should().NotBeNull();
        received!.UpdateKind.Should().Be("Metadata");
    }

    [Fact]
    public async Task UpdateMaintenanceProfileAsync_PublishesMachineMaintenanceUpdatedEvent()
    {
        (MachineService service, IModuleEventGateway gateway) = CreateService();
        MachineState created = await service.CreateMachineAsync("Press 1", MachineType.PressureVessel, Location);
        var nextMaintenance = new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc);
        MachineMaintenanceUpdatedEvent? received = null;
        using IDisposable subscription = gateway.Subscribe<MachineMaintenanceUpdatedEvent>((evt, _) => { received = evt; return Task.CompletedTask; });

        await service.UpdateMaintenanceProfileAsync(created.MachineId, null, nextMaintenance, 30);

        received.Should().NotBeNull();
        received!.NextScheduledMaintenanceAtUtc.Should().Be(nextMaintenance);
    }

    [Fact]
    public async Task RecordFailureAsync_PublishesMachineFailureRecordedEvent_AndMachineUpdatedEventForSafetyScore()
    {
        (MachineService service, IModuleEventGateway gateway) = CreateService();
        MachineState created = await service.CreateMachineAsync("Press 1", MachineType.PressureVessel, Location);
        MachineFailureRecordedEvent? failureReceived = null;
        var updatedReceived = new List<MachineUpdatedEvent>();
        using IDisposable failureSubscription = gateway.Subscribe<MachineFailureRecordedEvent>((evt, _) => { failureReceived = evt; return Task.CompletedTask; });
        using IDisposable updatedSubscription = gateway.Subscribe<MachineUpdatedEvent>((evt, _) => { updatedReceived.Add(evt); return Task.CompletedTask; });

        await service.RecordFailureAsync(created.MachineId, "Seal failure", HazardType.ChemicalSpill);

        failureReceived.Should().NotBeNull();
        failureReceived!.Description.Should().Be("Seal failure");
        failureReceived.ResultingHazardType.Should().Be(HazardType.ChemicalSpill);
        updatedReceived.Should().Contain(e => e.UpdateKind == "SafetyScore");
    }
}
