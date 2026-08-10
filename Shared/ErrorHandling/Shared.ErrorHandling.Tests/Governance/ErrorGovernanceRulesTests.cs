// ============================================================================
//  Project:        Shared.ErrorHandling.Tests
//  File:           ErrorGovernanceRulesTests.cs
//  Created:        2026-08-10
// ============================================================================
using AwesomeAssertions;

using Shared.ErrorHandling.Abstractions.Envelope;
using Shared.ErrorHandling.Infrastructure.Governance;

using Xunit;

namespace Shared.ErrorHandling.Tests.Governance;

public sealed class ErrorGovernanceRulesTests
{
    private static readonly DateTime Now = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Validate_Null_ReturnsMissingErrorEnvelopeViolation()
    {
        ErrorGovernanceRules.Validate(null).Should().Contain("MissingErrorEnvelope");
    }

    [Fact]
    public void Validate_ValidEnvelope_ReturnsNoViolations()
    {
        var envelope = new ErrorEnvelope("WORKFLOW-VALIDATION", ErrorCategory.Validation, "user msg", "dev msg", "corr-1", "WorkflowEngine", Now);

        ErrorGovernanceRules.Validate(envelope).Should().BeEmpty();
    }

    [Fact]
    public void Validate_EmptyErrorCode_ReturnsMissingErrorCodeViolation()
    {
        var envelope = new ErrorEnvelope("", ErrorCategory.Validation, "user msg", "dev msg", "corr-1", "WorkflowEngine", Now);

        ErrorGovernanceRules.Validate(envelope).Should().Contain("MissingErrorCode");
    }

    [Fact]
    public void Validate_EmptyCorrelationId_ReturnsMissingCorrelationIdViolation()
    {
        var envelope = new ErrorEnvelope("WORKFLOW-VALIDATION", ErrorCategory.Validation, "user msg", "dev msg", "", "WorkflowEngine", Now);

        ErrorGovernanceRules.Validate(envelope).Should().Contain("MissingCorrelationId");
    }

    [Fact]
    public void Validate_EmptyModule_ReturnsMissingModuleViolation()
    {
        var envelope = new ErrorEnvelope("WORKFLOW-VALIDATION", ErrorCategory.Validation, "user msg", "dev msg", "corr-1", "", Now);

        ErrorGovernanceRules.Validate(envelope).Should().Contain("MissingModule");
    }

    [Fact]
    public void Validate_DefaultTimestamp_ReturnsInvalidTimestampViolation()
    {
        var envelope = new ErrorEnvelope("WORKFLOW-VALIDATION", ErrorCategory.Validation, "user msg", "dev msg", "corr-1", "WorkflowEngine", default);

        ErrorGovernanceRules.Validate(envelope).Should().Contain("InvalidTimestamp");
    }

    [Fact]
    public void Validate_UndefinedCategory_ReturnsUndefinedErrorCategoryViolation()
    {
        var envelope = new ErrorEnvelope("WORKFLOW-VALIDATION", (ErrorCategory)999, "user msg", "dev msg", "corr-1", "WorkflowEngine", Now);

        ErrorGovernanceRules.Validate(envelope).Should().Contain("UndefinedErrorCategory");
    }
}
