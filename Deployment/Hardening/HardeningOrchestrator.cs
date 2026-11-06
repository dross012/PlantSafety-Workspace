// ============================================================================
//  Project:        Deployment
//  Module:         Hardening
//  File:           HardeningOrchestrator.cs
//  Created:        2026-08-10
//  Description:    Phase 18 deliverable. Deterministic, pure validation of
//                  an already-collected set of host facts against the
//                  ../ServerBaseline.md checklist -- ValidateBaseline never
//                  itself inspects the host (that is the shell scripts'
//                  job); it evaluates whatever HardeningBaselineFacts the
//                  caller supplies, so it is fully unit-testable without a
//                  real machine. ApplyHardeningAsync is the separate, real
//                  host-invocation seam (runs the platform-appropriate
//                  idempotent script via System.Diagnostics.Process) --
//                  documented honestly as the one part of this file that
//                  cannot be exercised by a deterministic unit test.
// ============================================================================
using System.Diagnostics;

namespace Deployment.Hardening;

/// <summary>The deterministic set of host facts ValidateBaseline checks against ../ServerBaseline.md.</summary>
public sealed record HardeningBaselineFacts(
    bool FirewallEnabled,
    bool RemoteRootLoginDisabled,
    bool PasswordAuthenticationDisabled,
    bool AuditLoggingEnabled,
    bool AutomaticUpdatesEnabled,
    bool MinimumTlsVersionEnforced,
    bool GuestAccountDisabled);

/// <summary>The deterministic outcome of a single baseline check.</summary>
public sealed record HardeningCheckResult(string CheckName, bool Passed, string Detail);

/// <summary>The deterministic outcome of a full baseline validation pass.</summary>
public sealed record HardeningReport(IReadOnlyList<HardeningCheckResult> Checks, bool AllPassed, int CriticalDeviationCount);

/// <summary>Deterministic host-hardening baseline validation and (real, non-deterministic) hardening-script invocation.</summary>
public static class HardeningOrchestrator
{
    /// <summary>Validates <paramref name="facts"/> against every control in ../ServerBaseline.md. Same input always produces the same output.</summary>
    public static HardeningReport ValidateBaseline(HardeningBaselineFacts facts)
    {
        ArgumentNullException.ThrowIfNull(facts);

        var checks = new List<HardeningCheckResult>
        {
            new("FirewallEnabled", facts.FirewallEnabled, "Host firewall enabled, default-deny inbound."),
            new("RemoteRootLoginDisabled", facts.RemoteRootLoginDisabled, "Remote root/Administrator login disabled."),
            new("PasswordAuthenticationDisabled", facts.PasswordAuthenticationDisabled, "Password authentication disabled (key-only)."),
            new("AuditLoggingEnabled", facts.AuditLoggingEnabled, "Audit logging enabled."),
            new("AutomaticUpdatesEnabled", facts.AutomaticUpdatesEnabled, "Automatic security updates enabled."),
            new("MinimumTlsVersionEnforced", facts.MinimumTlsVersionEnforced, "Minimum TLS 1.2 enforced."),
            new("GuestAccountDisabled", facts.GuestAccountDisabled, "Guest/anonymous account disabled."),
        };

        int criticalDeviationCount = checks.Count(check => !check.Passed);

        return new HardeningReport(checks, criticalDeviationCount == 0, criticalDeviationCount);
    }

    /// <summary>
    /// Runs the idempotent hardening script at <paramref name="scriptPath"/> with
    /// <paramref name="arguments"/> and returns its exit code. The real, non-deterministic host
    /// invocation seam -- not exercised by unit tests, documented honestly as such.
    /// </summary>
    public static async Task<int> ApplyHardeningAsync(string scriptPath, IReadOnlyList<string> arguments, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(scriptPath);
        ArgumentNullException.ThrowIfNull(arguments);

        var startInfo = new ProcessStartInfo(scriptPath)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };

        foreach (string argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        using Process process = Process.Start(startInfo) ?? throw new InvalidOperationException($"Failed to start hardening script \"{scriptPath}\".");
        await process.WaitForExitAsync(cancellationToken);
        return process.ExitCode;
    }
}
