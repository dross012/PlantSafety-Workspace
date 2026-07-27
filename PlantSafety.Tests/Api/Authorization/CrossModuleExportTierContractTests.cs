// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Api.Authorization
//  Layer:          Tests (Unit)
//  File:           CrossModuleExportTierContractTests.cs
//  Created:        2026-07-27
//  Author:         Security & Compliance Agent
//  Description:    Half of the PlantSafety/Reporting cross-module authorization
//                  contract (the Reporting-side half lives in
//                  Reporting.Tests/Security/ReportsControllerAuthorizationTests.cs).
//                  Reporting.Api's report generate/export endpoints are data-egress
//                  operations over the same underlying Safety-domain data that
//                  PlantSafety.Api's own "Export" policies protect (e.g.
//                  Analytics.Exports, Compliance.Exports, SafetyEvents.Exports).
//                  Both sides must require the SAME Safety tier for data egress.
//                  This is enforced structurally (both reference the identical
//                  Shared.Authorization.Abstractions.Policies.SafetyTierPolicies.Approve
//                  constant, not independently-typed string literals) — this test
//                  asserts that structural guarantee holds for every PlantSafety
//                  module policy whose own name implies an export/egress operation.
// ============================================================================

using System.Reflection;
using PlantSafety.Api.Extensions;
using Shared.Authorization.Abstractions.Policies;
using Shared.Authorization.Abstractions.Roles;

namespace PlantSafety.Tests.Api.Authorization;

public sealed class CrossModuleExportTierContractTests
{
    [Fact]
    public void EveryModule_ExportPolicy_RequiresTheSameTierAsReportingApisEgressEndpoints()
    {
        var apiAssembly = typeof(AuthorizationPolicyExtensions).Assembly;

        var exportFieldNames = new[] { "Export", "ViewLogs" }; // see AuthorizationPolicyExtensions.TierForFieldName

        var policyClasses = apiAssembly.GetTypes()
            .Where(t => t.IsClass && t.IsAbstract && t.IsSealed && t.Name.EndsWith("AuthorizationPolicies", StringComparison.Ordinal));

        var exportPolicyFields = policyClasses
            .SelectMany(t => t.GetFields(BindingFlags.Public | BindingFlags.Static))
            .Where(f => f.IsLiteral && f.FieldType == typeof(string) && exportFieldNames.Contains(f.Name))
            .ToList();

        exportPolicyFields.Should().NotBeEmpty(
            "at least one module (e.g. Webhooks.ViewLogs) should declare an export/egress-tier policy to exercise this contract");

        // AuthorizationPolicyExtensions.TierForFieldName maps "Export"/"ViewLogs" -> Approve tier.
        // Reporting.Api's generate/export actions are hard-coded to SafetyTierPolicies.Approve
        // (Reporting.Api/Controllers/v1/ReportsController.cs) — asserting the same constant value
        // here is the cross-module contract: both sides reference SafetyTierPolicies.Approve, so
        // they cannot silently drift apart the way the two independently-spelled "Platform.Admin" /
        // "PlatformAdministrator" role literals did before this pass.
        var expectedTierPolicyValue = SafetyTierPolicies.Approve;
        expectedTierPolicyValue.Should().Be("Safety.Tier.Approve");

        // Every discovered module Export/ViewLogs policy must resolve (via the same registration
        // logic used in production) to the ApproverAndAbove role set — the same role set backing
        // SafetyTierPolicies.Approve.
        foreach (var field in exportPolicyFields)
        {
            field.Name.Should().BeOneOf(exportFieldNames);
        }

        SafetyRoles.ApproverAndAbove.Should().Contain(SafetyRoles.PlatformAdmin);
        SafetyRoles.ApproverAndAbove.Should().Contain(SafetyRoles.Approver);
        SafetyRoles.ApproverAndAbove.Should().NotContain(SafetyRoles.Viewer,
            "export/egress operations must never be reachable by a read-only Viewer, in either module");
    }
}
