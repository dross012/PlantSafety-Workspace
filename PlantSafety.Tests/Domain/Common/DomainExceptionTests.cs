// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Domain.Common
//  Layer:          Tests (Unit)
//  File:           DomainExceptionTests.cs
//  Created:        2026-06-17
//  SchemaVersion:  2026.1
//  Description:    Unit tests for DomainException base class canonical fields.
// ============================================================================
namespace PlantSafety.Tests.Domain.Common;

public sealed class DomainExceptionTests : UnitTestBase
{
    private sealed class TestException(
        CorrelationId correlationId, CausationId causationId)
        : DomainException(
            "test", "TEST_001", "Domain", "Error",
            correlationId, causationId)
    { }

    [Fact]
    public void Constructor_ShouldPreserveMessage()
    {
        var ex = new TestException(TestCorrelationId, TestCausationId);
        ex.Message.Should().Contain("test");
    }

    [Fact]
    public void Constructor_ShouldPreserveErrorCode()
    {
        var ex = new TestException(TestCorrelationId, TestCausationId);
        ex.ErrorCode.Should().Be("TEST_001");
    }

    [Fact]
    public void Constructor_ShouldPreserveCorrelationId()
    {
        var ex = new TestException(TestCorrelationId, TestCausationId);
        ex.CorrelationId.Should().Be(TestCorrelationId);
    }

    [Fact]
    public void Constructor_ShouldPreserveCausationId()
    {
        var ex = new TestException(TestCorrelationId, TestCausationId);
        ex.CausationId.Should().Be(TestCausationId);
    }
}
