// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-08-03
// LAST UPDATED: 2026-08-03 -- Phase 8 alignment governance fix. Previously stamped only
//   X-Tenant-ID, which PlantSafety.Api's TenantResolutionMiddleware never actually read (a
//   platform-wide gap: impersonation was audited/time-boxed server-side but had zero effect at
//   the API boundary -- it failed closed, not a privilege-escalation risk, but the feature simply
//   did nothing). Fixed by also stamping X-Impersonation-Session-Id, the real server-side session
//   identifier the middleware now looks up and validates (Platform.Admin + active session +
//   session bound to this exact tenant) before trusting X-Tenant-ID at all. This handler still
//   performs no authorization itself -- it only supplies the two headers the middleware
//   independently verifies against the real session record on every request.
// FILE DESCRIPTION: Delegating HTTP handler that stamps X-Tenant-ID and X-Impersonation-Session-Id
//   on every outbound API request while a tenant-impersonation session (Phase 7 §6.3 Support
//   Tooling) is active and unexpired. Registered once in RegisterApiClient's shared handler
//   pipeline (MauiProgram.cs), so every typed API client in the application picks it up
//   automatically.
// SchemaVersion: 2026.2
// Additive-only evolution — see Enterprise Architectural Rule Handbook §1.0
// ============================================================

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using PlantSafety.UI.Modules.Admin.State;

namespace PlantSafety.UI.Infrastructure.Impersonation;

/// <summary>
/// Delegating handler that attaches the <c>X-Tenant-ID</c> and <c>X-Impersonation-Session-Id</c>
/// headers for the impersonated tenant while an <see cref="ImpersonationState"/> session is
/// active and not past its time-box. Server-side validation of these headers happens exclusively
/// in <c>PlantSafety.Api.Middleware.TenantResolutionMiddleware</c> -- this handler never bypasses
/// authorization, it only supplies the claim the middleware independently verifies.
/// </summary>
public sealed class ImpersonationHeaderHandler : DelegatingHandler
{
    private const string TenantIdHeaderName = "X-Tenant-ID";
    private const string ImpersonationSessionIdHeaderName = "X-Impersonation-Session-Id";

    private readonly ImpersonationState _impersonationState;

    /// <summary>Initializes a new instance of <see cref="ImpersonationHeaderHandler"/>.</summary>
    /// <param name="impersonationState">The platform-wide impersonation session state.</param>
    public ImpersonationHeaderHandler(ImpersonationState impersonationState)
    {
        _impersonationState = impersonationState;
    }

    /// <inheritdoc/>
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var session = _impersonationState.ActiveSession;

        if (session is not null && session.EndedAtUtc is null && DateTime.UtcNow < session.ExpiresAtUtc)
        {
            request.Headers.Remove(TenantIdHeaderName);
            request.Headers.Remove(ImpersonationSessionIdHeaderName);
            request.Headers.Add(TenantIdHeaderName, session.TenantId.ToString());
            request.Headers.Add(ImpersonationSessionIdHeaderName, session.SessionId.ToString());
        }

        return base.SendAsync(request, cancellationToken);
    }
}
