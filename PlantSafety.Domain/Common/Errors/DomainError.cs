// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Common
//  Layer:          Domain
//  File:           DomainError.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:
//      Lightweight, immutable error descriptor used in Result patterns.
//      Provides factory helpers for the canonical error categories:
//      NotFound, Validation, Conflict, Unauthorized.
//      Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Common.Errors;

/// <summary>Immutable error descriptor for domain operation results.</summary>
public sealed record DomainError(string Code, string Message, string Category)
{
    /// <summary>Creates a not-found error for <paramref name="entityName"/> with the given identity.</summary>
    public static DomainError NotFound(string entityName, string id)
        => new($"{entityName}.NotFound", $"{entityName} with id '{id}' was not found.", "NotFound");

    /// <summary>Creates a validation error for <paramref name="entityName"/>.</summary>
    public static DomainError Validation(string entityName, string message)
        => new($"{entityName}.Validation", message, "Validation");

    /// <summary>Creates a conflict error (e.g. duplicate, concurrency).</summary>
    public static DomainError Conflict(string entityName, string message)
        => new($"{entityName}.Conflict", message, "Conflict");

    /// <summary>Creates an invalid lifecycle transition error.</summary>
    public static DomainError InvalidTransition(string entityName, string from, string to)
        => new($"{entityName}.InvalidTransition",
               $"Cannot transition '{entityName}' from '{from}' to '{to}'.",
               "Lifecycle");

    /// <summary>Creates an unauthorized-action error.</summary>
    public static DomainError Unauthorized(string entityName, string action)
        => new($"{entityName}.Unauthorized", $"Actor is not authorised to perform '{action}' on '{entityName}'.", "Authorization");
}
