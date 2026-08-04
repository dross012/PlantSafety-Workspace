// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HumanFactors.CognitiveLoadAnalytics
//  Layer:          Application
//  File:           GenerateCognitiveRiskPredictionCommand.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Command to generate a predictive cognitive-risk assessment for a worker from a
//                  source CognitiveLoadLedger's recorded readings/trend. Invoked either
//                  interactively or by the predictive model retraining/scoring schedule.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.HumanFactors.CognitiveLoadAnalytics.Commands;

/// <summary>Generates a predictive cognitive-risk assessment for a worker.</summary>
public sealed record GenerateCognitiveRiskPredictionCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid TenantId,
    Guid? SiteId,
    Guid WorkerUserId,
    Guid SourceLedgerId,
    string ModelVersion,
    Guid RequesterUserId) : ICommand<Result<GenerateCognitiveRiskPredictionResult>>
{
    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => RequesterUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<GenerateCognitiveRiskPredictionResult>>.CausationId => CausationId;

    /// <summary>Builds a <see cref="GenerateCognitiveRiskPredictionCommand"/> from raw, untrusted transport values.</summary>
    public static GenerateCognitiveRiskPredictionCommand FromDto(
        Guid tenantId, Guid? siteId, Guid workerUserId, Guid sourceLedgerId, string modelVersion, Guid requesterUserId,
        Guid? correlationId = null, Guid? causationId = null, string apiVersionToken = "v1")
        => new(
            CommandId: Guid.NewGuid(),
            CorrelationId: correlationId ?? Guid.NewGuid(),
            CausationId: causationId ?? Guid.NewGuid(),
            RequestedAtUtc: DateTime.UtcNow,
            ApiVersionToken: apiVersionToken,
            TenantId: tenantId,
            SiteId: siteId,
            WorkerUserId: workerUserId,
            SourceLedgerId: sourceLedgerId,
            ModelVersion: (modelVersion ?? string.Empty).Trim(),
            RequesterUserId: requesterUserId);
}

/// <summary>Result of a successful <see cref="GenerateCognitiveRiskPredictionCommand"/> execution.</summary>
public sealed record GenerateCognitiveRiskPredictionResult(
    Guid CognitiveRiskPredictionId,
    string PredictedRiskLevel,
    decimal PredictionScore,
    string Status);
