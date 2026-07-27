// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Compliance.Certification
//  Layer:          Tests (Unit)
//  File:           CertificationIdTests.cs
//  Created:        2026-06-17
//  SchemaVersion:  2026.1
//  Description:    Unit tests for CertificationId StrongId value object.
// ============================================================================
using PlantSafety.Domain.Compliance.Certification.ValueObjects.Ids;

namespace PlantSafety.Tests.Compliance.Certification;

public sealed class CertificationIdTests : UnitTestBase
{
    [Fact]
    public void New_ShouldReturnNonEmptyId()
    {
        var id = CertificationId.New();
        id.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Constructor_WithEmptyGuid_ShouldThrow() =>
        FluentActions.Invoking(() => CertificationId.FromGuid(Guid.Empty))
                     .Should().Throw<ArgumentException>();

    [Fact]
    public void TwoIds_WithSameGuid_ShouldBeEqual()
    {
        var g = Guid.NewGuid();
        CertificationId.FromGuid(g).Should().Be(CertificationId.FromGuid(g));
    }

    [Fact]
    public void TwoIds_WithDifferentGuids_ShouldNotBeEqual() =>
        CertificationId.New().Should().NotBe(CertificationId.New());
}
