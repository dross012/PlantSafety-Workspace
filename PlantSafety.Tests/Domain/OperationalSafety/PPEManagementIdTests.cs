// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.OperationalSafety.PPEManagement
//  Layer:          Tests (Unit)
//  File:           PPEManagementIdTests.cs
//  Created:        2026-06-17
//  SchemaVersion:  2026.1
//  Description:    Unit tests for PPEManagementId StrongId value object.
// ============================================================================
using PlantSafety.Domain.Operational.PPEManagement.ValueObjects.Ids;

namespace PlantSafety.Tests.OperationalSafety.PPEManagement;

public sealed class PPEManagementIdTests : UnitTestBase
{
    [Fact]
    public void New_ShouldReturnNonEmptyId()
    {
        var id = PPEManagementId.New();
        id.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Constructor_WithEmptyGuid_ShouldThrow() =>
        FluentActions.Invoking(() => PPEManagementId.FromGuid(Guid.Empty))
                     .Should().Throw<ArgumentException>();

    [Fact]
    public void TwoIds_WithSameGuid_ShouldBeEqual()
    {
        var g = Guid.NewGuid();
        PPEManagementId.FromGuid(g).Should().Be(PPEManagementId.FromGuid(g));
    }

    [Fact]
    public void TwoIds_DifferentInstances_ShouldNotBeEqual() =>
        PPEManagementId.New().Should().NotBe(PPEManagementId.New());
}
