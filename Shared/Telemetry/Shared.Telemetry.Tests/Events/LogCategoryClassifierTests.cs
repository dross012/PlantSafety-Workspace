// ============================================================================
//  Project:        Shared.Telemetry.Tests
//  File:           LogCategoryClassifierTests.cs
//  Created:        2026-08-09
// ============================================================================
using AwesomeAssertions;

using Shared.Telemetry.Abstractions.Events;

using Xunit;

namespace Shared.Telemetry.Tests.Events;

public sealed class LogCategoryClassifierTests
{
    [Theory]
    [InlineData("facilities", LogCategory.Facility)]
    [InlineData("employees", LogCategory.Employee)]
    [InlineData("machines", LogCategory.Machine)]
    [InlineData("workflow-engine", LogCategory.WorkflowEngine)]
    [InlineData("severity-assessment", LogCategory.AiEngine)]
    [InlineData("safety-copilot", LogCategory.AiEngine)]
    [InlineData("datalake", LogCategory.EnterpriseEngine)]
    [InlineData("digital-twin", LogCategory.EnterpriseEngine)]
    [InlineData("unknown-route-segment", LogCategory.Infrastructure)]
    public void Classify_MapsModuleNameToExpectedCategory(string module, LogCategory expected)
    {
        LogCategoryClassifier.Classify(module).Should().Be(expected);
    }

    [Fact]
    public void Classify_IsCaseInsensitive()
    {
        LogCategoryClassifier.Classify("FACILITIES").Should().Be(LogCategory.Facility);
    }
}
