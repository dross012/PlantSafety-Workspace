// ============================================================================
//  Project:        Shared.ErrorHandling
//  Module:         Abstractions.Envelope
//  File:           ErrorEnvelopeFactory.cs
//  Created:        2026-08-09
//  Description:    Builds a standardized ErrorEnvelope from any .NET
//                  exception. UserMessage is deliberately generic per
//                  category (never the raw exception message, which may
//                  leak implementation detail) -- DeveloperMessage carries
//                  the real exception message for logs/diagnostics.
// ============================================================================
namespace Shared.ErrorHandling.Abstractions.Envelope;

/// <summary>Builds a standardized ErrorEnvelope from any .NET exception.</summary>
public static class ErrorEnvelopeFactory
{
    /// <summary>Builds an ErrorEnvelope for the given exception.</summary>
    public static ErrorEnvelope FromException(Exception exception, string module, string correlationId, DateTime occurredAtUtc)
    {
        ErrorCategory category = ExceptionClassifier.Classify(exception);
        return new ErrorEnvelope(
            ErrorCode: $"{module.ToUpperInvariant()}-{category.ToString().ToUpperInvariant()}",
            Category: category,
            UserMessage: UserMessageFor(category),
            DeveloperMessage: exception.Message,
            CorrelationId: correlationId,
            Module: module,
            OccurredAtUtc: occurredAtUtc);
    }

    private static string UserMessageFor(ErrorCategory category) => category switch
    {
        ErrorCategory.Validation => "The request was invalid. Check the submitted values and try again.",
        ErrorCategory.NotFound => "The requested item could not be found.",
        ErrorCategory.Conflict => "The request could not be completed because of the item's current state.",
        ErrorCategory.Unauthorized => "You are not authorized to perform this action.",
        ErrorCategory.Dependency => "A dependent service is currently unavailable. Please try again shortly.",
        ErrorCategory.Internal => "An unexpected error occurred. Please try again or contact support.",
        _ => "An unexpected error occurred.",
    };
}
