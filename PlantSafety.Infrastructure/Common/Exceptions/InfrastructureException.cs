// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Common
//  Layer:          Infrastructure
//  File:           InfrastructureException.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Base exception for all Infrastructure layer failures.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Common.Exceptions;

/// <summary>Base exception for all Infrastructure layer failures.</summary>
public abstract class InfrastructureException : Exception
{
    /// <summary>Unique identifier for this exception instance.</summary>
    public Guid ExceptionId { get; } = Uuid7.New();

    /// <summary>Machine-readable error code.</summary>
    public string ErrorCode { get; }

    /// <summary>Request correlation identifier.</summary>
    public string CorrelationId { get; }

    /// <summary>UTC instant at which this exception was created.</summary>
    public DateTimeOffset OccurredAtUtc { get; } = DateTimeOffset.UtcNow;

    /// <summary>Initialises with canonical infrastructure error fields.</summary>
    protected InfrastructureException(string message, string errorCode, string correlationId, Exception? inner = null)
        : base(message, inner)
    {
        ErrorCode = errorCode;
        CorrelationId = correlationId;
    }
}
