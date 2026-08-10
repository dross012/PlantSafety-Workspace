// ============================================================================
//  Project:        Shared.SafetyData.Tests
//  File:           RoutingTelemetryTests.cs
//  Created:        2026-08-10
// ============================================================================
using FluentAssertions;

using Shared.Deterministic.Abstractions.Clock;
using Shared.SafetyData.Abstractions.Models.Industries;
using Shared.SafetyData.Abstractions.Models.Industries.Eventing;
using Shared.SafetyData.Abstractions.Models.Industries.Facilities;
using Shared.SafetyData.Infrastructure.Telemetry;
using Shared.Telemetry.Abstractions.Events;

using Xunit;

namespace Shared.SafetyData.Tests.Telemetry;

public sealed class RoutingTelemetryTests
{
    private sealed class CapturingLogEventWriter : ILogEventWriter
    {
        public ILogEvent? LastEvent { get; private set; }
        public void Write(ILogEvent logEvent) => LastEvent = logEvent;
    }

    private static readonly FixedClock Clock = new(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));

    [Fact]
    public async Task RouteWithTelemetryAsync_MatchesDirectEngineResult()
    {
        var writer = new CapturingLogEventWriter();
        FacilityProfile facility = FacilityProfileBuilder.Build(Guid.NewGuid(), "X", "Chemical Manufacturing", "Loc");
        EventDefinition eventDefinition = EventCatalog.FindByCode("ChemicalSpill")!;

        EventRoutingResult viaWrapper = await RoutingTelemetry.RouteWithTelemetryAsync(eventDefinition, facility, Clock, writer, "corr-1");
        EventRoutingResult direct = EventRoutingEngine.Route(eventDefinition, facility);

        viaWrapper.Should().BeEquivalentTo(direct);
    }

    [Fact]
    public async Task RouteWithTelemetryAsync_WritesRoutingDecisionComputedLogEvent()
    {
        var writer = new CapturingLogEventWriter();
        FacilityProfile facility = FacilityProfileBuilder.Build(Guid.NewGuid(), "X", "Chemical Manufacturing", "Loc");
        EventDefinition eventDefinition = EventCatalog.FindByCode("ChemicalSpill")!;

        await RoutingTelemetry.RouteWithTelemetryAsync(eventDefinition, facility, Clock, writer, "corr-2");

        writer.LastEvent.Should().NotBeNull();
        writer.LastEvent!.EventId.Should().Be("ROUTING_DECISION_COMPUTED");
        writer.LastEvent.Category.Should().Be(LogCategory.Routing);
        writer.LastEvent.CorrelationId.Should().Be("corr-2");
    }
}
