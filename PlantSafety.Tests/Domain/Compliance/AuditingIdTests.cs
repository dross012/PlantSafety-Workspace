// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Compliance.Auditing
//  Layer:          Tests (Unit)
//  File:           AuditingIdTests.cs
//  Created:        2026-06-17
//  SchemaVersion:  2026.1
//  Description:    Unit tests for AuditingId StrongId value object.
// ============================================================================
using PlantSafety.Domain.Compliance.Auditing.ValueObjects.Ids;

namespace PlantSafety.Tests.Compliance.Auditing;

public sealed class AuditingIdTests : UnitTestBase
{
    [Fact]
    public void New_ShouldReturnNonEmptyId()
    {
        var id = AuditingId.New();
        id.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Constructor_WithEmptyGuid_ShouldThrow() =>
        FluentActions.Invoking(() => AuditingId.FromGuid(Guid.Empty))
                     .Should().Throw<ArgumentException>();

    [Fact]
    public void TwoIds_WithSameGuid_ShouldBeEqual()
    {
        var g = Guid.NewGuid();
        AuditingId.FromGuid(g).Should().Be(AuditingId.FromGuid(g));
    }

    [Fact]
    public void TwoIds_WithDifferentGuids_ShouldNotBeEqual() =>
        AuditingId.New().Should().NotBe(AuditingId.New());
}
