// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Typed API client for the Predictive Maintenance module
//   (Volume P1). Wraps all HTTP communication for the Dashboard, Equipment
//   Predictive View, Predictions List, Prediction Detail, Sensor Data
//   Explorer, Recommendation Detail, and Predictive Reports pages. All
//   methods return ApiResponseEnvelope<T>.
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
using PlantSafety.UI.Modules.PredictiveMaintenance.Models;

namespace PlantSafety.UI.Modules.PredictiveMaintenance.ApiClients;

/// <summary>Typed API client for the Predictive Maintenance module.</summary>
public sealed class PredictiveMaintenanceApiClient : SafetyApiClientBase
{
    /// <summary>Initializes a new instance of <see cref="PredictiveMaintenanceApiClient"/>.</summary>
    public PredictiveMaintenanceApiClient(HttpClient http, ILogger<PredictiveMaintenanceApiClient> logger)
        : base(http, logger)
    {
    }

    // Route prefix note (2026-08-03): these 13 sub-resources now call the real, extended
    // Operational.PredictiveMaintenance module (PredictiveMaintenanceInsightsV1Controller) at
    // "v1/operational/predictive-maintenance/*" instead of the previously-imagined
    // "v1/predictive-maintenance/*" prefix that had no backing controller. See
    // docs/Phase5/ui-gaps-scoping.md §4.

    /// <summary>Retrieves the Risk Summary card counts.</summary>
    public Task<ApiResponseEnvelope<RiskSummaryDto>> GetRiskSummaryAsync(CancellationToken cancellationToken = default)
        => GetAsync<RiskSummaryDto>("v1/operational/predictive-maintenance/risk-summary", cancellationToken);

    /// <summary>Retrieves the Equipment Health card percentages.</summary>
    public Task<ApiResponseEnvelope<EquipmentHealthDto>> GetEquipmentHealthAsync(CancellationToken cancellationToken = default)
        => GetAsync<EquipmentHealthDto>("v1/operational/predictive-maintenance/equipment-health", cancellationToken);

    /// <summary>Retrieves a paginated page of active predictions.</summary>
    public Task<ApiResponseEnvelope<PaginatedResponseDto<PredictionRowDto>>> GetActivePredictionsAsync(
        int pageNumber = 1, int pageSize = 25, CancellationToken cancellationToken = default)
        => GetAsync<PaginatedResponseDto<PredictionRowDto>>(
            $"v1/operational/predictive-maintenance/predictions?pageNumber={pageNumber}&pageSize={pageSize}&activeOnly=true", cancellationToken);

    /// <summary>Retrieves the full predictions list (active, historical, and superseded).</summary>
    public Task<ApiResponseEnvelope<PaginatedResponseDto<PredictionRowDto>>> GetPredictionsAsync(
        int pageNumber = 1, int pageSize = 25, CancellationToken cancellationToken = default)
        => GetAsync<PaginatedResponseDto<PredictionRowDto>>(
            $"v1/operational/predictive-maintenance/predictions?pageNumber={pageNumber}&pageSize={pageSize}", cancellationToken);

    /// <summary>Retrieves the predictive view header for one equipment asset.</summary>
    public Task<ApiResponseEnvelope<EquipmentPredictiveViewDto>> GetEquipmentPredictiveViewAsync(Guid equipmentId, CancellationToken cancellationToken = default)
        => GetAsync<EquipmentPredictiveViewDto>($"v1/operational/predictive-maintenance/equipment/{equipmentId}", cancellationToken);

    /// <summary>Retrieves the prediction summary header for the Prediction Detail page.</summary>
    public Task<ApiResponseEnvelope<PredictionSummaryDto>> GetPredictionSummaryAsync(Guid predictionId, CancellationToken cancellationToken = default)
        => GetAsync<PredictionSummaryDto>($"v1/operational/predictive-maintenance/predictions/{predictionId}", cancellationToken);

    /// <summary>Retrieves the model explanation (feature importance + version) for a prediction.</summary>
    public Task<ApiResponseEnvelope<ModelExplanationDto>> GetModelExplanationAsync(Guid predictionId, CancellationToken cancellationToken = default)
        => GetAsync<ModelExplanationDto>($"v1/operational/predictive-maintenance/predictions/{predictionId}/explanation", cancellationToken);

    /// <summary>Retrieves the risk forecast curve for a prediction.</summary>
    public Task<ApiResponseEnvelope<PaginatedResponseDto<ForecastPointDto>>> GetForecastAsync(Guid predictionId, CancellationToken cancellationToken = default)
        => GetAsync<PaginatedResponseDto<ForecastPointDto>>($"v1/operational/predictive-maintenance/predictions/{predictionId}/forecast", cancellationToken);

    /// <summary>Retrieves the recommended actions for a prediction.</summary>
    public Task<ApiResponseEnvelope<PaginatedResponseDto<RecommendationDto>>> GetRecommendationsAsync(Guid predictionId, CancellationToken cancellationToken = default)
        => GetAsync<PaginatedResponseDto<RecommendationDto>>($"v1/operational/predictive-maintenance/predictions/{predictionId}/recommendations", cancellationToken);

    /// <summary>Retrieves the full detail for a single recommendation.</summary>
    public Task<ApiResponseEnvelope<RecommendationDto>> GetRecommendationAsync(Guid recommendationId, CancellationToken cancellationToken = default)
        => GetAsync<RecommendationDto>($"v1/operational/predictive-maintenance/recommendations/{recommendationId}", cancellationToken);

    /// <summary>Retrieves sensor time-series data for the Sensor Data Explorer.</summary>
    public Task<ApiResponseEnvelope<PaginatedResponseDto<SensorDataPointDto>>> GetSensorDataAsync(
        Guid? equipmentId = null, string? channel = null, CancellationToken cancellationToken = default)
        => GetAsync<PaginatedResponseDto<SensorDataPointDto>>(
            "v1/operational/predictive-maintenance/sensors/data" +
            BuildQuery(("equipmentId", equipmentId?.ToString()), ("channel", channel)),
            cancellationToken);

    /// <summary>Retrieves detected anomalies for the Sensor Data Explorer.</summary>
    public Task<ApiResponseEnvelope<PaginatedResponseDto<SensorAnomalyDto>>> GetSensorAnomaliesAsync(
        Guid? equipmentId = null, CancellationToken cancellationToken = default)
        => GetAsync<PaginatedResponseDto<SensorAnomalyDto>>(
            "v1/operational/predictive-maintenance/sensors/anomalies" + BuildQuery(("equipmentId", equipmentId?.ToString())),
            cancellationToken);

    /// <summary>Retrieves the exportable Predictive Reports list.</summary>
    public Task<ApiResponseEnvelope<PaginatedResponseDto<PredictiveReportDto>>> GetReportsAsync(
        int pageNumber = 1, int pageSize = 25, CancellationToken cancellationToken = default)
        => GetAsync<PaginatedResponseDto<PredictiveReportDto>>(
            $"v1/operational/predictive-maintenance/reports?pageNumber={pageNumber}&pageSize={pageSize}", cancellationToken);

    private static string BuildQuery(params (string Key, string? Value)[] parameters)
    {
        var parts = new List<string>();
        foreach (var (key, value) in parameters)
        {
            if (!string.IsNullOrWhiteSpace(value))
                parts.Add($"{key}={Uri.EscapeDataString(value)}");
        }

        return parts.Count == 0 ? string.Empty : "?" + string.Join("&", parts);
    }
}
