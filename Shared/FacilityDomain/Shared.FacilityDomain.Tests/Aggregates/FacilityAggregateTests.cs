// ============================================================================
//  Project:        Shared.FacilityDomain.Tests
//  File:           FacilityAggregateTests.cs
//  Created:        2026-08-08
//  Description:    Phase B.1 unit tests for FacilityAggregate: command
//                  correctness, immutable state replacement, and domain
//                  event collection/clearing.
// ============================================================================
using FluentAssertions;

using Shared.FacilityDomain.Abstractions.Aggregates;
using Shared.FacilityDomain.Abstractions.Events;
using Shared.FacilityDomain.Abstractions.Hazards;
using Shared.FacilityDomain.Abstractions.ValueObjects;

using Xunit;

namespace Shared.FacilityDomain.Tests.Aggregates;

public sealed class FacilityAggregateTests
{
    private static readonly FacilityIndustry Industry = new("Chemical Manufacturing");
    private static readonly FacilityLocation Location = new("100 Main St", "Springfield", "IL", "62701", "US");

    [Fact]
    public void CreateFacility_SetsInitialStateAndRaisesFacilityCreatedEvent()
    {
        var facilityId = FacilityId.New();
        var now = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        FacilityAggregate aggregate = FacilityAggregate.CreateFacility(facilityId, "Plant 1", Industry, Location, now);

        aggregate.State.FacilityId.Should().Be(facilityId);
        aggregate.State.Name.Should().Be("Plant 1");
        aggregate.State.HazardProfile.ActiveHazardTypes.Should().BeEmpty();
        aggregate.State.RegulatoryProfile.RegulatorCodes.Should().BeEmpty();
        aggregate.State.ProgramProfile.RequiredProgramCodes.Should().BeEmpty();
        aggregate.State.CreatedAtUtc.Should().Be(now);
        aggregate.State.UpdatedAtUtc.Should().Be(now);

        aggregate.DomainEvents.Should().ContainSingle();
        aggregate.DomainEvents[0].Should().BeOfType<FacilityCreatedEvent>()
            .Which.FacilityId.Should().Be(facilityId);
    }

    [Fact]
    public void CreateFacility_BlankName_Throws()
    {
        Action act = () => FacilityAggregate.CreateFacility(FacilityId.New(), "   ", Industry, Location, DateTime.UtcNow);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void UpdateFacilityMetadata_ReplacesStateAndRaisesUpdatedEvent()
    {
        FacilityAggregate aggregate = FacilityAggregate.CreateFacility(FacilityId.New(), "Plant 1", Industry, Location, DateTime.UtcNow);
        var newIndustry = new FacilityIndustry("Marine Terminals");
        var updatedAt = DateTime.UtcNow.AddDays(1);

        aggregate.UpdateFacilityMetadata("Plant 1 Renamed", newIndustry, Location, updatedAt);

        aggregate.State.Name.Should().Be("Plant 1 Renamed");
        aggregate.State.Industry.Should().Be(newIndustry);
        aggregate.State.UpdatedAtUtc.Should().Be(updatedAt);

        aggregate.DomainEvents.Should().HaveCount(2);
        aggregate.DomainEvents[1].Should().BeOfType<FacilityUpdatedEvent>()
            .Which.UpdateKind.Should().Be(FacilityUpdateKind.Metadata);
    }

    [Fact]
    public void UpdateHazardProfile_ReplacesActiveHazardTypes()
    {
        FacilityAggregate aggregate = FacilityAggregate.CreateFacility(FacilityId.New(), "Plant 1", Industry, Location, DateTime.UtcNow);

        aggregate.UpdateHazardProfile([HazardType.Fire, HazardType.ChemicalSpill], DateTime.UtcNow);

        aggregate.State.HazardProfile.ActiveHazardTypes.Should().BeEquivalentTo([HazardType.Fire, HazardType.ChemicalSpill]);
        aggregate.DomainEvents.OfType<FacilityUpdatedEvent>().Should().Contain(e => e.UpdateKind == FacilityUpdateKind.HazardProfile);
    }

    [Fact]
    public void UpdateRegulatoryProfile_ReplacesRegulatorCodes()
    {
        FacilityAggregate aggregate = FacilityAggregate.CreateFacility(FacilityId.New(), "Plant 1", Industry, Location, DateTime.UtcNow);

        aggregate.UpdateRegulatoryProfile(["OSHA", "EPA"], DateTime.UtcNow);

        aggregate.State.RegulatoryProfile.RegulatorCodes.Should().BeEquivalentTo(["OSHA", "EPA"]);
        aggregate.DomainEvents.OfType<FacilityUpdatedEvent>().Should().Contain(e => e.UpdateKind == FacilityUpdateKind.RegulatoryProfile);
    }

    [Fact]
    public void UpdateProgramProfile_ReplacesRequiredProgramCodes()
    {
        FacilityAggregate aggregate = FacilityAggregate.CreateFacility(FacilityId.New(), "Plant 1", Industry, Location, DateTime.UtcNow);

        aggregate.UpdateProgramProfile(["HAZWOPER", "LOTO"], DateTime.UtcNow);

        aggregate.State.ProgramProfile.RequiredProgramCodes.Should().BeEquivalentTo(["HAZWOPER", "LOTO"]);
        aggregate.DomainEvents.OfType<FacilityUpdatedEvent>().Should().Contain(e => e.UpdateKind == FacilityUpdateKind.ProgramProfile);
    }

    [Fact]
    public void ClearDomainEvents_EmptiesTheList()
    {
        FacilityAggregate aggregate = FacilityAggregate.CreateFacility(FacilityId.New(), "Plant 1", Industry, Location, DateTime.UtcNow);
        aggregate.DomainEvents.Should().NotBeEmpty();

        aggregate.ClearDomainEvents();

        aggregate.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void Rehydrate_StartsWithNoDomainEvents()
    {
        FacilityAggregate original = FacilityAggregate.CreateFacility(FacilityId.New(), "Plant 1", Industry, Location, DateTime.UtcNow);

        FacilityAggregate rehydrated = FacilityAggregate.Rehydrate(original.State);

        rehydrated.State.Should().Be(original.State);
        rehydrated.DomainEvents.Should().BeEmpty();
    }
}
