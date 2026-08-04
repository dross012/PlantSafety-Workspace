// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HumanFactors.CognitiveLoadAnalytics
//  Layer:          Application
//  File:           RecordCognitiveLoadReadingCommand.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Command to record a single worker cognitive-workload measurement against the
//                  open CognitiveLoadLedger for the given tenant/site/shift (the ledger is
//                  get-or-created by the handler). Carries the full command envelope per Handbook
//                  Rule 2.1. Data governance: WorkerUserId/CognitiveLoadScore are sensitive
//                  personal data; access is restricted at the authorization-policy layer.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.HumanFactors.CognitiveLoadAnalytics.Commands;

/// <summary>Records a single worker cognitive-workload measurement.</summary>
public sealed record RecordCognitiveLoadReadingCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid TenantId,
    Guid? SiteId,
    string ShiftType,
    DateTime ShiftDateUtc,
    Guid WorkerUserId,
    Guid? TaskComplexityProfileId,
    decimal CognitiveLoadScore,
    string MeasurementMethod,
    DateTime MeasuredAtUtc,
    Guid RequesterUserId) : ICommand<Result<RecordCognitiveLoadReadingResult>>
{
    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => RequesterUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<RecordCognitiveLoadReadingResult>>.CausationId => CausationId;

    /// <summary>Builds a <see cref="RecordCognitiveLoadReadingCommand"/> from raw, untrusted transport values.</summary>
    public static RecordCognitiveLoadReadingCommand FromDto(
        Guid tenantId, Guid? siteId, string shiftType, DateTime shiftDateUtc,
        Guid workerUserId, Guid? taskComplexityProfileId, decimal cognitiveLoadScore, string measurementMethod,
        DateTime measuredAtUtc, Guid requesterUserId,
        Guid? correlationId = null, Guid? causationId = null, string apiVersionToken = "v1")
        => new(
            CommandId: Guid.NewGuid(),
            CorrelationId: correlationId ?? Guid.NewGuid(),
            CausationId: causationId ?? Guid.NewGuid(),
            RequestedAtUtc: DateTime.UtcNow,
            ApiVersionToken: apiVersionToken,
            TenantId: tenantId,
            SiteId: siteId,
            ShiftType: (shiftType ?? string.Empty).Trim(),
            ShiftDateUtc: shiftDateUtc,
            WorkerUserId: workerUserId,
            TaskComplexityProfileId: taskComplexityProfileId,
            CognitiveLoadScore: cognitiveLoadScore,
            MeasurementMethod: (measurementMethod ?? string.Empty).Trim(),
            MeasuredAtUtc: measuredAtUtc,
            RequesterUserId: requesterUserId);
}

/// <summary>Result of a successful <see cref="RecordCognitiveLoadReadingCommand"/> execution.</summary>
public sealed record RecordCognitiveLoadReadingResult(
    Guid CognitiveLoadLedgerId,
    Guid CognitiveLoadReadingId,
    decimal CognitiveLoadScore,
    string WorkloadLevel,
    bool IsOverload);
