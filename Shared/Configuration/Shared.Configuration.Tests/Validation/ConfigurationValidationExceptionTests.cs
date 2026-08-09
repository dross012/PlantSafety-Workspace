// ============================================================================
//  Project:        Shared.Configuration.Tests
//  File:           ConfigurationValidationExceptionTests.cs
//  Created:        2026-08-09
// ============================================================================
using AwesomeAssertions;

using Shared.Configuration.Abstractions.Validation;

using Xunit;

namespace Shared.Configuration.Tests.Validation;

public sealed class ConfigurationValidationExceptionTests
{
    [Fact]
    public void Message_ListsEveryError()
    {
        var exception = new ConfigurationValidationException(["Environment:Name is missing.", "RuntimeLimits are invalid."]);

        exception.Message.Should().Contain("Environment:Name is missing.");
        exception.Message.Should().Contain("RuntimeLimits are invalid.");
        exception.Errors.Should().HaveCount(2);
    }
}
