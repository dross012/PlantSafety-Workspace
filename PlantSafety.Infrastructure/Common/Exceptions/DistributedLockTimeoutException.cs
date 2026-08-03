// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Common
//  Layer:          Infrastructure
//  File:           DistributedLockTimeoutException.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code)
//  SchemaVersion:  2026.1
//  Description:    Thrown by IDistributedLockProvider.AcquireAsync when an exclusive distributed
//                  lock could not be obtained for a resource within the caller-supplied timeout.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Common.Exceptions;

/// <summary>
/// Thrown when a distributed lock could not be acquired for a resource within the requested
/// timeout. Callers that prefer non-throwing behavior should use
/// <c>IDistributedLockProvider.TryAcquireAsync</c> instead.
/// </summary>
public sealed class DistributedLockTimeoutException : InfrastructureException
{
    /// <summary>The resource key the lock was requested for.</summary>
    public string ResourceKey { get; }

    /// <summary>The timeout that elapsed without the lock being granted.</summary>
    public TimeSpan Timeout { get; }

    /// <summary>Creates a distributed lock timeout exception for the given resource key.</summary>
    public DistributedLockTimeoutException(string resourceKey, TimeSpan timeout, string correlationId = "")
        : base(
            $"Failed to acquire a distributed lock for resource '{resourceKey}' within {timeout}.",
            "Concurrency.DistributedLockTimeout",
            correlationId)
    {
        ResourceKey = resourceKey;
        Timeout = timeout;
    }
}
