// ============================================================================
//  Project:        Shared.Authorization
//  Module:         Abstractions.Identity
//  File:           ApiKeyIdentity.cs
//  Created:        2026-08-10
//  Description:    Phase 10 deliverable. The deterministic outcome of
//                  resolving an incoming request's API key (present or not)
//                  against the registered key -> EnterpriseRole map. Pure
//                  data, no behavior -- CallerContextFactory turns this into
//                  a CallerContext, and the caller (the authentication
//                  middleware) uses KeyProvided/KeyRecognized to decide
//                  whether to emit a governance violation, without ever
//                  rejecting the request itself (see this phase's "no
//                  endpoint breaks / no 401 returned" requirement).
// ============================================================================
using Shared.Authorization.Abstractions.Roles;

namespace Shared.Authorization.Abstractions.Identity;

/// <summary>The deterministic result of resolving a raw API key header value against the registered key map.</summary>
public sealed record ApiKeyIdentity(bool KeyProvided, bool KeyRecognized, EnterpriseRole ResolvedRole)
{
    /// <summary>No API key header was present. Resolves to the system placeholder role.</summary>
    public static ApiKeyIdentity NoKeyProvided(EnterpriseRole systemDefaultRole) =>
        new(KeyProvided: false, KeyRecognized: false, ResolvedRole: systemDefaultRole);

    /// <summary>An API key header was present but did not match any registered key. Resolves to the system placeholder role.</summary>
    public static ApiKeyIdentity UnrecognizedKey(EnterpriseRole systemDefaultRole) =>
        new(KeyProvided: true, KeyRecognized: false, ResolvedRole: systemDefaultRole);

    /// <summary>An API key header was present and matched a registered key, resolving to its mapped role.</summary>
    public static ApiKeyIdentity Recognized(EnterpriseRole resolvedRole) =>
        new(KeyProvided: true, KeyRecognized: true, ResolvedRole: resolvedRole);
}
