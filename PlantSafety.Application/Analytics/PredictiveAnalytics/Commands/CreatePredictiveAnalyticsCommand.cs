// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.PredictiveAnalytics
//  Layer:          Application
//  File:           CreatePredictiveAnalyticsCommand.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Command to submit a new predictive-model output/scoring record under the
//                  Analytics.PredictiveAnalytics bounded context. Carries the full command
//                  envelope (CorrelationId, CausationId, CommandId, RequestedAtUtc,
//                  ApiVersionToken) per Handbook Rule 2.1. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Commands.CreatePermitsCommand.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Analytics.PredictiveAnalytics.Commands;

/// <summary>Submits a new PredictiveAnalytics record.</summary>
/// <param name="CommandId">Unique identifier for this command instance (Uuid7).</param>
/// <param name="CorrelationId">Operation-group correlation identifier, constant across the whole request.</param>
/// <param name="CausationId">Identifier of the immediate parent command or event. Null only for root operations.</param>
/// <param name="RequestedAtUtc">UTC instant the command was issued, generated once at the API boundary.</param>
/// <param name="ApiVersionToken">API version token the request was issued under.</param>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="SiteId">Optional site scope. Null applies tenant-wide.</param>
/// <param name="ModelName">Name of the predictive model that generated this output.</param>
/// <param name="PredictionType">Prediction classification (e.g., IncidentRisk, EquipmentFailure, ComplianceDrift).</param>
/// <param name="TargetEntityId">Entity this prediction targets (worker, equipment, site), when applicable.</param>
/// <param name="PredictedRiskScore">Predicted risk score (0-100).</param>
/// <param name="ConfidenceLevel">Model confidence level for this prediction (0-100).</param>
/// <param name="PredictionHorizonDays">Prediction horizon, in days, this output forecasts over.</param>
/// <param name="RequesterUserId">Identity of the system/user generating the prediction.</param>
public sealed record CreatePredictiveAnalyticsCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid TenantId,
    Guid? SiteId,
    string ModelName,
    string PredictionType,
    Guid? TargetEntityId,
    decimal PredictedRiskScore,
    decimal ConfidenceLevel,
    int PredictionHorizonDays,
    Guid RequesterUserId) : ICommand<Result<CreatePredictiveAnalyticsResult>>
{
    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => RequesterUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<CreatePredictiveAnalyticsResult>>.CausationId => CausationId;

    /// <summary>Builds a <see cref="CreatePredictiveAnalyticsCommand"/> from raw, untrusted transport values.</summary>
    public static CreatePredictiveAnalyticsCommand FromDto(
        Guid tenantId,
        Guid? siteId,
        string modelName,
        string predictionType,
        Guid? targetEntityId,
        decimal predictedRiskScore,
        decimal confidenceLevel,
        int predictionHorizonDays,
        Guid requesterUserId,
        Guid? correlationId = null,
        Guid? causationId = null,
        string apiVersionToken = "v1")
        => new(
            CommandId: Guid.NewGuid(),
            CorrelationId: correlationId ?? Guid.NewGuid(),
            CausationId: causationId ?? Guid.NewGuid(),
            RequestedAtUtc: DateTime.UtcNow,
            ApiVersionToken: apiVersionToken,
            TenantId: tenantId,
            SiteId: siteId,
            ModelName: (modelName ?? string.Empty).Trim(),
            PredictionType: (predictionType ?? string.Empty).Trim(),
            TargetEntityId: targetEntityId,
            PredictedRiskScore: predictedRiskScore,
            ConfidenceLevel: confidenceLevel,
            PredictionHorizonDays: predictionHorizonDays < 1 ? 1 : predictionHorizonDays,
            RequesterUserId: requesterUserId);
}
