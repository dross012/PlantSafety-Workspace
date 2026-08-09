// ============================================================================
//  Project:        Shared.SqlGateway
//  Module:         Infrastructure.Resilience
//  File:           SqlCircuitBreaker.cs
//  Created:        2026-08-09
//  Description:    Phase 3.6 deliverable: a distinct CircuitBreaker
//                  instance dedicated to SQL Server calls. CircuitBreaker
//                  (Phase 2.3) is deliberately "per-dependency" by design
//                  (see its own header comment) -- PlantSafety.UI.Api
//                  already registers one plain CircuitBreaker singleton
//                  for GlobalErrorHandler's whole-API failure tracking.
//                  Wrapping a second CircuitBreaker in this distinct type
//                  (rather than registering a second unkeyed CircuitBreaker,
//                  which DI would resolve ambiguously) keeps a SQL Server
//                  outage from tripping the same circuit that every other,
//                  unrelated module's error handling reads.
// ============================================================================
using Shared.Deterministic.Abstractions.Clock;
using Shared.ErrorHandling.Abstractions.Policy;

namespace Shared.SqlGateway.Infrastructure.Resilience;

/// <summary>Wraps a CircuitBreaker instance dedicated to SQL Server calls, distinct from any other dependency's.</summary>
public sealed class SqlCircuitBreaker(IClock clock)
{
    /// <summary>The underlying circuit breaker, using the standard policy (5 consecutive failures trips it).</summary>
    public CircuitBreaker Breaker { get; } = new(CircuitBreakerPolicy.Standard, clock);
}
