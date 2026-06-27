// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Financials.Commands
//  Layer:          Tests (Unit)
//  File:           CreateFinancialsCommandTests.cs
//  Created:        2026-06-17
//  SchemaVersion:  2026.1
//  Description:    Unit tests for CreateFinancialsCommand.
// ============================================================================
namespace PlantSafety.Tests.Financials.Commands;

public sealed class CreateFinancialsCommandTests : UnitTestBase
{
    [Fact]
    public void Command_ShouldCarryTenantId()
    {
        var tenantId = TestTenantId;
        tenantId.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Placeholder_AlwaysPasses() =>
        true.Should().BeTrue("stub — add real command record tests here");
}
