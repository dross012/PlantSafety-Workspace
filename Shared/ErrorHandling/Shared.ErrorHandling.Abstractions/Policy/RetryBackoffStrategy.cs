// ============================================================================
//  Project:        Shared.ErrorHandling
//  Module:         Abstractions.Policy
//  File:           RetryBackoffStrategy.cs
//  Created:        2026-08-09
// ============================================================================
namespace Shared.ErrorHandling.Abstractions.Policy;

/// <summary>How the delay between retry attempts grows.</summary>
public enum RetryBackoffStrategy
{
    /// <summary>The same delay every attempt.</summary>
    Fixed,

    /// <summary>Delay doubles with every attempt (BaseDelay * 2^(attempt-1)).</summary>
    Exponential,
}
