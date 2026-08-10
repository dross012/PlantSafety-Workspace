// ============================================================================
//  Project:        PlantSafety.UI.Api
//  Module:         Middleware
//  File:           ApiKeyAuthenticationMiddleware.cs
//  Created:        2026-08-10
//  Description:    Phase 10 (Authentication Layer Modernization) deliverable.
//                  Runs immediately after CorrelationIdMiddleware (so a real
//                  correlation id already exists) and before routing/
//                  controllers execute. Reads the "X-Api-Key" header,
//                  deterministically resolves it via CallerContextFactory
//                  against the statically-configured ApiKeyAuthenticationOptions
//                  registration map, populates CallerContextProvider once for
//                  the request, and emits telemetry + governance signals.
//
//                  Deliberately does not short-circuit the pipeline on a
//                  missing or unrecognized key: per this phase's explicit
//                  requirement, "no endpoint breaks" and "no 401/403 is
//                  returned" from this middleware -- CallerContext falls back
//                  to CallerContext.SystemDefault (EnterpriseRole.PlatformAdmin)
//                  exactly as it did before this phase (previously constructed
//                  inline by WorkflowEngineController; now constructed once,
//                  here). Authorization enforcement continues to happen only
//                  inside the domain (Phase 9's WorkflowEngineAuthorization
//                  and friends), unchanged by this phase.
//
//                  Phase 11: also resolves the same "X-Api-Key" header
//                  against TenantAuthenticationOptions' independent
//                  key -> TenantId map (via TenantContextFactory) and
//                  populates TenantContextProvider once for the request --
//                  the prompt's own requirement that TenantContext "must be
//                  constructed by AuthenticationMiddleware." Falls back to
//                  TenantContext.Unscoped when no tenant binding is
//                  registered for the key, same "no endpoint breaks"
//                  guarantee as the CallerContext resolution above.
//
//                  Phase 12: composes the resolved CallerContextResolution +
//                  TenantContextResolution into a ContractMetadata
//                  (ContractMetadataFactory) and runs SchemaGovernanceEvaluator
//                  once per request -- the single composed identity/contract/
//                  tenant governance pass this phase's "Unified Governance"
//                  section asks for, on top of (not instead of) the
//                  per-layer Authentication/Tenant governance calls above.
// ============================================================================
using Shared.Authorization.Abstractions.Caller;
using Shared.Authorization.Abstractions.Contracts;
using Shared.Authorization.Abstractions.Identity;
using Shared.Authorization.Abstractions.Roles;
using Shared.Authorization.Abstractions.Tenant;
using Shared.Authorization.Infrastructure.Contracts;
using Shared.Authorization.Infrastructure.Governance;
using Shared.Authorization.Infrastructure.Telemetry;
using Shared.Deterministic.Abstractions.Clock;
using Shared.Telemetry.Abstractions.Correlation;
using Shared.Telemetry.Abstractions.Events;

namespace PlantSafety.UI.Api.Middleware;

/// <summary>Resolves the request's API key (if any) into a CallerContext + TenantContext and makes both available for the rest of the pipeline.</summary>
public sealed class ApiKeyAuthenticationMiddleware(RequestDelegate next)
{
    private const string ApiKeyHeader = "X-Api-Key";

    /// <summary>Invokes the middleware.</summary>
    public async Task InvokeAsync(
        HttpContext context,
        CallerContextProvider callerContextProvider,
        TenantContextProvider tenantContextProvider,
        IReadOnlyDictionary<string, EnterpriseRole> registeredApiKeys,
        IReadOnlyDictionary<string, string> registeredKeyTenants,
        ICorrelationContext correlationContext,
        IClock clock,
        ILogEventWriter logEventWriter)
    {
        string? apiKeyHeaderValue = context.Request.Headers.TryGetValue(ApiKeyHeader, out var headerValue)
            ? headerValue.ToString()
            : null;

        CallerContextResolution resolution = CallerContextFactory.Create(apiKeyHeaderValue, registeredApiKeys, correlationContext.CorrelationId);

        callerContextProvider.Set(resolution.Caller);

        AuthenticationTelemetry.RecordResolution(resolution, clock, logEventWriter);
        AuthenticationGovernanceEvaluator.EvaluateAndReport(resolution, clock, logEventWriter);

        TenantContextResolution tenantResolution = TenantContextFactory.Create(apiKeyHeaderValue, registeredKeyTenants, resolution.Caller, correlationContext.CorrelationId);

        tenantContextProvider.Set(tenantResolution.Context);

        TenantTelemetry.RecordResolution(tenantResolution, clock, logEventWriter);
        TenantGovernanceEvaluator.EvaluateResolutionAndReport(tenantResolution, clock, logEventWriter);

        ContractMetadata contractMetadata = ContractMetadataFactory.Create(tenantResolution.Context, clock);
        contractMetadata.RecordEmitted(clock, logEventWriter);
        SchemaGovernanceEvaluator.EvaluateAndReport(resolution, tenantResolution, contractMetadata, clock, logEventWriter);

        await next(context);
    }
}
