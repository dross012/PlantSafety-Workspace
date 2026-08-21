// ============================================================================
//  Project:        Shared.Authorization.Infrastructure
//  Module:         Telemetry
//  File:           TenantCorrelation.cs
//  Created:        2026-08-10
//  Description:    Phase 11 deliverable. Mirrors AuthenticationCorrelation
//                  (Phase 10) exactly: confirms the tenant layer always
//                  carries the request's real correlation id, rather than
//                  introducing a second correlation-id concept. Pure -- no
//                  new state.
// ============================================================================
using Shared.Telemetry.Abstractions.Correlation;

namespace Shared.Authorization.Infrastructure.Telemetry;

/// <summary>Resolves the correlation id the tenant layer must use, from the existing ICorrelationContext.</summary>
public static class TenantCorrelation
{
    /// <summary>Returns the current request's correlation id, as established by CorrelationIdMiddleware.</summary>
    public static string ResolveCorrelationId(ICorrelationContext correlationContext)
    {
        ArgumentNullException.ThrowIfNull(correlationContext);
        return correlationContext.CorrelationId;
    }
}
