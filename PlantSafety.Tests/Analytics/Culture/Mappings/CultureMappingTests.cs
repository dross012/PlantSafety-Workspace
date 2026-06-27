// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Analytics.Culture.Mappings
//  Layer:          Tests (Unit)
//  File:           CultureMappingTests.cs
//  Created:        2026-06-17
//  SchemaVersion:  2026.1
//  Description:    Unit tests for Culture aggregate mapping/projection helpers.
// ============================================================================
using PlantSafety.Application.Analytics.Culture.Mappings;

namespace PlantSafety.Tests.Analytics.Culture.Mappings;

public sealed class CultureMappingTests : UnitTestBase
{
    [Fact]
    public void MapToResponse_ShouldPreserveCultureId()
    {
        var agg = Culture.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor);
        var response = CultureMappings.ToResponse(agg);
        response.CultureId.Should().Be(agg.CultureId.Value);
    }

    [Fact]
    public void MapToResponse_ShouldPreserveTenantId()
    {
        var agg = Culture.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor);
        var response = CultureMappings.ToResponse(agg);
        response.Should().NotBeNull();
    }
}
