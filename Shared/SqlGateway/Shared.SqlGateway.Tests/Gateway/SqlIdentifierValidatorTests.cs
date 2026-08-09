// ============================================================================
//  Project:        Shared.SqlGateway.Tests
//  File:           SqlIdentifierValidatorTests.cs
//  Created:        2026-08-09
// ============================================================================
using AwesomeAssertions;

using Shared.SqlGateway.Abstractions.Gateway;

using Xunit;

namespace Shared.SqlGateway.Tests.Gateway;

public sealed class SqlIdentifierValidatorTests
{
    [Theory]
    [InlineData("dbo.usp_GetHazardSummary")]
    [InlineData("usp_GetSafetyCompliance")]
    [InlineData("RiskMgmt.usp_GetRisk")]
    public void IsValidStoredProcedureName_PlausibleIdentifiers_ReturnTrue(string name)
    {
        SqlIdentifierValidator.IsValidStoredProcedureName(name).Should().BeTrue();
    }

    [Theory]
    [InlineData("SELECT * FROM dbo.Risk")]
    [InlineData("dbo.usp_Get; DROP TABLE dbo.Risk;--")]
    [InlineData("dbo.usp_Get' OR '1'='1")]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("dbo.usp with spaces")]
    [InlineData("a.b.c")]
    public void IsValidStoredProcedureName_SqlTextOrInvalidInput_ReturnsFalse(string? name)
    {
        SqlIdentifierValidator.IsValidStoredProcedureName(name).Should().BeFalse();
    }

    [Fact]
    public void EnsureValid_InvalidName_ThrowsArgumentException()
    {
        Action act = () => SqlIdentifierValidator.EnsureValid("SELECT * FROM dbo.Risk");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void EnsureValid_ValidName_DoesNotThrow()
    {
        Action act = () => SqlIdentifierValidator.EnsureValid("dbo.usp_GetHazardSummary");

        act.Should().NotThrow();
    }
}
