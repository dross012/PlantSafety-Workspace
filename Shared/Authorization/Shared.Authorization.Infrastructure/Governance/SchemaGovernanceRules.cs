// ============================================================================
//  Project:        Shared.Authorization.Infrastructure
//  Module:         Governance
//  File:           SchemaGovernanceRules.cs
//  Created:        2026-08-10
//  Description:    Phase 12 deliverable. The composing rule set the prompt's
//                  "Unified Governance" section (1.6) asks for: one pure
//                  function that runs the identity/contract/tenant
//                  governance rules already built across Phases 9-12
//                  together, instead of each caller invoking four
//                  evaluators separately. Does not duplicate any of
//                  AuthenticationGovernanceRules (Phase 10),
//                  TenantGovernanceRules (Phase 11), or
//                  ContractValidationRules (Phase 12, this file's own
//                  project) -- it calls them.
//
//                  Error-path schema governance (ErrorGovernanceRules,
//                  Shared.ErrorHandling.Infrastructure) is deliberately NOT
//                  composed in here: it validates ErrorEnvelope shapes at
//                  the exception-handling boundary (GlobalErrorHandler),
//                  a different lifecycle from the success-path request
//                  metadata this evaluator validates. Composing them would
//                  require Shared.Authorization to reference
//                  Shared.ErrorHandling for a check that never runs on the
//                  same request path as this one.
// ============================================================================
using Shared.Authorization.Abstractions.Caller;
using Shared.Authorization.Abstractions.Contracts;
using Shared.Authorization.Abstractions.Tenant;

namespace Shared.Authorization.Infrastructure.Governance;

/// <summary>Composes the request-path identity/contract/tenant governance rules into one validation pass.</summary>
public static class SchemaGovernanceRules
{
    /// <summary>
    /// Validates <paramref name="callerResolution"/>, <paramref name="tenantResolution"/>, and
    /// <paramref name="contractMetadata"/> together, returning the concatenation of every
    /// violation any of the three underlying rule sets reports.
    /// </summary>
    public static IReadOnlyList<string> ValidateAll(
        CallerContextResolution? callerResolution,
        TenantContextResolution? tenantResolution,
        ContractMetadata? contractMetadata)
    {
        var violations = new List<string>();
        violations.AddRange(AuthenticationGovernanceRules.ValidateResolution(callerResolution));
        violations.AddRange(TenantGovernanceRules.ValidateResolution(tenantResolution));
        violations.AddRange(ContractValidationRules.Validate(contractMetadata));
        return violations;
    }
}
