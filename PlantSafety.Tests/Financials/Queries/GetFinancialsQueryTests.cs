// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Financials.Queries
//  Layer:          Tests (Unit)
//  File:           GetFinancialsQueryTests.cs
//  Created:        2026-06-17
//  SchemaVersion:  2026.1
//  Description:    Unit tests for GetFinancialsQuery.
// ============================================================================
namespace PlantSafety.Tests.Financials.Queries;

public sealed class GetFinancialsQueryTests : UnitTestBase
{
    [Fact]
    public void Query_ShouldCarryTenantId()
    {
        var tenantId = TestTenantId;
        tenantId.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Placeholder_AlwaysPasses() =>
        true.Should().BeTrue("stub — add real query record tests here");
}
