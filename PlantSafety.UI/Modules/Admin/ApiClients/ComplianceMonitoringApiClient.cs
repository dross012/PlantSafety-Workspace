// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-08-03
// FILE DESCRIPTION: Typed API client for the Admin Portal Support Console's tenant audit
//   stream viewer. Wraps HTTP communication for
//   PlantSafety.Api.Auditing.ComplianceMonitoring.Controllers.V1.ComplianceMonitoringV1Controller.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Globalization;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using PlantSafety.UI.Infrastructure.ApiClients;
using PlantSafety.UI.Models.Common;
using PlantSafety.UI.Modules.Admin.Models;

namespace PlantSafety.UI.Modules.Admin.ApiClients;

/// <summary>Typed API client for the Admin Portal Support Console's tenant audit stream viewer.</summary>
public sealed class ComplianceMonitoringApiClient : SafetyApiClientBase
{
    /// <summary>Initializes a new instance of <see cref="ComplianceMonitoringApiClient"/>.</summary>
    public ComplianceMonitoringApiClient(HttpClient http, ILogger<ComplianceMonitoringApiClient> logger)
        : base(http, logger)
    {
    }

    /// <summary>Retrieves the tenant-scoped authorization-decision (permission usage) audit stream for the trailing 24 hours.</summary>
    public Task<ApiResponseEnvelope<TenantAuditStreamResultDto>> GetTenantAuditStreamAsync(
        Guid tenantId, CancellationToken cancellationToken = default)
    {
        var toUtc = DateTime.UtcNow;
        var fromUtc = toUtc.AddHours(-24);

        var query = $"fromUtc={Uri.EscapeDataString(fromUtc.ToString("O", CultureInfo.InvariantCulture))}" +
                    $"&toUtc={Uri.EscapeDataString(toUtc.ToString("O", CultureInfo.InvariantCulture))}";

        return GetAsync<TenantAuditStreamResultDto>(
            $"v1/auditing/compliance-monitoring/tenants/{tenantId}/audit-stream?{query}", cancellationToken);
    }
}
