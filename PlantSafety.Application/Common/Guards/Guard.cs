// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Canonical guard clause helpers for Application layer pre-condition enforcement.
//   Guards replace defensive if/throw blocks with declarative, readable contracts.
//   All guards throw domain-safe exceptions — never ArgumentNullException or NullReferenceException.
//   Guards are used at command/handler boundaries; not inside domain objects.
// SchemaVersion: 1
// Additive-only evolution: new guard methods may be added; no existing methods may be removed or renamed.

using PlantSafety.Application.Common.Errors;

namespace PlantSafety.Application.Common.Guards;

/// <summary>
/// Canonical guard clause helpers for Application layer boundary enforcement.
/// All guard methods throw <see cref="DomainRuleException"/> on violation —
/// never <see cref="ArgumentNullException"/> or <see cref="ArgumentException"/>.
/// </summary>
public static class Guard
{
    /// <summary>
    /// Throws <see cref="DomainRuleException"/> if the value is null.
    /// </summary>
    /// <typeparam name="T">The reference type to check.</typeparam>
    /// <param name="value">The value to guard.</param>
    /// <param name="errorCode">The canonical error code.</param>
    /// <param name="message">The sanitized error message.</param>
    /// <param name="correlationId">The correlation identifier for lineage tracing.</param>
    /// <returns>The non-null value.</returns>
    /// <exception cref="DomainRuleException">Thrown when value is null.</exception>
    public static T NotNull<T>(T? value, string errorCode, string message, Guid correlationId)
        where T : class
    {
        if (value is null)
        {
            throw new DomainRuleException(errorCode, message, correlationId);
        }

        return value;
    }

    /// <summary>
    /// Throws <see cref="DomainRuleException"/> if the string is null, empty, or whitespace.
    /// </summary>
    /// <param name="value">The string to guard.</param>
    /// <param name="errorCode">The canonical error code.</param>
    /// <param name="message">The sanitized error message.</param>
    /// <param name="correlationId">The correlation identifier for lineage tracing.</param>
    /// <returns>The non-empty string (trimmed).</returns>
    /// <exception cref="DomainRuleException">Thrown when the string is null, empty, or whitespace.</exception>
    public static string NotNullOrWhiteSpace(string? value, string errorCode, string message, Guid correlationId)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainRuleException(errorCode, message, correlationId);
        }

        return value.Trim();
    }

    /// <summary>
    /// Throws <see cref="DomainRuleException"/> if the Guid is empty (<see cref="Guid.Empty"/>).
    /// </summary>
    /// <param name="value">The Guid to guard.</param>
    /// <param name="errorCode">The canonical error code.</param>
    /// <param name="message">The sanitized error message.</param>
    /// <param name="correlationId">The correlation identifier for lineage tracing.</param>
    /// <returns>The non-empty Guid.</returns>
    /// <exception cref="DomainRuleException">Thrown when the Guid is empty.</exception>
    public static Guid NotEmpty(Guid value, string errorCode, string message, Guid correlationId)
    {
        if (value == Guid.Empty)
        {
            throw new DomainRuleException(errorCode, message, correlationId);
        }

        return value;
    }

    /// <summary>
    /// Throws <see cref="DomainRuleException"/> if the integer is less than or equal to zero.
    /// </summary>
    /// <param name="value">The integer to guard.</param>
    /// <param name="errorCode">The canonical error code.</param>
    /// <param name="message">The sanitized error message.</param>
    /// <param name="correlationId">The correlation identifier for lineage tracing.</param>
    /// <returns>The positive integer value.</returns>
    /// <exception cref="DomainRuleException">Thrown when the integer is not positive.</exception>
    public static int Positive(int value, string errorCode, string message, Guid correlationId)
    {
        if (value <= 0)
        {
            throw new DomainRuleException(errorCode, message, correlationId);
        }

        return value;
    }

    /// <summary>
    /// Throws <see cref="DomainRuleException"/> if the DateTime is not UTC or is default.
    /// </summary>
    /// <param name="value">The DateTime to guard.</param>
    /// <param name="errorCode">The canonical error code.</param>
    /// <param name="message">The sanitized error message.</param>
    /// <param name="correlationId">The correlation identifier for lineage tracing.</param>
    /// <returns>The valid UTC DateTime.</returns>
    /// <exception cref="DomainRuleException">Thrown when the DateTime is not UTC or is default.</exception>
    public static DateTime UtcDateTime(DateTime value, string errorCode, string message, Guid correlationId)
    {
        if (value == default || value.Kind != DateTimeKind.Utc)
        {
            throw new DomainRuleException(errorCode, message, correlationId);
        }

        return value;
    }

    /// <summary>
    /// Throws <see cref="DomainRuleException"/> if the condition is false.
    /// </summary>
    /// <param name="condition">The condition that must be true.</param>
    /// <param name="errorCode">The canonical error code.</param>
    /// <param name="message">The sanitized error message.</param>
    /// <param name="correlationId">The correlation identifier for lineage tracing.</param>
    /// <exception cref="DomainRuleException">Thrown when the condition is false.</exception>
    public static void IsTrue(bool condition, string errorCode, string message, Guid correlationId)
    {
        if (!condition)
        {
            throw new DomainRuleException(errorCode, message, correlationId);
        }
    }
}
