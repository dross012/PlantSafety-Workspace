// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Integrations.Import
//  Layer:          Tests (Unit)
//  File:           ImportStubTests.cs
//  Created:        2026-06-17
//  SchemaVersion:  2026.1
//  Description:    Stub unit tests for Integrations.Import.
// ============================================================================
namespace PlantSafety.Tests.Integrations.Import;

public sealed class ImportStubTests : UnitTestBase
{
    [Fact]
    public void Placeholder_AlwaysPasses() =>
        true.Should().BeTrue("stub — expand with Import unit tests");
}
