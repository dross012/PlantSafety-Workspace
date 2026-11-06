// ============================================================================
//  Project:        Shared.Scheduling.Tests
//  File:           SchedulingGovernanceEvaluatorTests.cs
//  Created:        2026-08-10
// ============================================================================
using FluentAssertions;

using Shared.Scheduling.Abstractions.Contracts;
using Shared.Scheduling.Abstractions.DueTime;
using Shared.Scheduling.Abstractions.Rules;
using Shared.Scheduling.Infrastructure.Governance;
using Shared.Telemetry.Abstractions.Events;

using Xunit;

namespace Shared.Scheduling.Tests.Governance;

public sealed class SchedulingGovernanceEvaluatorTests
{
    private sealed class CapturingLogEventWriter : ILogEventWriter
    {
        public ILogEvent? LastEvent { get; private set; }
        public void Write(ILogEvent logEvent) => LastEvent = logEvent;
    }

    private static readonly DateTime OccurredAtUtc = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void EvaluateContractMetadataAndReport_ValidMetadata_ReturnsEmptyAndDoesNotWrite()
    {
        var writer = new CapturingLogEventWriter();
        var metadata = new SchedulingContractMetadata(SchedulingContractMetadata.CurrentSchemaVersion, "acme-plant-1", "corr-1", OccurredAtUtc);

        IReadOnlyList<string> violations = SchedulingGovernanceEvaluator.EvaluateContractMetadataAndReport(metadata, OccurredAtUtc, writer);

        violations.Should().BeEmpty();
        writer.LastEvent.Should().BeNull();
    }

    [Fact]
    public void EvaluateContractMetadataAndReport_InvalidMetadata_WritesGovernanceViolationEvent()
    {
        var writer = new CapturingLogEventWriter();
        var metadata = new SchedulingContractMetadata(SchedulingContractMetadata.CurrentSchemaVersion, string.Empty, "corr-2", OccurredAtUtc);

        IReadOnlyList<string> violations = SchedulingGovernanceEvaluator.EvaluateContractMetadataAndReport(metadata, OccurredAtUtc, writer);

        violations.Should().NotBeEmpty();
        writer.LastEvent.Should().NotBeNull();
        writer.LastEvent!.EventId.Should().Be("SCHEDULING_GOVERNANCE_VIOLATION");
        writer.LastEvent.Severity.Should().Be(LogSeverity.Warning);
    }

    [Fact]
    public void EvaluateDueTimesAndReport_InvalidDueTime_WritesGovernanceViolationEvent()
    {
        var writer = new CapturingLogEventWriter();
        var dueTimes = new[] { new ScheduledDueTime(string.Empty, "Description", OccurredAtUtc, IsOverdue: false) };

        IReadOnlyList<string> violations = SchedulingGovernanceEvaluator.EvaluateDueTimesAndReport(dueTimes, "corr-3", OccurredAtUtc, writer);

        violations.Should().NotBeEmpty();
        writer.LastEvent!.EventId.Should().Be("SCHEDULING_GOVERNANCE_VIOLATION");
    }

    [Fact]
    public void EvaluateRuleDecisionAndReport_ValidDecision_ReturnsEmpty()
    {
        var writer = new CapturingLogEventWriter();
        var decision = new SchedulingRuleDecision(1, 6, 6, false, new[] { "REQUIRED-ACTION-ACCUMULATION" });

        IReadOnlyList<string> violations = SchedulingGovernanceEvaluator.EvaluateRuleDecisionAndReport(decision, "corr-4", OccurredAtUtc, writer);

        violations.Should().BeEmpty();
    }
}
