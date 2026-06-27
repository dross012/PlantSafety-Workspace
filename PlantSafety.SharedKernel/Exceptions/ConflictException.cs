// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SharedKernel.Exceptions
//  Layer:          SharedKernel
//  File:           ConflictException.cs
//  Created:        2026-06-17
//  SchemaVersion:  2026.1
//  Description:    Raised when a write operation conflicts with existing state
//                  (e.g., duplicate key, optimistic concurrency violation).
//                  Maps to HTTP 409. Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Primitives.Errors;
using PlantSafety.SharedKernel.Primitives.Exceptions;

namespace PlantSafety.SharedKernel.Exceptions;

/// <summary>
/// Raised when a write conflicts with existing state.
/// Maps to HTTP 409 Conflict at the API boundary.
/// </summary>
public sealed class ConflictException : BaseException
{
    public ConflictException(string entityType)
        : base(
            new Error(
                new ErrorCode($"{entityType.ToUpperInvariant()}_CONFLICT"),
                ErrorType.Conflict,
                $"A conflict occurred with the requested {entityType}."),
            ExceptionSeverity.Warning)
    { }

    public ConflictException(Error error, Exception? inner = null)
        : base(error, ExceptionSeverity.Warning, inner) { }
}
