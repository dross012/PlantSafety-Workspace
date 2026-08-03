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
using Microsoft.Extensions.Options;
using NSubstitute;
using PlantSafety.Application.Abstractions.Context;
using PlantSafety.Application.Abstractions.Logging;
using PlantSafety.Application.Behaviors.Performance;

namespace PlantSafety.Tests.Application.Pipeline;

public sealed class PerformanceBehaviorTests : UnitTestBase
{
    private sealed record TestCommand : IRequest<Unit>;

    [Fact]
    public async Task Handle_ShouldCallNext()
    {
        var applicationLogger = Substitute.For<IApplicationLogger>();
        var operationContext = Substitute.For<IOperationContext>();
        var options = Options.Create(new PerformanceOptions());
        var behavior = new PerformanceBehavior<TestCommand, Unit>(applicationLogger, operationContext, options);
        var next = Substitute.For<RequestHandlerDelegate<Unit>>();
        next.Invoke().Returns(Unit.Value);

        await behavior.Handle(new TestCommand(), next, CancellationToken.None);

        await next.Received(1).Invoke();
    }
}
