// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HumanFactors.CognitiveLoadAnalytics
//  Layer:          Application
//  File:           AcknowledgeCognitiveRiskAlertCommand.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Command to acknowledge a generated CognitiveRiskPrediction alert. Backs the
//                  acknowledgement-response-time KPI. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.HumanFactors.CognitiveLoadAnalytics.Commands;

/// <summary>Acknowledges a generated cognitive-risk alert.</summary>
public sealed record AcknowledgeCognitiveRiskAlertCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid TenantId,
    Guid CognitiveRiskPredictionId,
    string? AcknowledgementNotes,
    Guid RequesterUserId) : ICommand<Result<AcknowledgeCognitiveRiskAlertResult>>
{
    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => RequesterUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<AcknowledgeCognitiveRiskAlertResult>>.CausationId => CausationId;

    /// <summary>Builds an <see cref="AcknowledgeCognitiveRiskAlertCommand"/> from raw, untrusted transport values.</summary>
    public static AcknowledgeCognitiveRiskAlertCommand FromDto(
        Guid tenantId, Guid cognitiveRiskPredictionId, string? acknowledgementNotes, Guid requesterUserId,
        Guid? correlationId = null, Guid? causationId = null, string apiVersionToken = "v1")
        => new(
            CommandId: Guid.NewGuid(),
            CorrelationId: correlationId ?? Guid.NewGuid(),
            CausationId: causationId ?? Guid.NewGuid(),
            RequestedAtUtc: DateTime.UtcNow,
            ApiVersionToken: apiVersionToken,
            TenantId: tenantId,
            CognitiveRiskPredictionId: cognitiveRiskPredictionId,
            AcknowledgementNotes: acknowledgementNotes?.Trim(),
            RequesterUserId: requesterUserId);
}

/// <summary>Result of a successful <see cref="AcknowledgeCognitiveRiskAlertCommand"/> execution.</summary>
public sealed record AcknowledgeCognitiveRiskAlertResult(
    Guid CognitiveRiskPredictionId,
    string Status,
    DateTime AcknowledgedAtUtc);
