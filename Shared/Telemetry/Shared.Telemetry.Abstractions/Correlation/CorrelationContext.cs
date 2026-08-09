// ============================================================================
//  Project:        Shared.Telemetry
//  Module:         Abstractions.Correlation
//  File:           CorrelationContext.cs
//  Created:        2026-08-09
//  Description:    Mutable-once-then-read implementation of
//                  ICorrelationContext. CorrelationIdMiddleware calls Set
//                  exactly once, at the start of the request, before
//                  anything else runs; every other reader only calls the
//                  read-only CorrelationId property.
// ============================================================================
namespace Shared.Telemetry.Abstractions.Correlation;

/// <summary>Scoped, per-request correlation id holder.</summary>
public sealed class CorrelationContext : ICorrelationContext
{
    private string? _correlationId;

    /// <inheritdoc />
    public string CorrelationId => _correlationId ?? throw new InvalidOperationException(
        "CorrelationContext.Set(...) must be called (by CorrelationIdMiddleware) before CorrelationId is read.");

    /// <summary>Sets the correlation id for this request. Must be called exactly once, before any reader runs.</summary>
    public void Set(string correlationId) => _correlationId = correlationId;
}
