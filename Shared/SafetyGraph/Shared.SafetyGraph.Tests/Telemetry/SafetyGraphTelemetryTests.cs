// ============================================================================
//  Project:        Shared.SafetyGraph.Tests
//  File:           SafetyGraphTelemetryTests.cs
//  Created:        2026-08-10
// ============================================================================
using FluentAssertions;

using Shared.Deterministic.Abstractions.Clock;
using Shared.SafetyGraph.Abstractions.Construction;
using Shared.SafetyGraph.Abstractions.Graph;
using Shared.SafetyGraph.Infrastructure.Telemetry;
using Shared.Telemetry.Abstractions.Events;

using Xunit;

namespace Shared.SafetyGraph.Tests.Telemetry;

public sealed class SafetyGraphTelemetryTests
{
    private sealed class CapturingLogEventWriter : ILogEventWriter
    {
        public ILogEvent? LastEvent { get; private set; }
        public void Write(ILogEvent logEvent) => LastEvent = logEvent;
    }

    private static readonly FixedClock Clock = new(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));

    private static SafetyGraphConstructionRequestContract BuildRequest() => new(
        new SafetyGraphMetadata(SafetyGraphMetadata.CurrentSchemaVersion, "acme-plant-1", "corr-1", Clock.UtcNow),
        Guid.NewGuid(), "Plant One", null, Guid.NewGuid(), null, null, null, null);

    [Fact]
    public async Task ConstructWithTelemetryAsync_ReturnsSameGraphAsDirectAssemble()
    {
        var writer = new CapturingLogEventWriter();
        SafetyGraphConstructionRequestContract request = BuildRequest();

        SafetyGraphModel viaWrapper = await SafetyGraphTelemetry.ConstructWithTelemetryAsync(request, Clock, writer, "corr-1");
        SafetyGraphModel direct = SafetyGraphAssembler.Assemble(request);

        viaWrapper.Should().BeEquivalentTo(direct);
    }

    [Fact]
    public async Task ConstructWithTelemetryAsync_EmitsGraphConstructedLogEvent()
    {
        var writer = new CapturingLogEventWriter();

        await SafetyGraphTelemetry.ConstructWithTelemetryAsync(BuildRequest(), Clock, writer, "corr-2");

        writer.LastEvent.Should().NotBeNull();
        writer.LastEvent!.EventId.Should().Be("SAFETY_GRAPH_CONSTRUCTED");
        writer.LastEvent.Category.Should().Be(LogCategory.SafetyGraph);
    }

    [Fact]
    public async Task InferWithTelemetryAsync_EmitsInferenceComputedLogEvent()
    {
        var writer = new CapturingLogEventWriter();
        SafetyGraphModel graph = SafetyGraphAssembler.Assemble(BuildRequest());

        await SafetyGraphTelemetry.InferWithTelemetryAsync(graph, null, Clock, writer, "corr-3");

        writer.LastEvent!.EventId.Should().Be("SAFETY_GRAPH_INFERENCE_COMPUTED");
    }
}
