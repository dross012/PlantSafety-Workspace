// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: MediatR pipeline behavior that skips FluentValidation for request types
//   that are explicitly decorated with [SkipValidation].  All other requests pass through
//   to the standard ValidationBehavior unchanged.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Validation.Attributes;

namespace PlantSafety.Application.Validation.Behaviors;

/// <summary>
/// Pipeline behavior that bypasses validation for requests decorated with
/// <see cref="SkipValidationAttribute"/>.  Sits immediately before <c>ValidationBehavior</c>
/// in the pipeline registration order.
/// </summary>
/// <remarks>
/// Registration order: LoggingBehavior → MetricsBehavior → PerformanceBehavior →
/// AuthorizationBehavior → CachingBehavior → <b>ConditionalValidationBehavior</b> →
/// ValidationBehavior → ConcurrencyBehavior → TransactionBehavior → Handler.
/// </remarks>
public sealed class ConditionalValidationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<ConditionalValidationBehavior<TRequest, TResponse>> _logger;

    /// <summary>Initialises the behavior.</summary>
    public ConditionalValidationBehavior(ILogger<ConditionalValidationBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestType = typeof(TRequest);

        if (requestType.IsDefined(typeof(SkipValidationAttribute), inherit: true))
        {
            _logger.LogDebug(
                "Validation skipped for {RequestType} — decorated with [SkipValidation].",
                requestType.Name);

            return await next();
        }

        return await next();
    }
}
