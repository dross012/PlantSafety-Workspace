// ============================================================================
//  Project:        PlantSafety.UI.Tests
//  File:           UiDispatcherTests.cs
//  Created:        2026-08-09
// ============================================================================
using AwesomeAssertions;

using PlantSafety.UI.Services;

using Xunit;

namespace PlantSafety.UI.Tests.Services;

public sealed class UiDispatcherTests
{
    [Fact]
    public async Task InvokeAsync_NoSynchronizationContext_RunsInline()
    {
        var dispatcher = new UiDispatcher();
        var ran = false;

        await dispatcher.InvokeAsync(() => ran = true);

        ran.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_FuncOverload_PropagatesTheResultOfTheAwaitedTask()
    {
        var dispatcher = new UiDispatcher();
        var ran = false;

        await dispatcher.InvokeAsync(async () =>
        {
            await Task.Delay(1);
            ran = true;
        });

        ran.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_ActionThrows_PropagatesTheException()
    {
        var dispatcher = new UiDispatcher();

        Func<Task> act = () => dispatcher.InvokeAsync(() => throw new InvalidOperationException("boom"));

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("boom");
    }
}
