// ============================================================================
//  Project:        Shared.Authorization.Infrastructure
//  Module:         Governance
//  File:           TenantGovernanceRules.cs
//  Created:        2026-08-10
//  Description:    Phase 11 deliverable. Pure validation of a
//                  TenantContextResolution's shape and of a tenant-isolation
//                  decision's shape -- mirrors AuthenticationGovernanceRules
//                  (Phase 10) and AuthorizationGovernanceRules (Phase 9)
//                  exactly: static methods, no I/O, no side effects, returns
//                  a list of violation-name strings (empty when valid).
// ============================================================================
using Shared.Authorization.Abstractions.Tenant;

namespace Shared.Authorization.Infrastructure.Governance;

/// <summary>Pure validation rules for tenant resolution and tenant-isolation decisions.</summary>
public static class TenantGovernanceRules
{
    /// <summary>
    /// Validates <paramref name="resolution"/>. Reports "MissingTenantResolution" when null;
    /// "MissingTenantContext" when its TenantContext is null; "InvalidTenantContextShape" when
    /// TenantId or CorrelationId is empty; "UnrecognizedTenantKeyMetadataMissing" when a key was
    /// provided but not recognized and the context wasn't correctly reset to Unscoped.
    /// </summary>
    public static IReadOnlyList<string> ValidateResolution(TenantContextResolution? resolution)
    {
        var violations = new List<string>();

        if (resolution is null)
        {
            violations.Add("MissingTenantResolution");
            return violations;
        }

        if (resolution.Context is null)
        {
            violations.Add("MissingTenantContext");
            return violations;
        }

        if (string.IsNullOrWhiteSpace(resolution.Context.TenantId) || string.IsNullOrWhiteSpace(resolution.Context.CorrelationId))
        {
            violations.Add("InvalidTenantContextShape");
        }

        if (resolution.TenantKeyProvided && !resolution.TenantRecognized && resolution.Context.TenantId != TenantContext.UnscopedTenantId)
        {
            violations.Add("UnrecognizedTenantKeyMetadataMissing");
        }

        return violations;
    }

    /// <summary>
    /// Validates a bare TenantContext (no resolution metadata available), for use by domain-layer
    /// wrappers that only receive the context, not the resolution it came from. Reports
    /// "MissingTenantContext" when null; "InvalidTenantContextShape" when TenantId or
    /// CorrelationId is empty.
    /// </summary>
    public static IReadOnlyList<string> ValidateTenantContext(TenantContext? tenantContext)
    {
        var violations = new List<string>();

        if (tenantContext is null)
        {
            violations.Add("MissingTenantContext");
            return violations;
        }

        if (string.IsNullOrWhiteSpace(tenantContext.TenantId) || string.IsNullOrWhiteSpace(tenantContext.CorrelationId))
        {
            violations.Add("InvalidTenantContextShape");
        }

        return violations;
    }

    /// <summary>Validates that <paramref name="requestedFacilityId"/> is present whenever the decision claims a facility-boundary reason.</summary>
    public static IReadOnlyList<string> ValidateIsolationDecisionShape(TenantAccessDecision? decision, Guid? requestedFacilityId)
    {
        var violations = new List<string>();

        if (decision is null)
        {
            violations.Add("MissingTenantAccessDecision");
            return violations;
        }

        if (string.IsNullOrWhiteSpace(decision.Reason))
        {
            violations.Add("MissingTenantAccessDecisionReason");
        }

        if (!decision.IsAuthorized && requestedFacilityId is null)
        {
            violations.Add("DenialWithoutRequestedFacility");
        }

        return violations;
    }
}
