/*==========================================================================================================================
    AUTHOR:             PlantSafety Intelligence Architecture Group
    CREATED:            2026-06-14
    FILE:               IntelligenceDomainException.cs
    DESCRIPTION:        Domain exception for invariant violations within the Intelligence bounded context.
                        Carries a domain-safe error code (no infrastructure details), a correlation ID, and
                        a causation ID for distributed tracing. Never expose stack traces, connection strings,
                        server names, or account details in the message. (SIEM-safe rule)

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

namespace Intelligence.Domain.Shared.Exceptions;

/// <summary>
/// Raised when a domain invariant is violated within the Intelligence bounded context.
/// The <see cref="ErrorCode"/> is a SCREAMING_SNAKE_CASE identifier safe for logging and SIEM ingestion.
/// </summary>
public sealed class IntelligenceDomainException : Exception
{
    /// <summary>A SCREAMING_SNAKE_CASE error code identifying the violated invariant (e.g., "INVESTIGATION_TERMINAL").</summary>
    public string ErrorCode { get; }

    /// <summary>Propagated correlation identifier for distributed tracing.</summary>
    public Guid CorrelationId { get; }

    /// <summary>Causation identifier linking this exception to the triggering command or event.</summary>
    public Guid CausationId { get; }

    /// <summary>
    /// Initialises a new <see cref="IntelligenceDomainException"/>.
    /// </summary>
    /// <param name="errorCode">A SCREAMING_SNAKE_CASE error code. Must not contain infrastructure details or PII.</param>
    /// <param name="message">A human-readable invariant violation message. No secrets, stack traces, or connection strings.</param>
    /// <param name="correlationId">The correlation ID from the triggering command.</param>
    /// <param name="causationId">The causation ID from the triggering command or event.</param>
    public IntelligenceDomainException(
        string errorCode,
        string message,
        Guid correlationId,
        Guid causationId)
        : base(message)
    {
        ErrorCode = errorCode;
        CorrelationId = correlationId;
        CausationId = causationId;
    }

    /// <summary>
    /// Initialises a new <see cref="IntelligenceDomainException"/> with an inner exception.
    /// </summary>
    public IntelligenceDomainException(
        string errorCode,
        string message,
        Guid correlationId,
        Guid causationId,
        Exception innerException)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
        CorrelationId = correlationId;
        CausationId = causationId;
    }
}
