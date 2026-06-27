// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Common.TestBase
//  Layer:          Tests (Unit)
//  File:           UnitTestBaseTests.cs
//  Created:        2026-06-17
//  SchemaVersion:  2026.1
//  Description:    Sanity-checks on UnitTestBase static test fixtures.
// ============================================================================
namespace PlantSafety.Tests.Common.TestBase;

public sealed class UnitTestBaseTests : UnitTestBase
{
    [Fact]
    public void FixedUtcNow_ShouldBeUtcKind() =>
        FixedUtcNow.Kind.Should().Be(DateTimeKind.Utc);

    [Fact]
    public void FixedTimestamp_ShouldWrapFixedUtcNow() =>
        FixedTimestamp.Value.Should().Be(FixedUtcNow);

    [Fact]
    public void TestTenantId_ShouldNotBeEmpty() =>
        TestTenantId.Value.Should().NotBe(Guid.Empty);

    [Fact]
    public void TestUserId_ShouldNotBeEmpty() =>
        TestUserId.Value.Should().NotBe(Guid.Empty);

    [Fact]
    public void TestCorrelationId_ShouldNotBeEmpty() =>
        TestCorrelationId.Value.Should().NotBe(Guid.Empty);

    [Fact]
    public void TestCausationId_ShouldNotBeEmpty() =>
        TestCausationId.Value.Should().NotBe(Guid.Empty);

    [Fact]
    public void TestActor_ShouldBeUserKind() =>
        TestActor.Kind.Should().Be(ActorKind.User);

    [Fact]
    public void NullLogger_ShouldReturnNonNullInstance() =>
        NullLogger<UnitTestBaseTests>().Should().NotBeNull();
}
