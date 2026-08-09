// ============================================================================
//  Project:        Shared.ErrorHandling
//  Module:         Abstractions.Policy
//  File:           CircuitBreakerPolicy.cs
//  Created:        2026-08-09
// ============================================================================
namespace Shared.ErrorHandling.Abstractions.Policy;

/// <summary>Circuit breaker configuration: how many consecutive failures trip it, and how long it stays open.</summary>
public sealed record CircuitBreakerPolicy(int FailureThreshold, TimeSpan OpenDuration)
{
    /// <summary>The standard circuit breaker policy: trips after 5 consecutive failures, stays open 30 seconds.</summary>
    public static CircuitBreakerPolicy Standard { get; } = new(FailureThreshold: 5, TimeSpan.FromSeconds(30));
}
