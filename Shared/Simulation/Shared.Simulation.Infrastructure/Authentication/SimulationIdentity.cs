// ============================================================================
//  Project:        Shared.Simulation.Infrastructure
//  Module:         Authentication
//  File:           SimulationIdentity.cs
//  Created:        2026-08-10
//  Description:    Phase 17 deliverable. Simulation's own adapter over the
//                  real Phase 10/11 identity resolution seam
//                  (CallerContextFactory/TenantContextFactory, Shared.
//                  Authorization), mirroring SafetyGraphIdentity (Phase 16)
//                  exactly, scoped to Module "Simulation". Has no live
//                  caller in PlantSafety.UI.Api today (no
//                  SimulationController is registered) -- real, tested,
//                  wired capability, documented honestly as dormant rather
//                  than pretended live.
// ============================================================================
using Shared.Authorization.Abstractions.Caller;
using Shared.Authorization.Abstractions.Identity;
using Shared.Authorization.Abstractions.Roles;
using Shared.Authorization.Abstractions.Tenant;
using Shared.Authorization.Infrastructure.Governance;
using Shared.Authorization.Infrastructure.Telemetry;
using Shared.Deterministic.Abstractions.Clock;
using Shared.Telemetry.Abstractions.Events;

namespace Shared.Simulation.Infrastructure.Authentication;

/// <summary>The resolved caller and tenant identity for a single Simulation request.</summary>
public sealed record SimulationIdentityResolution(CallerContext Caller, TenantContext TenantContext);

/// <summary>Resolves a raw API-key header value into a CallerContext + TenantContext for Simulation call sites, emitting authentication telemetry and governance.</summary>
public static class SimulationIdentity
{
    /// <summary>
    /// Resolves <paramref name="apiKeyHeaderValue"/> against <paramref name="registeredApiKeys"/> and
    /// <paramref name="registeredKeyTenants"/> via the real CallerContextFactory/TenantContextFactory,
    /// recording authentication telemetry and governance for both resolutions.
    /// </summary>
    public static SimulationIdentityResolution Resolve(
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

        return new SimulationIdentityResolution(callerResolution.Caller, tenantResolution.Context);
    }
}
