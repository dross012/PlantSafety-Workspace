// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.OperationalSafety.RiskManagement
//  Layer:          Tests (Unit)
//  File:           RiskManagementIdTests.cs
//  Created:        2026-06-17
//  SchemaVersion:  2026.1
//  Description:    Unit tests for RiskManagementId StrongId value object.
// ============================================================================
using PlantSafety.Domain.Operational.RiskManagement.ValueObjects.Ids;

namespace PlantSafety.Tests.OperationalSafety.RiskManagement;

public sealed class RiskManagementIdTests : UnitTestBase
{
    [Fact]
    public void New_ShouldReturnNonEmptyId()
    {
        var id = RiskManagementId.New();
        id.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Constructor_WithEmptyGuid_ShouldThrow() =>
        FluentActions.Invoking(() => RiskManagementId.FromGuid(Guid.Empty))
                     .Should().Throw<ArgumentException>();

    [Fact]
    public void TwoIds_WithSameGuid_ShouldBeEqual()
    {
        var g = Guid.NewGuid();
        RiskManagementId.FromGuid(g).Should().Be(RiskManagementId.FromGuid(g));
    }

    [Fact]
    public void TwoIds_DifferentInstances_ShouldNotBeEqual() =>
        RiskManagementId.New().Should().NotBe(RiskManagementId.New());
}
