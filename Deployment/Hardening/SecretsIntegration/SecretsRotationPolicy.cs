// ============================================================================
//  Project:        Deployment
//  Module:         Hardening.SecretsIntegration
//  File:           SecretsRotationPolicy.cs
//  Created:        2026-08-10
//  Description:    Phase 18 deliverable. Deterministic, pure secret-rotation
//                  due/grace-period evaluation. Every check is a plain
//                  TimeSpan comparison against a supplied "now" -- no
//                  IClock.UtcNow read internally, so the same
//                  (policy, lastRotatedAtUtc, nowUtc) triple always
//                  produces the same answer.
// ============================================================================
namespace Deployment.Hardening.SecretsIntegration;

/// <summary>A deterministic secret rotation policy: how often a secret must rotate, and how long a grace period follows a missed rotation before it is a hard failure.</summary>
public sealed record SecretsRotationPolicy(string SecretId, TimeSpan RotationInterval, TimeSpan GracePeriod);

/// <summary>Deterministic, pure secret-rotation due/grace-period evaluation.</summary>
public static class SecretsRotationPolicyEvaluator
{
    /// <summary>Returns true if <paramref name="policy"/>'s RotationInterval has elapsed since <paramref name="lastRotatedAtUtc"/>, as of <paramref name="nowUtc"/>.</summary>
    public static bool IsRotationDue(SecretsRotationPolicy policy, DateTime lastRotatedAtUtc, DateTime nowUtc)
    {
        ArgumentNullException.ThrowIfNull(policy);
        return nowUtc - lastRotatedAtUtc >= policy.RotationInterval;
    }

    /// <summary>Returns true if <paramref name="nowUtc"/> is still within RotationInterval + GracePeriod of <paramref name="lastRotatedAtUtc"/> -- a due-but-not-yet-critical rotation.</summary>
    public static bool IsWithinGracePeriod(SecretsRotationPolicy policy, DateTime lastRotatedAtUtc, DateTime nowUtc)
    {
        ArgumentNullException.ThrowIfNull(policy);
        return nowUtc - lastRotatedAtUtc <= policy.RotationInterval + policy.GracePeriod;
    }

    /// <summary>Returns true if rotation is due AND the grace period has also elapsed -- a hard, must-rotate-now failure.</summary>
    public static bool IsRotationOverdue(SecretsRotationPolicy policy, DateTime lastRotatedAtUtc, DateTime nowUtc)
    {
        return IsRotationDue(policy, lastRotatedAtUtc, nowUtc) && !IsWithinGracePeriod(policy, lastRotatedAtUtc, nowUtc);
    }
}
