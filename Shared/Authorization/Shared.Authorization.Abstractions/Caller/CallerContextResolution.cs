// ============================================================================
//  Project:        Shared.Authorization
//  Module:         Abstractions.Caller
//  File:           CallerContextResolution.cs
//  Created:        2026-08-10
//  Description:    Phase 10 deliverable. The full outcome of
//                  CallerContextFactory.Create -- the resolved CallerContext
//                  itself, plus the ApiKeyIdentity it was resolved from, so
//                  the authentication middleware can emit accurate telemetry
//                  and governance signals (e.g. "key provided but not
//                  recognized") without re-deriving that information.
// ============================================================================
using Shared.Authorization.Abstractions.Identity;

namespace Shared.Authorization.Abstractions.Caller;

/// <summary>The resolved CallerContext for a request, plus the ApiKeyIdentity it was derived from.</summary>
public sealed record CallerContextResolution(CallerContext Caller, ApiKeyIdentity Identity);
