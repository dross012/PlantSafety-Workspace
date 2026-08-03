// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-08-03
// FILE DESCRIPTION: Typed API client for the Admin Portal's Support Console impersonation
//   feature. Wraps all HTTP communication for
//   PlantSafety.Api.Support.Impersonation.Controllers.V1.ImpersonationV1Controller.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using PlantSafety.UI.Infrastructure.ApiClients;
using PlantSafety.UI.Models.Common;
using PlantSafety.UI.Modules.Admin.Models;

namespace PlantSafety.UI.Modules.Admin.ApiClients;

/// <summary>Typed API client for the Admin Portal's Support Console impersonation feature.</summary>
public sealed class ImpersonationApiClient : SafetyApiClientBase
{
    /// <summary>Initializes a new instance of <see cref="ImpersonationApiClient"/>.</summary>
    public ImpersonationApiClient(HttpClient http, ILogger<ImpersonationApiClient> logger)
        : base(http, logger)
    {
    }

    /// <summary>Starts a real, time-boxed, audited impersonation session of the given tenant.</summary>
    public Task<ApiResponseEnvelope<ImpersonationSessionDto>> StartAsync(
        Guid tenantId, StartImpersonationRequest request, CancellationToken cancellationToken = default)
        => PostAsync<StartImpersonationRequest, ImpersonationSessionDto>(
            $"v1/support/impersonation/tenants/{tenantId}", request, cancellationToken);

    /// <summary>Ends an active impersonation session.</summary>
    public Task<ApiResponseEnvelope<ImpersonationSessionDto>> EndAsync(Guid sessionId, CancellationToken cancellationToken = default)
        => PostAsync<object, ImpersonationSessionDto>($"v1/support/impersonation/{sessionId}/end", new { }, cancellationToken);

    /// <summary>Retrieves a single impersonation session's current state (used by the persistent impersonation banner).</summary>
    public Task<ApiResponseEnvelope<ImpersonationSessionDto>> GetSessionAsync(Guid sessionId, CancellationToken cancellationToken = default)
        => GetAsync<ImpersonationSessionDto>($"v1/support/impersonation/{sessionId}", cancellationToken);
}
