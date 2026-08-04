// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HumanFactors.CognitiveLoadAnalytics
//  Layer:          Application
//  File:           DefineTaskComplexityProfileCommand.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Command to define a task-complexity profile against the open
//                  CognitiveLoadLedger for the given tenant/site/shift (the ledger is
//                  get-or-created by the handler). Carries the full command envelope per Handbook
//                  Rule 2.1. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.HumanFactors.CognitiveLoadAnalytics.Commands;

/// <summary>Defines a task-complexity profile.</summary>
public sealed record DefineTaskComplexityProfileCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid TenantId,
    Guid? SiteId,
    string ShiftType,
    DateTime ShiftDateUtc,
    string TaskCode,
    string TaskName,
    int ComplexityScore,
    string Description,
    Guid RequesterUserId) : ICommand<Result<DefineTaskComplexityProfileResult>>
{
    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => RequesterUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<DefineTaskComplexityProfileResult>>.CausationId => CausationId;

    /// <summary>Builds a <see cref="DefineTaskComplexityProfileCommand"/> from raw, untrusted transport values.</summary>
    public static DefineTaskComplexityProfileCommand FromDto(
        Guid tenantId, Guid? siteId, string shiftType, DateTime shiftDateUtc,
        string taskCode, string taskName, int complexityScore, string description, Guid requesterUserId,
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
            TaskCode: (taskCode ?? string.Empty).Trim(),
            TaskName: (taskName ?? string.Empty).Trim(),
            ComplexityScore: complexityScore,
            Description: (description ?? string.Empty).Trim(),
            RequesterUserId: requesterUserId);
}

/// <summary>Result of a successful <see cref="DefineTaskComplexityProfileCommand"/> execution.</summary>
public sealed record DefineTaskComplexityProfileResult(
    Guid CognitiveLoadLedgerId,
    Guid TaskComplexityProfileId,
    string TaskCode,
    int ComplexityScore);
