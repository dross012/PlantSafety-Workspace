// ============================================================================
//  Project:        Shared.ModuleContracts.Tests
//  File:           ModuleEventGatewayExtensionsTests.cs
//  Created:        2026-08-09
// ============================================================================
using AwesomeAssertions;

using Shared.ModuleContracts.Abstractions.Events;
using Shared.Telemetry.Abstractions.Events;

using Xunit;

namespace Shared.ModuleContracts.Tests.Events;

public sealed class ModuleEventGatewayExtensionsTests
{
    private sealed record TestEvent(string SourceModule, DateTime OccurredAtUtc, string Payload) : IModuleEvent;

    private sealed class CapturingLogEventWriter : ILogEventWriter
    {
        public ILogEvent? Captured { get; private set; }
        public void Write(ILogEvent logEvent) => Captured = logEvent;
    }

    [Fact]
    public async Task PublishIsolatedAsync_SubscriberThrows_DoesNotPropagateToCaller()
    {
        var gateway = new ModuleEventGateway();
        var writer = new CapturingLogEventWriter();
        gateway.Subscribe<TestEvent>((_, _) => throw new InvalidOperationException("boom"));

        Func<Task> act = () => gateway.PublishIsolatedAsync(
            new TestEvent("Facility", DateTime.UtcNow, "created"), writer, "corr-1");

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task PublishIsolatedAsync_SubscriberThrows_LogsTheFailure()
    {
        var gateway = new ModuleEventGateway();
        var writer = new CapturingLogEventWriter();
        gateway.Subscribe<TestEvent>((_, _) => throw new InvalidOperationException("boom"));

        await gateway.PublishIsolatedAsync(new TestEvent("Facility", DateTime.UtcNow, "created"), writer, "corr-2");

        writer.Captured.Should().NotBeNull();
        writer.Captured!.EventId.Should().Be("MODULE_EVENT_PUBLISH_FAILED");
        writer.Captured!.Severity.Should().Be(LogSeverity.Error);
        writer.Captured!.CorrelationId.Should().Be("corr-2");
        writer.Captured!.Module.Should().Be("Facility");
    }

    [Fact]
    public async Task PublishIsolatedAsync_NoFailures_DoesNotLogAnything()
    {
        var gateway = new ModuleEventGateway();
        var writer = new CapturingLogEventWriter();
        var received = false;
        gateway.Subscribe<TestEvent>((_, _) => { received = true; return Task.CompletedTask; });

        await gateway.PublishIsolatedAsync(new TestEvent("Facility", DateTime.UtcNow, "created"), writer, "corr-3");

        received.Should().BeTrue();
        writer.Captured.Should().BeNull();
    }
}
