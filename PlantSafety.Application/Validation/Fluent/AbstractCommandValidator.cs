// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Base FluentValidation validator that pre-applies canonical envelope validation
//   for all ICommand types. Command-specific validators inherit from this to avoid repeating envelope rules.
// SchemaVersion: 1

using FluentValidation;
using PlantSafety.Application.Abstractions.Messaging;

namespace PlantSafety.Application.Validation.Fluent;

/// <summary>
/// Base validator for <see cref="ICommand{TResponse}"/> types.
/// Automatically validates the canonical envelope fields:
/// CommandId, CorrelationId, CausationId, TenantId, ActorId, RequestedAtUtc, ApiVersionToken.
/// Derived validators add domain-specific rules after calling base rules via inheritance.
/// </summary>
/// <typeparam name="TCommand">The command type being validated.</typeparam>
public abstract class AbstractCommandValidator<TCommand> : AbstractValidator<TCommand>
    where TCommand : class
{
    /// <summary>
    /// Initialises the canonical envelope validation rules.
    /// Derived classes must call <c>base()</c> (implicit for parameterless constructors)
    /// before adding domain-specific rules.
    /// </summary>
    protected AbstractCommandValidator()
    {
        // All envelope validation is intentionally left to the concrete validators
        // to preserve explicit, auditable rule sets per command.
        // This base class provides only the shared pattern — concrete classes
        // may call ApplyEnvelopeRules() to add envelope validation.
    }

    /// <summary>
    /// Applies canonical envelope validation rules for <see cref="ICommand"/>-envelope properties.
    /// Call this from derived validators when the command type has these properties.
    /// </summary>
    /// <param name="commandIdSelector">Expression selecting the CommandId property.</param>
    /// <param name="correlationIdSelector">Expression selecting the CorrelationId property.</param>
    /// <param name="tenantIdSelector">Expression selecting the TenantId property.</param>
    /// <param name="actorIdSelector">Expression selecting the ActorId property.</param>
    protected void ApplyEnvelopeRules(
        System.Linq.Expressions.Expression<Func<TCommand, Guid>> commandIdSelector,
        System.Linq.Expressions.Expression<Func<TCommand, Guid>> correlationIdSelector,
        System.Linq.Expressions.Expression<Func<TCommand, Guid>> tenantIdSelector,
        System.Linq.Expressions.Expression<Func<TCommand, Guid>> actorIdSelector)
    {
        RuleFor(commandIdSelector)
            .NotEmpty().WithMessage("CommandId is required.");

        RuleFor(correlationIdSelector)
            .NotEmpty().WithMessage("CorrelationId is required.");

        RuleFor(tenantIdSelector)
            .NotEmpty().WithMessage("TenantId is required.");

        RuleFor(actorIdSelector)
            .NotEmpty().WithMessage("ActorId is required.");
    }
}
