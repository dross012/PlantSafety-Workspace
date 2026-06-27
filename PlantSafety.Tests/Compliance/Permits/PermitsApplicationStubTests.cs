// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Compliance.Permits
//  Layer:          Tests (Unit)
//  File:           PermitsApplicationStubTests.cs
//  Created:        2026-06-17
//  SchemaVersion:  2026.1
//  Description:    Application-layer stub for Compliance.Permits (domain tests in Domain\Compliance\).
// ============================================================================
namespace PlantSafety.Tests.Compliance.Permits;

public sealed class PermitsApplicationStubTests : UnitTestBase
{
    [Fact]
    public void Placeholder_AlwaysPasses() =>
        true.Should().BeTrue("stub — expand with Permits handler, validator, and mapping tests");
}
