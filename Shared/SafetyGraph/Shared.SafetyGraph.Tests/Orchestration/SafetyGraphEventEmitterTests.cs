// ============================================================================
//  Project:        Shared.SafetyGraph.Tests
//  File:           SafetyGraphEventEmitterTests.cs
//  Created:        2026-08-10
// ============================================================================
using FluentAssertions;

using Shared.PredictiveSafety.Abstractions.Common;
using Shared.SafetyGraph.Abstractions.Graph;
using Shared.SafetyGraph.Abstractions.Inference;
using Shared.SafetyGraph.Infrastructure.Orchestration;
using Shared.Telemetry.Abstractions.Events;

using Xunit;

namespace Shared.SafetyGraph.Tests.Orchestration;

public sealed class SafetyGraphEventEmitterTests
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
        var metadata = new SafetyGraphMetadata(SafetyGraphMetadata.CurrentSchemaVersion, "acme-plant-1", "corr-1", DateTime.UtcNow);
        var graph = new SafetyGraphModel(metadata, Array.Empty<SafetyGraphNode>(), Array.Empty<SafetyGraphEdge>());
        var inference = new SafetyGraphInferenceResult(false, PredictivePriority.Low, Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>());
        var result = new SafetyGraphEvaluationResultContract(metadata, graph, inference);

        SafetyGraphEventEmitter.Emit(result, writer);

        writer.LastEvent.Should().NotBeNull();
        writer.LastEvent!.EventId.Should().Be("SAFETY_GRAPH_EVALUATION_EVENT_PUBLISHED");
        writer.LastEvent.CorrelationId.Should().Be("corr-1");
    }

    [Fact]
    public void Emit_NullResult_Throws()
    {
        var writer = new CapturingLogEventWriter();

        Action act = () => SafetyGraphEventEmitter.Emit(null!, writer);

        act.Should().Throw<ArgumentNullException>();
    }
}
