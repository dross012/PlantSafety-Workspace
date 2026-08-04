// ============================================================
// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Typed API client for the CommunitySafety module. Wraps HTTP communication
//   with the real PlantSafety.Api endpoints under api/v1/communitysafety/...
//   (CommunitySafetyV1Controller). Calls gracefully fault through ApiResponseEnvelope so the UI
//   degrades to its Error state on backend failure.
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
using PlantSafety.UI.Modules.CommunitySafety.Models;

namespace PlantSafety.UI.Modules.CommunitySafety.ApiClients;

/// <summary>Typed API client for the CommunitySafety module.</summary>
public sealed class CommunitySafetyApiClient : SafetyApiClientBase
{
    /// <summary>Initializes a new instance of <see cref="CommunitySafetyApiClient"/>.</summary>
    /// <param name="http">Typed HttpClient with correlation and auth handler pipeline.</param>
    /// <param name="logger">SIEM-safe structured logger.</param>
    public CommunitySafetyApiClient(HttpClient http, ILogger<CommunitySafetyApiClient> logger)
        : base(http, logger)
    {
    }

    /// <summary>Lists open community concerns, feeding the Community Concern Tracker dashboard.</summary>
    public Task<ApiResponseEnvelope<CommunityConcernsResponseDto>> ListOpenCommunityConcernsAsync(
        Guid? siteId, CancellationToken cancellationToken = default)
    {
        var query = siteId.HasValue ? $"?siteId={Uri.EscapeDataString(siteId.Value.ToString("D", CultureInfo.InvariantCulture))}" : string.Empty;
        return GetAsync<CommunityConcernsResponseDto>($"v1/communitysafety/community-concerns/open{query}", cancellationToken);
    }

    /// <summary>Retrieves community notification history for a tenant/site within a date range,
    /// feeding the Notification History &amp; Reach dashboard.</summary>
    public Task<ApiResponseEnvelope<CommunityNotificationsResponseDto>> ListNotificationsByPeriodAsync(
        Guid? siteId, DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        var query = $"?From={Uri.EscapeDataString(from.ToString("O", CultureInfo.InvariantCulture))}" +
                    $"&To={Uri.EscapeDataString(to.ToString("O", CultureInfo.InvariantCulture))}";
        if (siteId.HasValue)
            query += $"&SiteId={Uri.EscapeDataString(siteId.Value.ToString("D", CultureInfo.InvariantCulture))}";

        return GetAsync<CommunityNotificationsResponseDto>($"v1/communitysafety/community-notifications{query}", cancellationToken);
    }

    /// <summary>Retrieves community impact assessments for a given SafetyEvents incident,
    /// feeding the Impact Assessment Summary by incident dashboard.</summary>
    public Task<ApiResponseEnvelope<CommunityImpactAssessmentsResponseDto>> GetImpactAssessmentsByIncidentAsync(
        Guid incidentId, CancellationToken cancellationToken = default)
        => GetAsync<CommunityImpactAssessmentsResponseDto>($"v1/communitysafety/community-impact-assessments/by-incident/{incidentId:D}", cancellationToken);
}
