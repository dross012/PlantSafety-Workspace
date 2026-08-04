// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-08-04
// FILE DESCRIPTION: Typed API client for the HumanFactors.CognitiveLoadAnalytics sub-module.
//   Targets PlantSafety.Api.HumanFactors.CognitiveLoadAnalytics.Controllers.V1.
//   CognitiveLoadAnalyticsV1Controller ("api/v1/human-factors/cognitive-load-analytics/...").
//   Calls gracefully fault through ApiResponseEnvelope so the UI degrades to its Error state on
//   backend failure.
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
using PlantSafety.UI.Modules.HumanFactors.CognitiveLoadAnalytics.Models;

namespace PlantSafety.UI.Modules.HumanFactors.CognitiveLoadAnalytics.ApiClients;

/// <summary>Typed API client for the HumanFactors.CognitiveLoadAnalytics sub-module.</summary>
public sealed class CognitiveLoadAnalyticsApiClient : SafetyApiClientBase
{
    /// <summary>Initializes a new instance of <see cref="CognitiveLoadAnalyticsApiClient"/>.</summary>
    /// <param name="http">Typed HttpClient with correlation and auth handler pipeline.</param>
    /// <param name="logger">SIEM-safe structured logger.</param>
    public CognitiveLoadAnalyticsApiClient(HttpClient http, ILogger<CognitiveLoadAnalyticsApiClient> logger)
        : base(http, logger)
    {
    }

    /// <summary>Retrieves a single worker's cognitive-load rollup for a period.</summary>
    public Task<ApiResponseEnvelope<CognitiveLoadByWorkerDto>> GetCognitiveLoadByWorkerAsync(
        Guid workerUserId, DateTime periodStartUtc, DateTime periodEndUtc, CancellationToken cancellationToken = default)
    {
        var query = $"periodStartUtc={Uri.EscapeDataString(periodStartUtc.ToString("O", CultureInfo.InvariantCulture))}" +
                    $"&periodEndUtc={Uri.EscapeDataString(periodEndUtc.ToString("O", CultureInfo.InvariantCulture))}";

        return GetAsync<CognitiveLoadByWorkerDto>($"v1/human-factors/cognitive-load-analytics/workers/{workerUserId:D}?{query}", cancellationToken);
    }

    /// <summary>Retrieves a single task-complexity profile by task code.</summary>
    public Task<ApiResponseEnvelope<TaskComplexityProfileDto>> GetTaskComplexityProfileAsync(
        string taskCode, CancellationToken cancellationToken = default)
        => GetAsync<TaskComplexityProfileDto>(
            $"v1/human-factors/cognitive-load-analytics/task-complexity-profiles/{Uri.EscapeDataString(taskCode)}", cancellationToken);

    /// <summary>Retrieves workload-trend snapshots for a shift within a period.</summary>
    public Task<ApiResponseEnvelope<IReadOnlyList<WorkloadTrendByShiftDto>>> GetWorkloadTrendByShiftAsync(
        Guid? siteId, string shiftType, DateTime periodStartUtc, DateTime periodEndUtc, CancellationToken cancellationToken = default)
    {
        var query = $"shiftType={Uri.EscapeDataString(shiftType)}" +
                    $"&periodStartUtc={Uri.EscapeDataString(periodStartUtc.ToString("O", CultureInfo.InvariantCulture))}" +
                    $"&periodEndUtc={Uri.EscapeDataString(periodEndUtc.ToString("O", CultureInfo.InvariantCulture))}";
        if (siteId.HasValue)
            query += $"&siteId={Uri.EscapeDataString(siteId.Value.ToString("D", CultureInfo.InvariantCulture))}";

        return GetAsync<IReadOnlyList<WorkloadTrendByShiftDto>>($"v1/human-factors/cognitive-load-analytics/workload-trend?{query}", cancellationToken);
    }

    /// <summary>Retrieves all Elevated/Critical, non-dismissed cognitive-risk predictions.</summary>
    public Task<ApiResponseEnvelope<IReadOnlyList<CognitiveRiskPredictionDto>>> ListElevatedCognitiveRiskPredictionsAsync(
        Guid? siteId, CancellationToken cancellationToken = default)
    {
        var query = siteId.HasValue ? $"?siteId={Uri.EscapeDataString(siteId.Value.ToString("D", CultureInfo.InvariantCulture))}" : string.Empty;

        return GetAsync<IReadOnlyList<CognitiveRiskPredictionDto>>($"v1/human-factors/cognitive-load-analytics/risk-predictions/elevated{query}", cancellationToken);
    }

    /// <summary>Acknowledges a generated cognitive-risk alert.</summary>
    public Task<ApiResponseEnvelope<object>> AcknowledgeCognitiveRiskAlertAsync(
        Guid cognitiveRiskPredictionId, string? acknowledgementNotes, CancellationToken cancellationToken = default)
        => PostAsync<object, object>(
            $"v1/human-factors/cognitive-load-analytics/risk-predictions/{cognitiveRiskPredictionId:D}/acknowledge",
            new { AcknowledgementNotes = acknowledgementNotes }, cancellationToken);
}
