// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HumanFactors.CognitiveLoadAnalytics
//  Layer:          Application
//  File:           RecalculateWorkloadTrendCommand.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Command to recalculate a CognitiveLoadLedger's workload trend for a given
//                  window. Invoked either interactively or by the periodic per-shift-cycle
//                  recalculation scheduling hook. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.HumanFactors.CognitiveLoadAnalytics.Commands;

/// <summary>Recalculates a CognitiveLoadLedger's workload trend for a given window.</summary>
public sealed record RecalculateWorkloadTrendCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid TenantId,
    Guid CognitiveLoadLedgerId,
    DateTime PeriodStartUtc,
    DateTime PeriodEndUtc,
    Guid RequesterUserId) : ICommand<Result<RecalculateWorkloadTrendResult>>
{
    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => RequesterUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<RecalculateWorkloadTrendResult>>.CausationId => CausationId;

    /// <summary>Builds a <see cref="RecalculateWorkloadTrendCommand"/> from raw, untrusted transport values.</summary>
    public static RecalculateWorkloadTrendCommand FromDto(
        Guid tenantId, Guid cognitiveLoadLedgerId, DateTime periodStartUtc, DateTime periodEndUtc, Guid requesterUserId,
        Guid? correlationId = null, Guid? causationId = null, string apiVersionToken = "v1")
        => new(
            CommandId: Guid.NewGuid(),
            CorrelationId: correlationId ?? Guid.NewGuid(),
            CausationId: causationId ?? Guid.NewGuid(),
            RequestedAtUtc: DateTime.UtcNow,
            ApiVersionToken: apiVersionToken,
            TenantId: tenantId,
            CognitiveLoadLedgerId: cognitiveLoadLedgerId,
            PeriodStartUtc: periodStartUtc,
            PeriodEndUtc: periodEndUtc,
            RequesterUserId: requesterUserId);
}

/// <summary>Result of a successful <see cref="RecalculateWorkloadTrendCommand"/> execution.</summary>
public sealed record RecalculateWorkloadTrendResult(
    Guid CognitiveLoadLedgerId,
    Guid WorkloadTrendRecordId,
    decimal MeanCognitiveLoadScore,
    int OverloadEventCount,
    int ReadingCount);
