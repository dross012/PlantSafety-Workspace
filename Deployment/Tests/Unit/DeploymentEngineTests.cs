// ============================================================================
//  Project:        Deployment.Tests
//  File:           DeploymentEngineTests.cs
//  Created:        2026-08-10
// ============================================================================
using Deployment.Orchestration;

using FluentAssertions;

using Shared.Deterministic.Abstractions.Clock;
using Shared.Telemetry.Abstractions.Events;

using Xunit;

namespace Deployment.Tests.Unit;

[Trait("Category", "Unit")]
public sealed class DeploymentEngineTests
{
    private sealed class CapturingLogEventWriter : ILogEventWriter
    {
        public List<ILogEvent> Events { get; } = new();
        public void Write(ILogEvent logEvent) => Events.Add(logEvent);
    }

    private static readonly FixedClock Clock = new(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));

    [Fact]
    public async Task ExecuteAsync_AllStepsSucceed_ReturnsSucceededWithNoCompensation()
    {
        var writer = new CapturingLogEventWriter();
        var order = new List<string>();
        var steps = new[]
        {
            new DeploymentStep("second", 1, TimeSpan.FromSeconds(5), 0, _ => { order.Add("second"); return Task.FromResult(true); }, null),
            new DeploymentStep("first", 0, TimeSpan.FromSeconds(5), 0, _ => { order.Add("first"); return Task.FromResult(true); }, null),
        };

        DeploymentExecutionResult result = await DeploymentEngine.ExecuteAsync("deploy-1", steps, Clock, writer, "corr-1");

        result.Succeeded.Should().BeTrue();
        result.CompensationRan.Should().BeFalse();
        order.Should().Equal("first", "second");
    }

    [Fact]
    public async Task ExecuteAsync_StepFails_StopsAndRunsCompensationInReverseOrder()
    {
        var writer = new CapturingLogEventWriter();
        var compensated = new List<string>();

        var steps = new[]
        {
            new DeploymentStep("first", 0, TimeSpan.FromSeconds(5), 0, _ => Task.FromResult(true), _ => { compensated.Add("first"); return Task.CompletedTask; }),
            new DeploymentStep("second", 1, TimeSpan.FromSeconds(5), 0, _ => Task.FromResult(false), _ => { compensated.Add("second"); return Task.CompletedTask; }),
            new DeploymentStep("third", 2, TimeSpan.FromSeconds(5), 0, _ => Task.FromResult(true), _ => { compensated.Add("third"); return Task.CompletedTask; }),
        };

        DeploymentExecutionResult result = await DeploymentEngine.ExecuteAsync("deploy-1", steps, Clock, writer, "corr-1");

        result.Succeeded.Should().BeFalse();
        result.CompensationRan.Should().BeTrue();
        result.StepResults.Should().HaveCount(2); // third never ran
        compensated.Should().Equal("first"); // only "first" succeeded before "second" failed
    }

    [Fact]
    public async Task ExecuteAsync_RetriesUpToMaxRetriesThenSucceeds()
    {
        var writer = new CapturingLogEventWriter();
        int attemptCount = 0;
        var steps = new[]
        {
            new DeploymentStep("flaky", 0, TimeSpan.FromSeconds(5), 2, _ =>
            {
                attemptCount++;
                return Task.FromResult(attemptCount >= 3);
            }, null),
        };

        DeploymentExecutionResult result = await DeploymentEngine.ExecuteAsync("deploy-1", steps, Clock, writer, "corr-1");

        result.Succeeded.Should().BeTrue();
        result.StepResults[0].AttemptsUsed.Should().Be(3);
    }

    [Fact]
    public async Task ExecuteAsync_ExhaustsRetries_Fails()
    {
        var writer = new CapturingLogEventWriter();
        var steps = new[]
        {
            new DeploymentStep("always-fails", 0, TimeSpan.FromSeconds(5), 1, _ => Task.FromResult(false), null),
        };

        DeploymentExecutionResult result = await DeploymentEngine.ExecuteAsync("deploy-1", steps, Clock, writer, "corr-1");

        result.Succeeded.Should().BeFalse();
        result.StepResults[0].AttemptsUsed.Should().Be(2); // 1 initial + 1 retry
    }

    [Fact]
    public async Task ExecuteAsync_EmitsStepExecutedLogEventPerStep()
    {
        var writer = new CapturingLogEventWriter();
        var steps = new[] { new DeploymentStep("only", 0, TimeSpan.FromSeconds(5), 0, _ => Task.FromResult(true), null) };

        await DeploymentEngine.ExecuteAsync("deploy-1", steps, Clock, writer, "corr-1");

        writer.Events.Should().Contain(e => e.EventId == "DEPLOYMENT_STEP_EXECUTED");
    }

    [Fact]
    public async Task ExecuteAsync_NullSteps_Throws()
    {
        var writer = new CapturingLogEventWriter();

        Func<Task> act = () => DeploymentEngine.ExecuteAsync("deploy-1", null!, Clock, writer, "corr-1");

        await act.Should().ThrowAsync<ArgumentNullException>();
    }
}
