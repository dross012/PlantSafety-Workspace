// ============================================================================
//  Project:        Shared.PredictiveSafety.Tests
//  File:           PredictiveSafetyEventEmitterTests.cs
//  Created:        2026-08-10
// ============================================================================
using FluentAssertions;

using Shared.FacilityDomain.Abstractions.Hazards;
using Shared.FacilityDomain.Abstractions.Regulatory;
using Shared.FacilityDomain.Abstractions.Severity;
using Shared.PredictiveSafety.Abstractions.Audit;
using Shared.PredictiveSafety.Abstractions.Common;
using Shared.PredictiveSafety.Abstractions.Contracts;
using Shared.PredictiveSafety.Abstractions.Hazard;
using Shared.PredictiveSafety.Abstractions.Maintenance;
using Shared.PredictiveSafety.Abstractions.Routing;
using Shared.PredictiveSafety.Abstractions.Scheduling;
using Shared.PredictiveSafety.Infrastructure.Orchestration;
using Shared.Telemetry.Abstractions.Events;

using Xunit;

namespace Shared.PredictiveSafety.Tests.Orchestration;

public sealed class PredictiveSafetyEventEmitterTests
{
    private sealed class CapturingLogEventWriter : ILogEventWriter
    {
        public ILogEvent? LastEvent { get; private set; }
        public void Write(ILogEvent logEvent) => LastEvent = logEvent;
    }

    [Fact]
    public void Emit_PublishesEvaluationEventPublishedLogEventWithMatchingCorrelation()
    {
        var writer = new CapturingLogEventWriter();
        var metadata = new PredictiveSafetyContractMetadata(PredictiveSafetyContractMetadata.CurrentSchemaVersion, "acme-plant-1", "corr-1", DateTime.UtcNow);
        var hazard = new PredictiveHazardDecision(HazardType.Fire, SeverityLevel.Minor, Array.Empty<RegulatoryJurisdiction>(), false, PredictivePriority.Low, Array.Empty<string>());
        var maintenance = new PredictiveMaintenanceDecision(Guid.NewGuid(), false, PredictivePriority.Low, Array.Empty<string>());
        var routing = new PredictiveRoutingDecision("Fire", Guid.NewGuid(), false, PredictivePriority.Low, Array.Empty<string>());
        var scheduling = new PredictiveSchedulingDecision(false, PredictivePriority.Low, Array.Empty<string>());
        var audit = new PredictiveAuditAnomaly(false, PredictivePriority.Low, Array.Empty<string>());
        var result = new PredictiveSafetyEvaluationResultContract(metadata, hazard, maintenance, routing, scheduling, audit, PredictivePriority.Low);

        PredictiveSafetyEventEmitter.Emit(result, writer);

        writer.LastEvent.Should().NotBeNull();
        writer.LastEvent!.EventId.Should().Be("PREDICTIVE_EVALUATION_EVENT_PUBLISHED");
        writer.LastEvent.CorrelationId.Should().Be("corr-1");
    }

    [Fact]
    public void Emit_NullResult_Throws()
    {
        var writer = new CapturingLogEventWriter();

        Action act = () => PredictiveSafetyEventEmitter.Emit(null!, writer);

        act.Should().Throw<ArgumentNullException>();
    }
}
