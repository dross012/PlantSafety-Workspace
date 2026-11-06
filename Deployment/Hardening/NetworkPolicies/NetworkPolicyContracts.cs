// ============================================================================
//  Project:        Deployment
//  Module:         Hardening.NetworkPolicies
//  File:           NetworkPolicyContracts.cs
//  Created:        2026-08-10
//  Description:    Phase 18 deliverable. Strongly typed, deterministic
//                  network policy manifests: the explicit allow-list of
//                  inbound/outbound rules a tenant's compute pool is bound
//                  to. NetworkPolicyValidator performs pure, explicit
//                  field validation -- no reflection.
// ============================================================================
using System.Text.Json.Serialization;

namespace Deployment.Hardening.NetworkPolicies;

/// <summary>The deterministic transport protocol a network rule applies to.</summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum NetworkProtocol
{
    /// <summary>TCP.</summary>
    Tcp,

    /// <summary>UDP.</summary>
    Udp,
}

/// <summary>A single, deterministic network port rule.</summary>
public sealed record NetworkPortRule(
    int Port,
    NetworkProtocol Protocol,
    string Description,
    IReadOnlyList<string> AllowedSourceCidrs);

/// <summary>A tenant's full, deterministic network policy manifest.</summary>
public sealed record NetworkPolicyManifest(
    string TenantId,
    IReadOnlyList<NetworkPortRule> AllowedInboundRules,
    IReadOnlyList<NetworkPortRule> AllowedOutboundRules);

/// <summary>Pure, deterministic, explicit validation of a NetworkPolicyManifest.</summary>
public static class NetworkPolicyValidator
{
    /// <summary>Validates <paramref name="manifest"/>. Returns every violation found (empty if valid).</summary>
    public static IReadOnlyList<string> Validate(NetworkPolicyManifest manifest)
    {
        ArgumentNullException.ThrowIfNull(manifest);

        var violations = new List<string>();

        if (string.IsNullOrWhiteSpace(manifest.TenantId))
        {
            violations.Add("NetworkPolicyManifest.TenantId is missing or empty.");
        }

        violations.AddRange(ValidateRules(manifest.AllowedInboundRules, "AllowedInboundRules"));
        violations.AddRange(ValidateRules(manifest.AllowedOutboundRules, "AllowedOutboundRules"));

        return violations;
    }

    private static IReadOnlyList<string> ValidateRules(IReadOnlyList<NetworkPortRule> rules, string ruleSetName)
    {
        var violations = new List<string>();
        var seenRuleKeys = new HashSet<(int Port, NetworkProtocol Protocol)>();

        foreach (NetworkPortRule rule in rules)
        {
            if (rule.Port is < 1 or > 65535)
            {
                violations.Add($"{ruleSetName}: port {rule.Port} is out of the valid range 1-65535.");
            }

            if (rule.AllowedSourceCidrs.Count == 0)
            {
                violations.Add($"{ruleSetName}: rule for port {rule.Port}/{rule.Protocol} has no AllowedSourceCidrs -- an empty allow-list is not the same as deny-all and must be explicit.");
            }

            if (!seenRuleKeys.Add((rule.Port, rule.Protocol)))
            {
                violations.Add($"{ruleSetName}: duplicate rule for port {rule.Port}/{rule.Protocol}.");
            }
        }

        return violations;
    }
}
