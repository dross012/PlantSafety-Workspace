// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.MachineSafety.CraneHoistSystems.Interoperability
//  Layer:          Tests (Interoperability)
//  File:           CraneHoistSystemsInteroperabilityTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Interoperability tests verifying the CraneHoistSystems wire contracts
//                  (CraneHoistSystemsItemDto, CraneHoistSystemsActionResult,
//                  CraneHoistSystemsQueryResult) round-trip cleanly through System.Text.Json,
//                  the serializer used by the ASP.NET Core API surface every external client
//                  integrates against. Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using System.Text.Json;
using PlantSafety.Application.MachineSafety.Commands;
using PlantSafety.Application.MachineSafety.CraneHoistSystems.Models;
using PlantSafety.Application.MachineSafety.CraneHoistSystems.Queries;

namespace PlantSafety.Tests.MachineSafety.CraneHoistSystems.Interoperability;

public sealed class CraneHoistSystemsInteroperabilityTests : UnitTestBase
{
    [Fact]
    public void CraneHoistSystemsItemDto_RoundTripsThroughJson()
    {
        var dto = new CraneHoistSystemsItemDto
        {
            CraneHoistSystemsId = Guid.NewGuid(),
            TenantId = TestTenantId.Value,
            SiteId = Guid.NewGuid(),
            AssetTag = "IOP-1",
            AssetTitle = "Interop Crane",
            AssetType = "OverheadBridgeCrane",
            RatedLoadCapacityKg = 5000.50m,
            LastObservedLoadKg = 1200.25m,
            OverloadEventCount = 2,
            InspectionDueAtUtc = FixedUtcNow,
            OperatorQualificationExpiresAtUtc = FixedUtcNow.AddYears(1),
            SystemStatus = "InService",
            CreatedAtUtc = FixedUtcNow,
            CreatedBy = TestUserId.Value,
        };

        var json = JsonSerializer.Serialize(dto);
        var roundTripped = JsonSerializer.Deserialize<CraneHoistSystemsItemDto>(json);

        roundTripped.Should().BeEquivalentTo(dto);
    }

    [Fact]
    public void CraneHoistSystemsActionResult_RoundTripsThroughJson()
    {
        var result = new CraneHoistSystemsActionResult(Guid.NewGuid(), "InspectionDue");

        var json = JsonSerializer.Serialize(result);
        var roundTripped = JsonSerializer.Deserialize<CraneHoistSystemsActionResult>(json);

        roundTripped.Should().BeEquivalentTo(result);
    }

    [Fact]
    public void CraneHoistSystemsQueryResult_RoundTripsThroughJson()
    {
        var result = new CraneHoistSystemsQueryResult(
            TestTenantId.Value, null, FixedUtcNow,
            [new CraneHoistSystemsItemDto
            {
                CraneHoistSystemsId = Guid.NewGuid(),
                TenantId = TestTenantId.Value,
                AssetTag = "IOP-2",
                AssetTitle = "Interop Crane Two",
                AssetType = "Hoist",
                RatedLoadCapacityKg = 1000m,
                SystemStatus = "Registered",
                CreatedAtUtc = FixedUtcNow,
                CreatedBy = TestUserId.Value,
            }]);

        var json = JsonSerializer.Serialize(result);
        var roundTripped = JsonSerializer.Deserialize<CraneHoistSystemsQueryResult>(json);

        roundTripped.Should().BeEquivalentTo(result);
    }

    [Fact]
    public void CraneHoistSystemsItemDto_Json_UsesPascalCasePropertyNamesByDefault()
    {
        var dto = new CraneHoistSystemsItemDto
        {
            CraneHoistSystemsId = Guid.NewGuid(),
            TenantId = TestTenantId.Value,
            AssetTag = "IOP-3",
            AssetTitle = "Interop Crane Three",
            AssetType = "JibCrane",
            RatedLoadCapacityKg = 500m,
            SystemStatus = "Registered",
            CreatedAtUtc = FixedUtcNow,
            CreatedBy = TestUserId.Value,
        };

        var json = JsonSerializer.Serialize(dto);

        json.Should().Contain("\"CraneHoistSystemsId\"");
        json.Should().Contain("\"AssetTag\"");
    }
}
