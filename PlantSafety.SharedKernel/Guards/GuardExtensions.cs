// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SharedKernel.Guards
//  Layer:          SharedKernel
//  File:           GuardExtensions.cs
//  Created:        2026-06-17
//  SchemaVersion:  2026.1
//  Description:    Fluent guard extension methods for common invariant patterns.
//                  All methods are pure and throw on violation. Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Primitives.Errors;
using PlantSafety.SharedKernel.Primitives.Exceptions;

namespace PlantSafety.SharedKernel.Guards;

/// <summary>Fluent guard extension methods for common invariant checks.</summary>
public static class GuardExtensions
{
    /// <summary>Throws <see cref="DomainException"/> if <paramref name="value"/> is null.</summary>
    public static T NotNull<T>(this T? value, string paramName, Error? error = null)
        where T : class
    {
        if (value is null)
            throw new DomainException(error ?? CommonErrors.NullValue);
        return value;
    }

    /// <summary>Throws <see cref="DomainException"/> if the string is null or whitespace.</summary>
    public static string NotNullOrWhiteSpace(this string? value, string paramName, Error? error = null)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException(error ?? CommonErrors.NullValue);
        return value;
    }

    /// <summary>Throws <see cref="DomainException"/> if the Guid is empty.</summary>
    public static Guid NotEmpty(this Guid value, string paramName, Error? error = null)
    {
        if (value == Guid.Empty)
            throw new DomainException(
                error ?? new Error(ErrorCode.Create($"{paramName}.empty"),
                                   ErrorType.Validation,
                                   $"{paramName} must not be Guid.Empty."));
        return value;
    }

    /// <summary>
    /// Throws <see cref="DomainException"/> if <paramref name="value"/> is not
    /// greater than <paramref name="min"/>.
    /// </summary>
    public static int GreaterThan(this int value, int min, string paramName, Error? error = null)
    {
        if (value <= min)
            throw new DomainException(
                error ?? new Error(ErrorCode.Create($"{paramName}.out_of_range"),
                                   ErrorType.Validation,
                                   $"{paramName} must be greater than {min}."));
        return value;
    }
}
