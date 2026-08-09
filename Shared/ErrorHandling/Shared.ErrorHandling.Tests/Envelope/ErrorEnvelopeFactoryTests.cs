// ============================================================================
//  Project:        Shared.ErrorHandling.Tests
//  File:           ErrorEnvelopeFactoryTests.cs
//  Created:        2026-08-09
// ============================================================================
using AwesomeAssertions;

using Shared.ErrorHandling.Abstractions.Envelope;

using Xunit;

namespace Shared.ErrorHandling.Tests.Envelope;

public sealed class ErrorEnvelopeFactoryTests
{
    private static readonly DateTime OccurredAtUtc = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void FromException_ArgumentException_ClassifiesAsValidation()
    {
        ErrorEnvelope envelope = ErrorEnvelopeFactory.FromException(new ArgumentException("bad arg"), "Facility", "corr-1", OccurredAtUtc);

        envelope.Category.Should().Be(ErrorCategory.Validation);
        envelope.ErrorCode.Should().Be("FACILITY-VALIDATION");
    }

    [Fact]
    public void FromException_KeyNotFoundException_ClassifiesAsNotFound()
    {
        ErrorEnvelope envelope = ErrorEnvelopeFactory.FromException(new KeyNotFoundException(), "Machine", "corr-2", OccurredAtUtc);

        envelope.Category.Should().Be(ErrorCategory.NotFound);
    }

    [Fact]
    public void FromException_InvalidOperationException_ClassifiesAsConflict()
    {
        ErrorEnvelope envelope = ErrorEnvelopeFactory.FromException(new InvalidOperationException(), "WorkflowEngine", "corr-3", OccurredAtUtc);

        envelope.Category.Should().Be(ErrorCategory.Conflict);
    }

    [Fact]
    public void FromException_UnknownExceptionType_ClassifiesAsInternal()
    {
        ErrorEnvelope envelope = ErrorEnvelopeFactory.FromException(new Exception("mystery"), "Employee", "corr-4", OccurredAtUtc);

        envelope.Category.Should().Be(ErrorCategory.Internal);
    }

    [Fact]
    public void FromException_UserMessage_NeverContainsTheRawExceptionMessage()
    {
        var exception = new InvalidOperationException("internal stack detail that should never reach the user");

        ErrorEnvelope envelope = ErrorEnvelopeFactory.FromException(exception, "Facility", "corr-5", OccurredAtUtc);

        envelope.UserMessage.Should().NotContain("internal stack detail");
        envelope.DeveloperMessage.Should().Contain("internal stack detail");
    }

    [Fact]
    public void FromException_PreservesCorrelationIdModuleAndTimestamp()
    {
        ErrorEnvelope envelope = ErrorEnvelopeFactory.FromException(new Exception(), "AI.SeverityAssessment", "corr-6", OccurredAtUtc);

        envelope.CorrelationId.Should().Be("corr-6");
        envelope.Module.Should().Be("AI.SeverityAssessment");
        envelope.OccurredAtUtc.Should().Be(OccurredAtUtc);
    }
}
