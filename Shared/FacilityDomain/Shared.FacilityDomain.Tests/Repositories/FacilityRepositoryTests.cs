// ============================================================================
//  Project:        Shared.FacilityDomain.Tests
//  File:           FacilityRepositoryTests.cs
//  Created:        2026-08-08
//  Description:    Phase B.1 unit tests for FacilityRepository/FacilityStore
//                  and FacilityService's orchestration over them.
// ============================================================================
using FluentAssertions;

using Shared.Deterministic.Abstractions.Clock;
using Shared.FacilityDomain.Abstractions.Aggregates;
using Shared.FacilityDomain.Abstractions.Hazards;
using Shared.FacilityDomain.Abstractions.Repositories;
using Shared.FacilityDomain.Abstractions.ValueObjects;
using Shared.FacilityDomain.Infrastructure.Persistence;
using Shared.FacilityDomain.Infrastructure.Services;
using Shared.ModuleContracts.Abstractions.Events;
using Shared.Telemetry.Abstractions.Correlation;
using Shared.Telemetry.Abstractions.Events;

using Xunit;

namespace Shared.FacilityDomain.Tests.Repositories;

public sealed class FacilityRepositoryTests
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

    private static FacilityService CreateService(IFacilityRepository repository) =>
        new(repository, new SystemClock(), new ModuleEventGateway(), new NoOpLogEventWriter(), new FixedCorrelationContext());

    [Fact]
    public async Task FindAsync_UnknownFacilityId_ReturnsNull()
    {
        var repository = new FacilityRepository(new FacilityStore());

        FacilityAggregate? result = await repository.FindAsync(FacilityId.New());

        result.Should().BeNull();
    }

    [Fact]
    public async Task SaveAsync_ThenFindAsync_RoundTripsState()
    {
        var repository = new FacilityRepository(new FacilityStore());
        FacilityAggregate created = FacilityAggregate.CreateFacility(FacilityId.New(), "Plant 1", Industry, Location, DateTime.UtcNow);

        await repository.SaveAsync(created);
        FacilityAggregate? found = await repository.FindAsync(created.State.FacilityId);

        found.Should().NotBeNull();
        found!.State.Should().Be(created.State);
    }

    [Fact]
    public async Task SaveAsync_ExistingFacilityId_Overwrites()
    {
        var store = new FacilityStore();
        var repository = new FacilityRepository(store);
        FacilityAggregate aggregate = FacilityAggregate.CreateFacility(FacilityId.New(), "Plant 1", Industry, Location, DateTime.UtcNow);
        await repository.SaveAsync(aggregate);

        aggregate.UpdateFacilityMetadata("Plant 1 Renamed", Industry, Location, DateTime.UtcNow);
        await repository.SaveAsync(aggregate);

        FacilityAggregate? found = await repository.FindAsync(aggregate.State.FacilityId);
        found!.State.Name.Should().Be("Plant 1 Renamed");
    }

    [Fact]
    public async Task GetAllAsync_ReturnsEveryPersistedFacility()
    {
        var repository = new FacilityRepository(new FacilityStore());
        FacilityAggregate first = FacilityAggregate.CreateFacility(FacilityId.New(), "Plant 1", Industry, Location, DateTime.UtcNow);
        FacilityAggregate second = FacilityAggregate.CreateFacility(FacilityId.New(), "Plant 2", Industry, Location, DateTime.UtcNow);
        await repository.SaveAsync(first);
        await repository.SaveAsync(second);

        IReadOnlyList<FacilityAggregate> all = await repository.GetAllAsync();

        all.Should().HaveCount(2);
        all.Select(a => a.State.FacilityId).Should().BeEquivalentTo([first.State.FacilityId, second.State.FacilityId]);
    }

    [Fact]
    public async Task FacilityService_CreateThenUpdateHazardProfile_PersistsChange()
    {
        IFacilityService service = CreateService(new FacilityRepository(new FacilityStore()));

        FacilityState created = await service.CreateFacilityAsync("Plant 1", Industry, Location);
        FacilityState? updated = await service.UpdateHazardProfileAsync(created.FacilityId, [HazardType.Fire]);

        updated.Should().NotBeNull();
        updated!.HazardProfile.ActiveHazardTypes.Should().ContainSingle().Which.Should().Be(HazardType.Fire);
    }

    [Fact]
    public async Task FacilityService_UpdateUnknownFacility_ReturnsNull()
    {
        IFacilityService service = CreateService(new FacilityRepository(new FacilityStore()));

        FacilityState? result = await service.UpdateRegulatoryProfileAsync(FacilityId.New(), ["OSHA"]);

        result.Should().BeNull();
    }

    [Fact]
    public async Task FacilityService_GetAllFacilitiesAsync_ReflectsCreatedFacilities()
    {
        IFacilityService service = CreateService(new FacilityRepository(new FacilityStore()));
        await service.CreateFacilityAsync("Plant 1", Industry, Location);
        await service.CreateFacilityAsync("Plant 2", Industry, Location);

        IReadOnlyList<FacilityState> all = await service.GetAllFacilitiesAsync();

        all.Should().HaveCount(2);
    }
}
