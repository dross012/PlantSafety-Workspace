// ============================================================================
//  Project:        PlantSafety.UI.Tests
//  File:           DebounceGuardTests.cs
//  Created:        2026-08-09
//  Description:    Tests for Phase 2.2's DebounceGuard, using the real
//                  UiDispatcher (this test host has no
//                  SynchronizationContext, so it runs actions inline --
//                  exactly the same "no-op passthrough" behavior a
//                  non-UI console test host needs). Uses a short real
//                  delay (the guard is backed by a real
//                  System.Threading.Timer) and a completion signal
//                  instead of Thread.Sleep guessing, to keep the test
//                  both real and fast.
// ============================================================================
using AwesomeAssertions;

using PlantSafety.UI.Policies;
using PlantSafety.UI.Services;

using Xunit;

namespace PlantSafety.UI.Tests.Policies;

public sealed class DebounceGuardTests
{
    [Fact]
    public async Task Trigger_SingleCall_FiresAfterDelay()
    {
        var dispatcher = new UiDispatcher();
        using var guard = new DebounceGuard(dispatcher, TimeSpan.FromMilliseconds(20));
        var fired = new TaskCompletionSource();

        guard.Trigger(() =>
        {
            fired.SetResult();
            return Task.CompletedTask;
        });

        await fired.Task.WaitAsync(TimeSpan.FromSeconds(2));
    }

    [Fact]
    public async Task Trigger_RapidRepeatedCalls_OnlyTheLastActionFires()
    {
        var dispatcher = new UiDispatcher();
        using var guard = new DebounceGuard(dispatcher, TimeSpan.FromMilliseconds(30));
        var fireCount = 0;
        string? lastValue = null;
        var fired = new TaskCompletionSource();

        for (var i = 0; i < 5; i++)
        {
            string value = $"call-{i}";
            guard.Trigger(() =>
            {
                Interlocked.Increment(ref fireCount);
                lastValue = value;
                fired.TrySetResult();
                return Task.CompletedTask;
            });
        }

        await fired.Task.WaitAsync(TimeSpan.FromSeconds(2));
        await Task.Delay(50);

        fireCount.Should().Be(1);
        lastValue.Should().Be("call-4");
    }
}
