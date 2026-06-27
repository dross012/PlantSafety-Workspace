// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Analytics.Culture.Commands
//  Layer:          Tests (Unit)
//  File:           CreateCultureCommandTests.cs
//  Created:        2026-06-17
//  SchemaVersion:  2026.1
//  Description:    Unit tests for CreateCultureCommand record validity.
// ============================================================================
namespace PlantSafety.Tests.Analytics.Culture.Commands;

public sealed class CreateCultureCommandTests : UnitTestBase
{
    [Fact]
    public void Ctor_WithValidArgs_ShouldCreateCommand()
    {
        var cmd = new CreateCultureCommand(TestTenantId, TestUserId, TestCorrelationId, TestCausationId, TestActor);
        cmd.Should().NotBeNull();
    }

    [Fact]
    public void Ctor_ShouldPreserveTenantId()
    {
        var cmd = new CreateCultureCommand(TestTenantId, TestUserId, TestCorrelationId, TestCausationId, TestActor);
        cmd.TenantId.Should().Be(TestTenantId);
    }

    [Fact]
    public void Ctor_ShouldPreserveActor()
    {
        var cmd = new CreateCultureCommand(TestTenantId, TestUserId, TestCorrelationId, TestCausationId, TestActor);
        cmd.Actor.Should().Be(TestActor);
    }
}
