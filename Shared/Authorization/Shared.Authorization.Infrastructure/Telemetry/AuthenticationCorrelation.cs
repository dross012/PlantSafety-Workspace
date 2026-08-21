// ============================================================================
//  Project:        Shared.Authorization.Infrastructure
//  Module:         Telemetry
//  File:           AuthenticationCorrelation.cs
//  Created:        2026-08-10
//  Description:    Phase 10 deliverable. A thin helper confirming the
//                  authentication layer always carries the request's real
//                  correlation id (from ICorrelationContext, set by
//                  CorrelationIdMiddleware earlier in the pipeline) into the
//                  CallerContext it produces, rather than introducing a
//                  second correlation-id concept. Pure -- no new state.
// ============================================================================
using Shared.Telemetry.Abstractions.Correlation;

namespace Shared.Authorization.Infrastructure.Telemetry;

/// <summary>Resolves the correlation id the authentication layer must use, from the existing ICorrelationContext.</summary>
public static class AuthenticationCorrelation
{
    /// <summary>Returns the current request's correlation id, as established by CorrelationIdMiddleware.</summary>
    public static string ResolveCorrelationId(ICorrelationContext correlationContext)
    {
        ArgumentNullException.ThrowIfNull(correlationContext);
        return correlationContext.CorrelationId;
    }
}
