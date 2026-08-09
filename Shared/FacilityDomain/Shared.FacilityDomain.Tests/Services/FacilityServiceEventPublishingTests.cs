// ============================================================================
//  Project:        Shared.FacilityDomain.Tests
//  File:           FacilityServiceEventPublishingTests.cs
//  Created:        2026-08-10
//  Description:    Phase 4 (Facility domain pass), Section 9 "Domain
//                  Testing": proves FacilityService actually publishes an
//                  integration event, of the correct concrete type, for
//                  every command -- through a real ModuleEventGateway, not
//                  a mock, so a subscriber registered via
//                  Subscribe<TConcreteEvent>() (exactly how
//                  PlantSafety.UI.Api's Program.cs subscribes) actually
//                  receives it.
//
//                  This exists because of a real bug caught while manually
//                  verifying this exact behavior via a live `dotnet run`:
//                  the first version of FacilityService.PublishAndClearAsync
//                  translated every domain event through a helper returning
//                  IModuleEvent?, then called PublishIsolatedAsync on that
//                  IModuleEvent?-typed variable. IModuleEventGateway.
//                  PublishAsync<TEvent> is generic, and C# infers TEvent
//                  from the variable's *compile-time* type -- so every
//                  event silently published under typeof(IModuleEvent)
//                  instead of its real concrete type, and Subscribe<
//                  FacilityCreatedEvent>() never received anything. No
//                  exception, no failed-publish log -- it just silently
//                  went to zero subscribers. These tests exist so that
//                  regression is caught by `dotnet build`+test, not by a
//                  human noticing a missing log line in a live run.
// ============================================================================
using FluentAssertions;

using Shared.Deterministic.Abstractions.Clock;
using Shared.FacilityDomain.Abstractions.Aggregates;
using Shared.FacilityDomain.Abstractions.Repositories;
using Shared.FacilityDomain.Abstractions.ValueObjects;
using Shared.FacilityDomain.Infrastructure.Events;
using Shared.FacilityDomain.Infrastructure.Persistence;
using Shared.FacilityDomain.Infrastructure.Services;
using Shared.ModuleContracts.Abstractions.Events;
using Shared.Telemetry.Abstractions.Correlation;
using Shared.Telemetry.Abstractions.Events;

using Xunit;

namespace Shared.FacilityDomain.Tests.Services;

public sealed class FacilityServiceEventPublishingTests
{
    private static readonly FacilityIndustry Industry = new("Medical Devices");
    private static readonly FacilityLocation Location = new("1 Device Way", "Boston", "MA", "02101", "US");

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

    private static (FacilityService Service, IModuleEventGateway Gateway) CreateService()
    {
        var gateway = new ModuleEventGateway();
        var repository = new FacilityRepository(new FacilityStore());
        var service = new FacilityService(repository, new SystemClock(), gateway, new NoOpLogEventWriter(), new FixedCorrelationContext());
        return (service, gateway);
    }

    [Fact]
    public async Task CreateFacilityAsync_PublishesFacilityCreatedEvent_ToAConcretelyTypedSubscriber()
    {
        (FacilityService service, IModuleEventGateway gateway) = CreateService();
        FacilityCreatedEvent? received = null;
        using IDisposable subscription = gateway.Subscribe<FacilityCreatedEvent>((evt, _) => { received = evt; return Task.CompletedTask; });

        await service.CreateFacilityAsync("Plant 1", Industry, Location);

        received.Should().NotBeNull();
        received!.Name.Should().Be("Plant 1");
        received.SourceModule.Should().Be("Facility");
    }

    [Fact]
    public async Task UpdateHazardProfileAsync_PublishesFacilityUpdatedEvent()
    {
        (FacilityService service, IModuleEventGateway gateway) = CreateService();
        FacilityState created = await service.CreateFacilityAsync("Plant 1", Industry, Location);
        FacilityUpdatedEvent? received = null;
        using IDisposable subscription = gateway.Subscribe<FacilityUpdatedEvent>((evt, _) => { received = evt; return Task.CompletedTask; });

        await service.UpdateHazardProfileAsync(created.FacilityId, [Shared.FacilityDomain.Abstractions.Hazards.HazardType.Fire]);

        received.Should().NotBeNull();
        received!.UpdateKind.Should().Be("HazardProfile");
    }

    [Fact]
    public async Task AddBuildingAsync_PublishesFacilityHierarchyUpdatedEvent()
    {
        (FacilityService service, IModuleEventGateway gateway) = CreateService();
        FacilityState created = await service.CreateFacilityAsync("Plant 1", Industry, Location);
        FacilityHierarchyUpdatedEvent? received = null;
        using IDisposable subscription = gateway.Subscribe<FacilityHierarchyUpdatedEvent>((evt, _) => { received = evt; return Task.CompletedTask; });

        await service.AddBuildingAsync(created.FacilityId, "Building A");

        received.Should().NotBeNull();
        received!.ChangeKind.Should().Be("BuildingAdded");
    }

    [Fact]
    public async Task ActivateFacilityAsync_PublishesFacilityLifecycleChangedEvent_WithActiveStatus()
    {
        (FacilityService service, IModuleEventGateway gateway) = CreateService();
        FacilityState created = await service.CreateFacilityAsync("Plant 1", Industry, Location);
        FacilityLifecycleChangedEvent? received = null;
        using IDisposable subscription = gateway.Subscribe<FacilityLifecycleChangedEvent>((evt, _) => { received = evt; return Task.CompletedTask; });

        await service.ActivateFacilityAsync(created.FacilityId);

        received.Should().NotBeNull();
        received!.NewStatus.Should().Be("Active");
        received.Reason.Should().BeNull();
    }

    [Fact]
    public async Task ShutdownFacilityAsync_PublishesFacilityLifecycleChangedEvent_WithReason()
    {
        (FacilityService service, IModuleEventGateway gateway) = CreateService();
        FacilityState created = await service.CreateFacilityAsync("Plant 1", Industry, Location);
        await service.ActivateFacilityAsync(created.FacilityId);
        FacilityLifecycleChangedEvent? received = null;
        using IDisposable subscription = gateway.Subscribe<FacilityLifecycleChangedEvent>((evt, _) => { received = evt; return Task.CompletedTask; });

        await service.ShutdownFacilityAsync(created.FacilityId, "Scheduled maintenance");

        received.Should().NotBeNull();
        received!.NewStatus.Should().Be("ShutDown");
        received.Reason.Should().Be("Scheduled maintenance");
    }

    [Fact]
    public async Task DecommissionFacilityAsync_PublishesFacilityLifecycleChangedEvent_WithDecommissionedStatus()
    {
        (FacilityService service, IModuleEventGateway gateway) = CreateService();
        FacilityState created = await service.CreateFacilityAsync("Plant 1", Industry, Location);
        FacilityLifecycleChangedEvent? received = null;
        using IDisposable subscription = gateway.Subscribe<FacilityLifecycleChangedEvent>((evt, _) => { received = evt; return Task.CompletedTask; });

        await service.DecommissionFacilityAsync(created.FacilityId, "End of life");

        received.Should().NotBeNull();
        received!.NewStatus.Should().Be("Decommissioned");
        received.Reason.Should().Be("End of life");
    }
}
