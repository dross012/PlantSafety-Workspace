// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SharedKernel.Exceptions
//  Layer:          SharedKernel
//  File:           DomainException.cs
//  Created:        2026-06-17
//  SchemaVersion:  2026.1
//  Description:    Top-level domain exception alias. Delegates to the canonical
//                  Primitives.Exceptions.DomainException. Do not throw directly.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Primitives.Errors;
using PlantSafety.SharedKernel.Primitives.Exceptions;

namespace PlantSafety.SharedKernel.Exceptions;

// Re-export so callers can use PlantSafety.SharedKernel.Exceptions.DomainException
// without importing the deeper Primitives namespace.
/// <inheritdoc cref="PlantSafety.SharedKernel.Primitives.Exceptions.DomainException"/>
public sealed class DomainException : BaseException
{
    public DomainException(Error error, Exception? inner = null)
        : base(error, ExceptionSeverity.Error, inner) { }
}
