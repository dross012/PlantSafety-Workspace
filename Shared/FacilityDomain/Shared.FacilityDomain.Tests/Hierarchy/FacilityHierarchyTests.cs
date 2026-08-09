// ============================================================================
//  Project:        Shared.FacilityDomain.Tests
//  File:           FacilityHierarchyTests.cs
//  Created:        2026-08-08
//  Description:    Phase C.5 unit tests for FacilityAggregate's hierarchy
//                  commands (AddBuilding/AddArea/AddZone/UpdateHierarchy)
//                  and FacilityHierarchyUpdatedEvent.
// ============================================================================
using FluentAssertions;

using Shared.FacilityDomain.Abstractions.Aggregates;
using Shared.FacilityDomain.Abstractions.Events;
using Shared.FacilityDomain.Abstractions.Hierarchy;
using Shared.FacilityDomain.Abstractions.ValueObjects;

using Xunit;

namespace Shared.FacilityDomain.Tests.Hierarchy;

public sealed class FacilityHierarchyTests
{
    private static readonly FacilityIndustry Industry = new("Chemical Manufacturing");
    private static readonly FacilityLocation Location = new("100 Main St", "Springfield", "IL", "62701", "US");

    private static FacilityAggregate CreateFacility() =>
        FacilityAggregate.CreateFacility(FacilityId.New(), "Plant 1", Industry, Location, DateTime.UtcNow);

    [Fact]
    public void CreateFacility_StartsWithEmptyHierarchy()
    {
        FacilityAggregate aggregate = CreateFacility();

        aggregate.State.Hierarchy.Buildings.Should().BeEmpty();
    }

    [Fact]
    public void AddBuilding_AppendsBuildingAndRaisesEvent()
    {
        FacilityAggregate aggregate = CreateFacility();
        var buildingId = BuildingId.New();

        aggregate.AddBuilding(buildingId, "Building A", DateTime.UtcNow);

        aggregate.State.Hierarchy.Buildings.Should().ContainSingle(b => b.BuildingId == buildingId && b.Name == "Building A");
        aggregate.DomainEvents.OfType<FacilityHierarchyUpdatedEvent>().Should().Contain(e => e.Kind == FacilityHierarchyChangeKind.BuildingAdded);
    }

    [Fact]
    public void AddBuilding_BlankName_Throws()
    {
        FacilityAggregate aggregate = CreateFacility();

        Action act = () => aggregate.AddBuilding(BuildingId.New(), " ", DateTime.UtcNow);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AddArea_AppendsAreaToCorrectBuilding()
    {
        FacilityAggregate aggregate = CreateFacility();
        var buildingId = BuildingId.New();
        aggregate.AddBuilding(buildingId, "Building A", DateTime.UtcNow);
        var areaId = AreaId.New();

        aggregate.AddArea(buildingId, areaId, "Area 1", DateTime.UtcNow);

        FacilityBuilding building = aggregate.State.Hierarchy.Buildings.Single(b => b.BuildingId == buildingId);
        building.Areas.Should().ContainSingle(a => a.AreaId == areaId && a.Name == "Area 1");
    }

    [Fact]
    public void AddArea_UnknownBuilding_Throws()
    {
        FacilityAggregate aggregate = CreateFacility();

        Action act = () => aggregate.AddArea(BuildingId.New(), AreaId.New(), "Area 1", DateTime.UtcNow);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void AddZone_AppendsZoneToCorrectArea()
    {
        FacilityAggregate aggregate = CreateFacility();
        var buildingId = BuildingId.New();
        aggregate.AddBuilding(buildingId, "Building A", DateTime.UtcNow);
        var areaId = AreaId.New();
        aggregate.AddArea(buildingId, areaId, "Area 1", DateTime.UtcNow);
        var zoneId = ZoneId.New();

        aggregate.AddZone(areaId, zoneId, "Zone 1", DateTime.UtcNow);

        FacilityArea area = aggregate.State.Hierarchy.Buildings.Single().Areas.Single(a => a.AreaId == areaId);
        area.Zones.Should().ContainSingle(z => z.ZoneId == zoneId && z.Name == "Zone 1");
    }

    [Fact]
    public void AddZone_UnknownArea_Throws()
    {
        FacilityAggregate aggregate = CreateFacility();

        Action act = () => aggregate.AddZone(AreaId.New(), ZoneId.New(), "Zone 1", DateTime.UtcNow);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void UpdateHierarchy_ReplacesEntireHierarchy()
    {
        FacilityAggregate aggregate = CreateFacility();
        aggregate.AddBuilding(BuildingId.New(), "Building A", DateTime.UtcNow);
        var replacement = new FacilityHierarchy([new FacilityBuilding(BuildingId.New(), "Building B", [])], DateTime.UtcNow);

        aggregate.UpdateHierarchy(replacement, DateTime.UtcNow);

        aggregate.State.Hierarchy.Buildings.Should().ContainSingle(b => b.Name == "Building B");
        aggregate.DomainEvents.OfType<FacilityHierarchyUpdatedEvent>().Should().Contain(e => e.Kind == FacilityHierarchyChangeKind.Replaced);
    }
}
