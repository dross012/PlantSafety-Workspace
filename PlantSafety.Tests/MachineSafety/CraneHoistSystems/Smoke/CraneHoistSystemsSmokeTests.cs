// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.MachineSafety.CraneHoistSystems.Smoke
//  Layer:          Tests (Smoke)
//  File:           CraneHoistSystemsSmokeTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Fast sanity checks that the CraneHoistSystems module's core building blocks
//                  (aggregate creation, DTO shape, command/result wiring) are minimally wired up
//                  correctly. Intended as a first-line-of-defense gate, not exhaustive coverage.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.MachineSafety.Commands;
using PlantSafety.Application.MachineSafety.CraneHoistSystems.Models;
using PlantSafety.Domain.MachineSafety.CraneHoistSystems.Enums;
using Agg = PlantSafety.Domain.MachineSafety.CraneHoistSystems.Aggregates.CraneHoistSystems;

namespace PlantSafety.Tests.MachineSafety.CraneHoistSystems.Smoke;

public sealed class CraneHoistSystemsSmokeTests : UnitTestBase
{
    [Fact]
    public void Smoke_AggregateCanBeCreated()
    {
        var asset = Agg.Create(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            TestTenantId.Value, null, "SMOKE-1", "Smoke Crane", CraneHoistAssetType.Hoist, 1000m);

        asset.Should().NotBeNull();
        asset.CraneHoistSystemsId.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Smoke_ActionResultRoundTripsCoreFields()
    {
        var id = Guid.NewGuid();
        var result = new CraneHoistSystemsActionResult(id, "Registered");

        result.CraneHoistSystemsId.Should().Be(id);
        result.Status.Should().Be("Registered");
    }

    [Fact]
    public void Smoke_ItemDtoRequiredFieldsCanBePopulated()
    {
        var dto = new CraneHoistSystemsItemDto
        {
            CraneHoistSystemsId = Guid.NewGuid(),
            TenantId = TestTenantId.Value,
            AssetTag = "SMOKE-2",
            AssetTitle = "Smoke Crane Two",
            AssetType = "MobileCrane",
            RatedLoadCapacityKg = 4000m,
            SystemStatus = "Registered",
            CreatedAtUtc = FixedUtcNow,
            CreatedBy = TestUserId.Value,
        };

        dto.AssetTag.Should().Be("SMOKE-2");
        dto.RatedLoadCapacityKg.Should().Be(4000m);
    }

    [Theory]
    [InlineData(CraneHoistSystemsStatus.Registered)]
    [InlineData(CraneHoistSystemsStatus.InService)]
    [InlineData(CraneHoistSystemsStatus.InspectionDue)]
    [InlineData(CraneHoistSystemsStatus.InspectionOverdue)]
    [InlineData(CraneHoistSystemsStatus.Suspended)]
    [InlineData(CraneHoistSystemsStatus.Retired)]
    public void Smoke_AllStatusValuesAreDefined(CraneHoistSystemsStatus status)
    {
        Enum.IsDefined(status).Should().BeTrue();
    }
}
