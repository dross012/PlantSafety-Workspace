// ============================================================================
//  Project:        Shared.ErrorHandling
//  Module:         Abstractions.Envelope
//  File:           ErrorCategory.cs
//  Created:        2026-08-09
//  Description:    Phase 2.3 (Deterministic Error Envelope Standardization):
//                  the closed set of error categories every module's
//                  errors are classified into. Deliberately small and
//                  closed (not an open string) so classification stays
//                  deterministic and every category has one, and only
//                  one, HTTP status mapping (see ErrorCategoryHttpMapping).
// ============================================================================
namespace Shared.ErrorHandling.Abstractions.Envelope;

/// <summary>The closed set of error categories an ErrorEnvelope can carry.</summary>
public enum ErrorCategory
{
    /// <summary>The request itself was malformed or failed validation.</summary>
    Validation,

    /// <summary>The requested resource does not exist.</summary>
    NotFound,

    /// <summary>The request conflicts with the current state (e.g. an invalid transition).</summary>
    Conflict,

    /// <summary>The caller is not authorized to perform this action.</summary>
    Unauthorized,

    /// <summary>A downstream dependency (another service, the database) failed or was unreachable.</summary>
    Dependency,

    /// <summary>An unexpected internal failure with no more specific category.</summary>
    Internal,
}
