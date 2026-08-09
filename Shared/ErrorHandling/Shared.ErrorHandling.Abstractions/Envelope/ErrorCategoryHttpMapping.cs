// ============================================================================
//  Project:        Shared.ErrorHandling
//  Module:         Abstractions.Envelope
//  File:           ErrorCategoryHttpMapping.cs
//  Created:        2026-08-09
//  Description:    Deterministic ErrorCategory -> HTTP status code mapping,
//                  used by PlantSafety.UI.Api's global exception-handling
//                  middleware. Kept as a pure lookup, not scattered as
//                  magic numbers at each call site.
// ============================================================================
namespace Shared.ErrorHandling.Abstractions.Envelope;

/// <summary>Deterministic ErrorCategory -> HTTP status code mapping.</summary>
public static class ErrorCategoryHttpMapping
{
    /// <summary>Returns the HTTP status code an ErrorCategory maps to.</summary>
    public static int ToHttpStatusCode(ErrorCategory category) => category switch
    {
        ErrorCategory.Validation => 400,
        ErrorCategory.Unauthorized => 401,
        ErrorCategory.NotFound => 404,
        ErrorCategory.Conflict => 409,
        ErrorCategory.Dependency => 502,
        ErrorCategory.Internal => 500,
        _ => 500,
    };
}
