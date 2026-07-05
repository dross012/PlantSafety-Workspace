// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SharedKernel.Exceptions
//  Layer:          SharedKernel
//  File:           ForbiddenException.cs
//  Created:        2026-06-17
//  SchemaVersion:  2026.1
//  Description:    Raised when the caller is authenticated but lacks permission.
//                  Maps to HTTP 403. Never expose the reason to the caller.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Primitives.Errors;
using PlantSafety.SharedKernel.Primitives.Exceptions;

namespace PlantSafety.SharedKernel.Exceptions;

/// <summary>
/// Raised when the caller lacks permission for the requested operation.
/// Maps to HTTP 403 Forbidden.
/// SIEM-safe: the response MUST NOT reveal the specific permission that was denied.
/// </summary>
public sealed class ForbiddenException : BaseException
{
    public ForbiddenException()
        : base(
            new Error(
                ErrorCode.Create("access.forbidden"),
                ErrorType.Forbidden,
                "You do not have permission to perform this action."),
            ExceptionSeverity.Warning)
    { }

    public ForbiddenException(Error error, Exception? inner = null)
        : base(error, ExceptionSeverity.Warning, inner) { }
}
