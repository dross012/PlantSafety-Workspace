// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Application.Pipeline
//  Layer:          Tests (Unit)
//  File:           ValidationBehaviorTests.cs
//  Created:        2026-06-17
//  SchemaVersion:  2026.1
//  Description:    Unit tests for MediatR ValidationBehavior pipeline step.
// ============================================================================
using FluentValidation;
using MediatR;
using NSubstitute;
using PlantSafety.Application.Behaviors.Validation;

namespace PlantSafety.Tests.Application.Pipeline;

public sealed class ValidationBehaviorTests : UnitTestBase
{
    // Must be public (not private/internal): NSubstitute's Castle DynamicProxy needs to
    // generate a proxy for IValidator<TestCommand> in a separate dynamic assembly, which
    // requires TestCommand to be externally accessible.
    public sealed record TestCommand : IRequest<Unit>;
    private sealed class TestCommandValidator : AbstractValidator<TestCommand> { }

    [Fact]
    public async Task Handle_WithNoValidators_ShouldCallNext()
    {
        var behavior = new ValidationBehavior<TestCommand, Unit>([]);
        var next = Substitute.For<RequestHandlerDelegate<Unit>>();
        next.Invoke().Returns(Unit.Value);

        await behavior.Handle(new TestCommand(), next, CancellationToken.None);

        await next.Received(1).Invoke();
    }

    [Fact]
    public async Task Handle_WithPassingValidator_ShouldCallNext()
    {
        IValidator<TestCommand>[] validators = [new TestCommandValidator()];
        var behavior = new ValidationBehavior<TestCommand, Unit>(validators);
        var next = Substitute.For<RequestHandlerDelegate<Unit>>();
        next.Invoke().Returns(Unit.Value);

        await behavior.Handle(new TestCommand(), next, CancellationToken.None);

        await next.Received(1).Invoke();
    }

    [Fact]
    public async Task Handle_WithFailingValidator_ShouldThrowValidationException()
    {
        var failValidator = Substitute.For<IValidator<TestCommand>>();
        var failures = new[] { new FluentValidation.Results.ValidationFailure("Prop", "Error") };
        failValidator.ValidateAsync(Arg.Any<ValidationContext<TestCommand>>(), Arg.Any<CancellationToken>())
                     .Returns(new FluentValidation.Results.ValidationResult(failures));

        var behavior = new ValidationBehavior<TestCommand, Unit>([failValidator]);
        var next = Substitute.For<RequestHandlerDelegate<Unit>>();

        await FluentActions.Invoking(() => behavior.Handle(new TestCommand(), next, CancellationToken.None))
                           .Should().ThrowAsync<ValidationException>();
    }
}
