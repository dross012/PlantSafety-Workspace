// ============================================================================
//  Project:        Shared.Authorization.Infrastructure
//  Module:         Telemetry
//  File:           AuthenticationTracing.cs
//  Created:        2026-08-10
//  Description:    Phase 10 deliverable. A named, deterministic span-name
//                  constant for the one authentication operation this phase
//                  introduces (API-key resolution). Not a new tracing
//                  primitive or an OpenTelemetry ActivitySource -- this
//                  program has no distributed-tracing collector configured
//                  (same standing fact as Phase 8's telemetry work); this
//                  type exists so log events and any future real tracing
//                  integration share one canonical span-name string instead
//                  of each call site inventing its own.
// ============================================================================
namespace Shared.Authorization.Infrastructure.Telemetry;

/// <summary>Canonical, deterministic span/operation names for the authentication layer.</summary>
public static class AuthenticationTracing
{
    /// <summary>The operation name for resolving an incoming request's API key into a CallerContext.</summary>
    public const string ApiKeyResolutionSpanName = "Authentication.ResolveApiKey";
}
