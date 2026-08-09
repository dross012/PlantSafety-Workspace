// ============================================================================
//  Project:        Shared.ModuleContracts.Tests
//  File:           ModuleEventGatewayTests.cs
//  Created:        2026-08-09
// ============================================================================
using AwesomeAssertions;

using Shared.ModuleContracts.Abstractions.Events;

using Xunit;

namespace Shared.ModuleContracts.Tests.Events;

public sealed class ModuleEventGatewayTests
{
    private sealed record TestEvent(string SourceModule, DateTime OccurredAtUtc, string Payload) : IModuleEvent;

    [Fact]
    public async Task PublishAsync_OneSubscriber_ReceivesTheEvent()
    {
        var gateway = new ModuleEventGateway();
        TestEvent? received = null;
        gateway.Subscribe<TestEvent>((evt, _) =>
        {
            received = evt;
            return Task.CompletedTask;
        });

        await gateway.PublishAsync(new TestEvent("Facility", DateTime.UtcNow, "created"));

        received.Should().NotBeNull();
        received!.Payload.Should().Be("created");
    }

    [Fact]
    public async Task PublishAsync_MultipleSubscribers_AllReceiveTheEvent()
    {
        var gateway = new ModuleEventGateway();
        var callCount = 0;
        gateway.Subscribe<TestEvent>((_, _) => { callCount++; return Task.CompletedTask; });
        gateway.Subscribe<TestEvent>((_, _) => { callCount++; return Task.CompletedTask; });

        await gateway.PublishAsync(new TestEvent("Facility", DateTime.UtcNow, "created"));

        callCount.Should().Be(2);
    }

    [Fact]
    public async Task PublishAsync_NoSubscribers_DoesNotThrow()
    {
        var gateway = new ModuleEventGateway();

        Func<Task> act = () => gateway.PublishAsync(new TestEvent("Facility", DateTime.UtcNow, "created"));

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Subscribe_DisposedToken_StopsReceivingFutureEvents()
    {
        var gateway = new ModuleEventGateway();
        var callCount = 0;
        IDisposable subscription = gateway.Subscribe<TestEvent>((_, _) => { callCount++; return Task.CompletedTask; });

        subscription.Dispose();
        await gateway.PublishAsync(new TestEvent("Facility", DateTime.UtcNow, "created"));

        callCount.Should().Be(0);
    }

    [Fact]
    public async Task PublishAsync_OneSubscriberThrows_OtherSubscribersStillRun()
    {
        var gateway = new ModuleEventGateway();
        var secondRan = false;
        gateway.Subscribe<TestEvent>((_, _) => throw new InvalidOperationException("boom"));
        gateway.Subscribe<TestEvent>((_, _) => { secondRan = true; return Task.CompletedTask; });

        Func<Task> act = () => gateway.PublishAsync(new TestEvent("Facility", DateTime.UtcNow, "created"));

        await act.Should().ThrowAsync<InvalidOperationException>();
        secondRan.Should().BeTrue();
    }

    [Fact]
    public async Task PublishAsync_DifferentEventTypes_OnlyMatchingSubscribersReceive()
    {
        var gateway = new ModuleEventGateway();
        var received = 0;
        gateway.Subscribe<TestEvent>((_, _) => { received++; return Task.CompletedTask; });

        await gateway.PublishAsync(new OtherEvent("Employee", DateTime.UtcNow));

        received.Should().Be(0);
    }

    private sealed record OtherEvent(string SourceModule, DateTime OccurredAtUtc) : IModuleEvent;
}
