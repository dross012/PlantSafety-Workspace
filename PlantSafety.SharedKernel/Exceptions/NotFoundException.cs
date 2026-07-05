// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SharedKernel.Exceptions
//  Layer:          SharedKernel
//  File:           NotFoundException.cs
//  Created:        2026-06-17
//  SchemaVersion:  2026.1
//  Description:    Raised when a requested resource could not be found.
//                  Maps to HTTP 404. SIEM-safe: no PII in message.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Primitives.Errors;
using PlantSafety.SharedKernel.Primitives.Exceptions;

namespace PlantSafety.SharedKernel.Exceptions;

/// <summary>
/// Raised when a requested aggregate or resource cannot be found.
/// Maps to HTTP 404 Not Found at the API boundary.
/// </summary>
public sealed class NotFoundException : BaseException
{
    public NotFoundException(string entityType, string id)
        : base(
            new Error(
                ErrorCode.Create($"{entityType}.not_found"),
                ErrorType.NotFound,
                $"The requested {entityType} was not found."),
            ExceptionSeverity.Warning)
    { }

    public NotFoundException(Error error, Exception? inner = null)
        : base(error, ExceptionSeverity.Warning, inner) { }
}
