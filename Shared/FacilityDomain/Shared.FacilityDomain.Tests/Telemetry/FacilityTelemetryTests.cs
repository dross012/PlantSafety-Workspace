// ============================================================================
//  Project:        Shared.FacilityDomain.Tests
//  File:           FacilityTelemetryTests.cs
//  Created:        2026-08-10
// ============================================================================
using FluentAssertions;

using Shared.Deterministic.Abstractions.Clock;
using Shared.FacilityDomain.Abstractions.Hazards;
using Shared.FacilityDomain.Abstractions.Regulatory;
using Shared.FacilityDomain.Abstractions.Severity;
using Shared.FacilityDomain.Infrastructure.Telemetry;
using Shared.Telemetry.Abstractions.Events;

using Xunit;

namespace Shared.FacilityDomain.Tests.Telemetry;

public sealed class FacilityTelemetryTests
{
    private sealed class CapturingLogEventWriter : ILogEventWriter
    {
        public ILogEvent? LastEvent { get; private set; }
        public void Write(ILogEvent logEvent) => LastEvent = logEvent;
    }

    private static readonly FixedClock Clock = new(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));

    [Fact]
    public async Task InferHazardTypeWithTelemetryAsync_MatchesDirectRuleResult()
    {
        var writer = new CapturingLogEventWriter();

        HazardType viaWrapper = await FacilityTelemetry.InferHazardTypeWithTelemetryAsync("Fire in warehouse", null, Clock, writer, "corr-1");
        HazardType direct = HazardInferenceRules.InferHazardTypeFromSafetyEvent("Fire in warehouse", null);

        viaWrapper.Should().Be(direct);
        writer.LastEvent!.EventId.Should().Be("FACILITY_INFERENCE_RULE_EVALUATED");
        writer.LastEvent.Payload["Rule"].Should().Be("Hazard");
    }

    [Fact]
    public async Task InferSeverityWithTelemetryAsync_MatchesDirectRuleResult()
    {
        var writer = new CapturingLogEventWriter();

        SeverityLevel viaWrapper = await FacilityTelemetry.InferSeverityWithTelemetryAsync(HazardType.Fire, 2, 1, 3, Clock, writer, "corr-2");
        SeverityLevel direct = SeverityInferenceRules.Infer(HazardType.Fire, 2, 1, 3);

        viaWrapper.Should().Be(direct);
        writer.LastEvent!.Payload["Rule"].Should().Be("Severity");
    }

    [Fact]
    public async Task InferRegulatoryJurisdictionsWithTelemetryAsync_MatchesDirectRuleResult()
    {
        var writer = new CapturingLogEventWriter();

        IReadOnlyList<RegulatoryJurisdiction> viaWrapper = await FacilityTelemetry.InferRegulatoryJurisdictionsWithTelemetryAsync(
            HazardType.Fire, SeverityLevel.Severe, Clock, writer, "corr-3");
        IReadOnlyList<RegulatoryJurisdiction> direct = RegulatoryInferenceRules.Infer(HazardType.Fire, SeverityLevel.Severe);

        viaWrapper.Should().BeEquivalentTo(direct);
        writer.LastEvent!.Payload["Rule"].Should().Be("Regulatory");
    }
}
