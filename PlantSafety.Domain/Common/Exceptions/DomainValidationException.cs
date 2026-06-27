// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Common
//  Layer:          Domain
//  File:           DomainValidationException.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:
//      Thrown when domain invariant validation fails. Maps to HTTP 422 at the
//      API layer. Carries the list of individual validation failures.
//      Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Common.Exceptions;

/// <summary>Thrown when one or more domain invariants are violated.</summary>
public class DomainValidationException : DomainException
{
    /// <summary>Individual validation errors that contributed to this exception.</summary>
    public IReadOnlyList<string> Errors { get; }

    /// <summary>Creates a validation exception with one or more error messages.</summary>
    public DomainValidationException(
        string entityName,
        IReadOnlyList<string> errors,
        CorrelationId correlationId,
        CausationId causationId)
        : base(
            $"{entityName} validation failed: {string.Join("; ", errors)}",
            $"{entityName}.ValidationFailed",
            "Validation",
            "Low",
            correlationId,
            causationId)
    {
        Errors = errors;
    }
}
