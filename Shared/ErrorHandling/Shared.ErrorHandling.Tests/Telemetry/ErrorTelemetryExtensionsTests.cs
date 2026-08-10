// ============================================================================
//  Project:        Shared.ErrorHandling.Tests
//  File:           ErrorTelemetryExtensionsTests.cs
//  Created:        2026-08-10
// ============================================================================
using AwesomeAssertions;

using Shared.ErrorHandling.Abstractions.Envelope;
using Shared.ErrorHandling.Infrastructure.Telemetry;

using Xunit;

namespace Shared.ErrorHandling.Tests.Telemetry;

public sealed class ErrorTelemetryExtensionsTests
{
    private static readonly DateTime Now = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void ToLogPayload_ReturnsKeysInAlphabeticalOrder()
    {
        var envelope = new ErrorEnvelope("WORKFLOW-VALIDATION", ErrorCategory.Validation, "user msg", "dev msg", "corr-1", "WorkflowEngine", Now);

        IReadOnlyDictionary<string, object?> payload = envelope.ToLogPayload();

        payload.Keys.Should().Equal("Category", "CorrelationId", "ErrorCode", "Module", "OccurredAtUtc");
    }
}
