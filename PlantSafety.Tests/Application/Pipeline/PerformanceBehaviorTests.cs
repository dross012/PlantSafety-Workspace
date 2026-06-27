// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Application.Pipeline
//  Layer:          Tests (Unit)
//  File:           PerformanceBehaviorTests.cs
//  Created:        2026-06-17
//  SchemaVersion:  2026.1
//  Description:    Unit tests for MediatR PerformanceBehavior — logs slow requests.
// ============================================================================
using MediatR;
using NSubstitute;
using PlantSafety.Application.Common.Behaviours;

namespace PlantSafety.Tests.Application.Pipeline;

public sealed class PerformanceBehaviorTests : UnitTestBase
{
    private sealed record TestCommand : IRequest<Unit>;

    [Fact]
    public async Task Handle_ShouldCallNext()
    {
        var logger = NullLogger<PerformanceBehavior<TestCommand, Unit>>();
        var behavior = new PerformanceBehavior<TestCommand, Unit>(logger);
        var next = Substitute.For<RequestHandlerDelegate<Unit>>();
        next.Invoke().Returns(Unit.Value);

        await behavior.Handle(new TestCommand(), next, CancellationToken.None);

        await next.Received(1).Invoke();
    }
}
