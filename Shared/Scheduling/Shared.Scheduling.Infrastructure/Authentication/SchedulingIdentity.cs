// ============================================================================
//  Project:        Shared.Scheduling.Infrastructure
//  Module:         Authentication
//  File:           SchedulingIdentity.cs
//  Created:        2026-08-10
//  Description:    Phase 14 (Scheduling Modernization) deliverable.
//                  Scheduling's own adapter over the real Phase 10/11
//                  identity resolution seam (CallerContextFactory/
//                  TenantContextFactory, Shared.Authorization), for call
//                  sites that reach SchedulingOrchestrator without already
//                  having gone through PlantSafety.UI.Api's shared
//                  ApiKeyAuthenticationMiddleware (e.g. a future background
//                  scheduling job or SchedulingController with its own
//                  request pipeline). Performs the exact same two-step
//                  resolution ApiKeyAuthenticationMiddleware.cs's own header
//                  comment documents (CallerContext then TenantContext),
//                  scoped to Module "Scheduling" -- does not invent a new
//                  identity/API-key format, does not bypass
//                  CallerContextFactory/TenantContextFactory.
//
//                  Like WorkflowEngineTelemetry/RoutingTelemetry before it,
//                  this adapter has no live caller in PlantSafety.UI.Api
//                  today (no SchedulingController is registered) -- real,
//                  tested, wired capability, documented honestly as dormant
//                  rather than pretended live.
// ============================================================================
using Shared.Authorization.Abstractions.Caller;
using Shared.Authorization.Abstractions.Identity;
using Shared.Authorization.Abstractions.Roles;
using Shared.Authorization.Abstractions.Tenant;
using Shared.Authorization.Infrastructure.Governance;
using Shared.Authorization.Infrastructure.Telemetry;
using Shared.Deterministic.Abstractions.Clock;
using Shared.Telemetry.Abstractions.Events;

namespace Shared.Scheduling.Infrastructure.Authentication;

/// <summary>The resolved caller and tenant identity for a single Scheduling request.</summary>
public sealed record SchedulingIdentityResolution(CallerContext Caller, TenantContext TenantContext);

/// <summary>Resolves a raw API-key header value into a CallerContext + TenantContext for Scheduling call sites, emitting authentication telemetry and governance.</summary>
public static class SchedulingIdentity
{
    /// <summary>
    /// Resolves <paramref name="apiKeyHeaderValue"/> against <paramref name="registeredApiKeys"/> and
    /// <paramref name="registeredKeyTenants"/> via the real CallerContextFactory/TenantContextFactory,
    /// recording authentication telemetry and governance for both resolutions.
    /// </summary>
    public static SchedulingIdentityResolution Resolve(
        string? apiKeyHeaderValue,
        IReadOnlyDictionary<string, EnterpriseRole> registeredApiKeys,
        IReadOnlyDictionary<string, string> registeredKeyTenants,
        string correlationId,
        IClock clock,
        ILogEventWriter logEventWriter)
    {
        ArgumentNullException.ThrowIfNull(registeredApiKeys);
        ArgumentNullException.ThrowIfNull(registeredKeyTenants);
        ArgumentNullException.ThrowIfNull(clock);
        ArgumentNullException.ThrowIfNull(logEventWriter);

        CallerContextResolution callerResolution = CallerContextFactory.Create(apiKeyHeaderValue, registeredApiKeys, correlationId);
        AuthenticationTelemetry.RecordResolution(callerResolution, clock, logEventWriter);
        AuthenticationGovernanceEvaluator.EvaluateAndReport(callerResolution, clock, logEventWriter);

        TenantContextResolution tenantResolution = TenantContextFactory.Create(apiKeyHeaderValue, registeredKeyTenants, callerResolution.Caller, correlationId);
        TenantTelemetry.RecordResolution(tenantResolution, clock, logEventWriter);
        TenantGovernanceEvaluator.EvaluateResolutionAndReport(tenantResolution, clock, logEventWriter);

        return new SchedulingIdentityResolution(callerResolution.Caller, tenantResolution.Context);
    }
}
