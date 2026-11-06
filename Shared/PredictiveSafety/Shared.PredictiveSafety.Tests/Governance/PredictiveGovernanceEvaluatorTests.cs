// ============================================================================
//  Project:        Shared.PredictiveSafety.Tests
//  File:           PredictiveGovernanceEvaluatorTests.cs
//  Created:        2026-08-10
// ============================================================================
using FluentAssertions;

using Shared.PredictiveSafety.Abstractions.Common;
using Shared.PredictiveSafety.Abstractions.Contracts;
using Shared.PredictiveSafety.Infrastructure.Governance;
using Shared.Telemetry.Abstractions.Events;

using Xunit;

namespace Shared.PredictiveSafety.Tests.Governance;

public sealed class PredictiveGovernanceEvaluatorTests
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
        var metadata = new PredictiveSafetyContractMetadata(PredictiveSafetyContractMetadata.CurrentSchemaVersion, "acme-plant-1", "corr-1", OccurredAtUtc);

        PredictiveGovernanceEvaluator.EvaluateContractMetadataAndReport(metadata, OccurredAtUtc, writer).Should().BeEmpty();
        writer.LastEvent.Should().BeNull();
    }

    [Fact]
    public void EvaluateContractMetadataAndReport_InvalidMetadata_WritesGovernanceViolationEvent()
    {
        var writer = new CapturingLogEventWriter();
        var metadata = new PredictiveSafetyContractMetadata(PredictiveSafetyContractMetadata.CurrentSchemaVersion, string.Empty, "corr-2", OccurredAtUtc);

        IReadOnlyList<string> violations = PredictiveGovernanceEvaluator.EvaluateContractMetadataAndReport(metadata, OccurredAtUtc, writer);

        violations.Should().NotBeEmpty();
        writer.LastEvent!.EventId.Should().Be("PREDICTIVE_GOVERNANCE_VIOLATION");
        writer.LastEvent.Severity.Should().Be(LogSeverity.Warning);
    }

    [Fact]
    public void EvaluateEscalationOutcomeAndReport_InconsistentOutcome_WritesGovernanceViolationEvent()
    {
        var writer = new CapturingLogEventWriter();
        var outcome = new PredictiveEscalationOutcome(true, Array.Empty<string>());

        PredictiveGovernanceEvaluator.EvaluateEscalationOutcomeAndReport(outcome, "corr-3", OccurredAtUtc, writer).Should().NotBeEmpty();
        writer.LastEvent!.EventId.Should().Be("PREDICTIVE_GOVERNANCE_VIOLATION");
    }
}
