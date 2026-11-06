// ============================================================================
//  Project:        Shared.Authorization.Tests
//  File:           UnifiedEventGovernanceTests.cs
//  Created:        2026-08-10
// ============================================================================
using AwesomeAssertions;

using Shared.Authorization.Abstractions.Caller;
using Shared.Authorization.Abstractions.Contracts;
using Shared.Authorization.Infrastructure.Events;
using Shared.Telemetry.Abstractions.Events;

using Xunit;

namespace Shared.Authorization.Tests.Events;

public sealed class UnifiedEventGovernanceTests
{
    private static readonly DateTime Now = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private static readonly ContractMetadata Metadata = new("Safety.Editor", "corr-1", MfaStatus.NotApplicable, "acme-plant-1", Now);

    private static LogEvent MakeEvent(string correlationId) => new(
        EventId: "TEST_EVENT",
        Category: LogCategory.Infrastructure,
        Severity: LogSeverity.Info,
        Message: "test",
        Payload: new Dictionary<string, object?>(),
        CorrelationId: correlationId,
        Module: "Test",
        OccurredAtUtc: Now);

    [Fact]
    public void ValidateCorrelationConsistency_Null_ReturnsMissingLogEventViolation()
    {
        UnifiedEventGovernance.ValidateCorrelationConsistency(null, Metadata).Should().Contain("MissingLogEvent");
    }

    [Fact]
    public void ValidateCorrelationConsistency_MatchingCorrelationId_ReturnsNoViolations()
    {
        UnifiedEventGovernance.ValidateCorrelationConsistency(MakeEvent("corr-1"), Metadata).Should().BeEmpty();
    }

    [Fact]
    public void ValidateCorrelationConsistency_MismatchedCorrelationId_ReturnsViolation()
    {
        UnifiedEventGovernance.ValidateCorrelationConsistency(MakeEvent("corr-other"), Metadata).Should().Contain("EventCorrelationIdMismatch");
    }
}
