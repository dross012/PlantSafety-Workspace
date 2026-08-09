// ============================================================================
//  Project:        Shared.ErrorHandling
//  Module:         Abstractions.Envelope
//  File:           ExceptionClassifier.cs
//  Created:        2026-08-09
//  Description:    Deterministic .NET exception type -> ErrorCategory
//                  classification. This is the seam that lets the global
//                  exception-handling middleware (PlantSafety.UI.Api)
//                  produce a correctly categorized ErrorEnvelope without
//                  every controller/engine needing to catch and reclassify
//                  its own exceptions -- narrower, purpose-built exception
//                  types (a future ValidationException, NotFoundException,
//                  etc.) slot in here as they're introduced, without
//                  touching call sites.
// ============================================================================
namespace Shared.ErrorHandling.Abstractions.Envelope;

/// <summary>Deterministically classifies a .NET exception into an ErrorCategory.</summary>
public static class ExceptionClassifier
{
    /// <summary>Classifies the given exception.</summary>
    public static ErrorCategory Classify(Exception exception) => exception switch
    {
        ArgumentException => ErrorCategory.Validation,
        FormatException => ErrorCategory.Validation,
        KeyNotFoundException => ErrorCategory.NotFound,
        InvalidOperationException => ErrorCategory.Conflict,
        UnauthorizedAccessException => ErrorCategory.Unauthorized,
        HttpRequestException => ErrorCategory.Dependency,
        TimeoutException => ErrorCategory.Dependency,
        _ => ErrorCategory.Internal,
    };
}
