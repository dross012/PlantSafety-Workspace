// ============================================================
// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Typed API client for the MachineSafety.CraneHoistSystems sub-aggregate.
//   Targets the CraneHoistSystems endpoints on PlantSafety.Api.MachineSafety.Controllers.V1.
//   MachineSafetyV1Controller ("api/v1/machinesafety/crane-hoist-systems/..."). Calls gracefully
//   fault through ApiResponseEnvelope so the UI degrades to its Error state on backend failure.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using PlantSafety.UI.Infrastructure.ApiClients;
using PlantSafety.UI.Models.Common;
using PlantSafety.UI.Modules.MachineSafety.CraneHoistSystems.Models;

namespace PlantSafety.UI.Modules.MachineSafety.CraneHoistSystems.ApiClients;

/// <summary>Typed API client for the MachineSafety.CraneHoistSystems sub-aggregate.</summary>
public sealed class CraneHoistSystemsApiClient : SafetyApiClientBase
{
    /// <summary>Initializes a new instance of <see cref="CraneHoistSystemsApiClient"/>.</summary>
    /// <param name="http">Typed HttpClient with correlation and auth handler pipeline.</param>
    /// <param name="logger">SIEM-safe structured logger.</param>
    public CraneHoistSystemsApiClient(HttpClient http, ILogger<CraneHoistSystemsApiClient> logger)
        : base(http, logger)
    {
    }

    /// <summary>Returns CraneHoistSystems read-model data for a tenant/site within a date range,
    /// feeding the Crane/Hoist Fleet Status dashboard.</summary>
    public Task<ApiResponseEnvelope<CraneHoistSystemsQueryResponseDto>> GetCraneHoistSystemsAsync(
        Guid? siteId, DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        var query = $"?From={Uri.EscapeDataString(from.ToString("O", CultureInfo.InvariantCulture))}" +
                    $"&To={Uri.EscapeDataString(to.ToString("O", CultureInfo.InvariantCulture))}";
        if (siteId.HasValue)
            query += $"&SiteId={Uri.EscapeDataString(siteId.Value.ToString("D", CultureInfo.InvariantCulture))}";

        return GetAsync<CraneHoistSystemsQueryResponseDto>($"v1/machinesafety/crane-hoist-systems{query}", cancellationToken);
    }

    /// <summary>Returns CraneHoistSystems assets due (or overdue) for inspection as of a given UTC instant,
    /// feeding the Crane/Hoist Fleet Status dashboard's inspection-due emphasis.</summary>
    public Task<ApiResponseEnvelope<IReadOnlyList<CraneHoistSystemsItemDto>>> ListAssetsDueForInspectionAsync(
        Guid? siteId, DateTime asOfUtc, CancellationToken cancellationToken = default)
    {
        var query = $"?AsOfUtc={Uri.EscapeDataString(asOfUtc.ToString("O", CultureInfo.InvariantCulture))}";
        if (siteId.HasValue)
            query += $"&SiteId={Uri.EscapeDataString(siteId.Value.ToString("D", CultureInfo.InvariantCulture))}";

        return GetAsync<IReadOnlyList<CraneHoistSystemsItemDto>>($"v1/machinesafety/crane-hoist-systems/due-for-inspection{query}", cancellationToken);
    }

    /// <summary>Returns CraneHoistSystems assets whose operator qualification expires within the given
    /// window, feeding the Operator Qualification Matrix dashboard.</summary>
    public Task<ApiResponseEnvelope<IReadOnlyList<CraneHoistSystemsItemDto>>> ListExpiringOperatorQualificationsAsync(
        Guid? siteId, int withinDays, CancellationToken cancellationToken = default)
    {
        var query = $"?WithinDays={withinDays}";
        if (siteId.HasValue)
            query += $"&SiteId={Uri.EscapeDataString(siteId.Value.ToString("D", CultureInfo.InvariantCulture))}";

        return GetAsync<IReadOnlyList<CraneHoistSystemsItemDto>>($"v1/machinesafety/crane-hoist-systems/expiring-operator-qualifications{query}", cancellationToken);
    }
}
