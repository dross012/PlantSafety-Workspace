// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-08-03
// FILE DESCRIPTION: Delegating HTTP handler that stamps the X-Tenant-ID header (honored by
//   PlantSafety.Api's TenantResolutionMiddleware, see ApiHeaderConstants.TenantId) on every
//   outbound API request while a tenant-impersonation session (Phase 7 §6.3 Support Tooling) is
//   active and unexpired. This is what makes "browse as tenant" during impersonation actually
//   scope every module's API calls to the impersonated tenant, on top of the real, audited
//   StartImpersonationCommand/EndImpersonationCommand session lifecycle -- it never bypasses
//   server-side authorization, it only supplies the header the middleware already honors.
//   Registered once in RegisterApiClient's shared handler pipeline (MauiProgram.cs), so every
//   typed API client in the application picks it up automatically.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §1.0
// ============================================================

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using PlantSafety.UI.Modules.Admin.State;

namespace PlantSafety.UI.Infrastructure.Impersonation;

/// <summary>
/// Delegating handler that attaches the <c>X-Tenant-ID</c> header for the impersonated tenant
/// while an <see cref="ImpersonationState"/> session is active and not past its time-box.
/// </summary>
public sealed class ImpersonationHeaderHandler : DelegatingHandler
{
    private const string TenantIdHeaderName = "X-Tenant-ID";

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
            request.Headers.Add(TenantIdHeaderName, session.TenantId.ToString());
        }

        return base.SendAsync(request, cancellationToken);
    }
}
