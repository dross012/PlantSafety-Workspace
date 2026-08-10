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
// ============================================================================
using Shared.Authorization.Abstractions.Caller;
using Shared.Authorization.Abstractions.Identity;
using Shared.Authorization.Abstractions.Roles;
using Shared.Authorization.Infrastructure.Governance;
using Shared.Authorization.Infrastructure.Telemetry;
using Shared.Deterministic.Abstractions.Clock;
using Shared.Telemetry.Abstractions.Correlation;
using Shared.Telemetry.Abstractions.Events;

namespace PlantSafety.UI.Api.Middleware;

/// <summary>Resolves the request's API key (if any) into a CallerContext and makes it available for the rest of the pipeline.</summary>
public sealed class ApiKeyAuthenticationMiddleware(RequestDelegate next)
{
    private const string ApiKeyHeader = "X-Api-Key";

    /// <summary>Invokes the middleware.</summary>
    public async Task InvokeAsync(
        HttpContext context,
        CallerContextProvider callerContextProvider,
        IReadOnlyDictionary<string, EnterpriseRole> registeredApiKeys,
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

        await next(context);
    }
}
