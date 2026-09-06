// ============================================================================
//  Project:        Shared.Authorization.Tests
//  File:           ContractGovernanceEvaluatorTests.cs
//  Created:        2026-08-10
// ============================================================================
using AwesomeAssertions;

using Shared.Authorization.Abstractions.Caller;
using Shared.Authorization.Abstractions.Contracts;
using Shared.Authorization.Infrastructure.Governance;
using Shared.Deterministic.Abstractions.Clock;
using Shared.Telemetry.Abstractions.Events;

using Xunit;

namespace Shared.Authorization.Tests.Governance;

public sealed class ContractGovernanceEvaluatorTests
{
    private sealed class CapturingLogEventWriter : ILogEventWriter
    {
        public ILogEvent? LastEvent { get; private set; }
        public void Write(ILogEvent logEvent) => LastEvent = logEvent;
    }

    private static readonly FixedClock Clock = new(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));

    [Fact]
    public void EvaluateAndReport_Valid_WritesNoLogEvent()
    {
        var writer = new CapturingLogEventWriter();
        var metadata = new ContractMetadata("Safety.Editor", "corr-1", MfaStatus.NotApplicable, "acme-plant-1", Clock.UtcNow);

        ContractGovernanceEvaluator.EvaluateAndReport(metadata, Clock, writer).Should().BeEmpty();
        writer.LastEvent.Should().BeNull();
    }

    [Fact]
    public void EvaluateAndReport_Invalid_WritesGovernanceViolationLogEvent()
    {
        var writer = new CapturingLogEventWriter();

        IReadOnlyList<string> violations = ContractGovernanceEvaluator.EvaluateAndReport(null, Clock, writer);

        violations.Should().NotBeEmpty();
        writer.LastEvent!.EventId.Should().Be("CONTRACT_GOVERNANCE_VIOLATION");
        writer.LastEvent.Severity.Should().Be(LogSeverity.Warning);
    }
}
