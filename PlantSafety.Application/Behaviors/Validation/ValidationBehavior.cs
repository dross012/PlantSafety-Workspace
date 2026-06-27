// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: MediatR pipeline behavior that runs all registered FluentValidation validators.
//   Executes THIRD in the pipeline: Logging → Performance → Validation → Transaction → Handler.
//   Throws ValidationException on the first set of failures — no partial execution.
//   Validators MUST NOT call repositories, log, or mutate state (§2.3).
// SchemaVersion: 1
// Additive-only evolution: new fields may be added; no behavior may be altered destructively.

using FluentValidation;
using MediatR;

namespace PlantSafety.Application.Behaviors.Validation;

/// <summary>
/// MediatR pipeline behavior that validates all incoming requests using FluentValidation.
/// Throws <see cref="ValidationException"/> with all collected failures if any validator fails.
/// Position in pipeline: third (after Logging and Performance, before Transaction and Handler).
/// </summary>
/// <typeparam name="TRequest">The MediatR request type.</typeparam>
/// <typeparam name="TResponse">The response type produced by the handler.</typeparam>
/// <remarks>
/// Initializes a new instance of <see cref="ValidationBehavior{TRequest,TResponse}"/>.
/// </remarks>
/// <param name="validators">All registered FluentValidation validators for the request type.</param>
public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators = validators;

    /// <summary>
    /// Validates the request against all registered validators and delegates to the next behavior on success.
    /// </summary>
    /// <param name="request">The incoming MediatR request.</param>
    /// <param name="next">The delegate to the next behavior or final handler.</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    /// <returns>The response from the handler.</returns>
    /// <exception cref="ValidationException">Thrown when one or more validators report failures.</exception>
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next(cancellationToken);
        }

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count > 0)
        {
            throw new ValidationException(failures);
        }

        return await next(cancellationToken);
    }
}
