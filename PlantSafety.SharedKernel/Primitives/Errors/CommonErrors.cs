/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-27
    FILE:               CommonErrors.cs
    DESCRIPTION:        Canonical set of common errors used across all modules. MUST remain additive-only. Each error MUST
                        use normalized error codes and SIEM-safe messages.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only. New errors MAY be added; existing ones MUST NOT change semantics.
==========================================================================================================================*/

namespace PlantSafety.SharedKernel.Primitives.Errors;

/// <summary>
/// Canonical set of common errors.
/// </summary>
public static class CommonErrors
{
    public static readonly Error NullValue =
        new(ErrorCode.Create("validation.null"), ErrorType.Validation, "A required value was null.");

    public static readonly Error InvalidFormat =
        new(ErrorCode.Create("validation.format"), ErrorType.Validation, "The value format is invalid.");

    public static readonly Error NotFound =
        new(ErrorCode.Create("resource.notfound"), ErrorType.NotFound, "The requested resource was not found.");

    public static readonly Error Conflict =
        new(ErrorCode.Create("resource.conflict"), ErrorType.Conflict, "A conflicting resource already exists.");

    public static readonly Error Unauthorized =
        new(ErrorCode.Create("auth.unauthorized"), ErrorType.Unauthorized, "Authentication is required.");

    public static readonly Error Forbidden =
        new(ErrorCode.Create("auth.forbidden"), ErrorType.Forbidden, "You do not have permission to perform this action.");

    // Newly added (2026 canonical)
    public static readonly Error MissingRequiredField =
        new(ErrorCode.Create("validation.required"), ErrorType.Validation, "A required field was missing or empty.");

    public static readonly Error ConcurrencyConflict =
        new(ErrorCode.Create("concurrency.conflict"), ErrorType.Concurrency, "A concurrency conflict occurred.");
}
