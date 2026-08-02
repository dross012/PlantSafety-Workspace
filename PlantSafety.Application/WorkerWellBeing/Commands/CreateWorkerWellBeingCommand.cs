// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         WorkerWellBeing
//  Layer:          Application
//  File:           CreateWorkerWellBeingCommand.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Command to submit a new WorkerWellBeing check-in record. Carries the full
//                  command envelope per Handbook Rule 2.1. Mirrors
//                  PlantSafety.Application.Compliance.Auditing.Commands.CreateAuditingCommand.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.WorkerWellBeing.Commands;

/// <summary>Submits a new WorkerWellBeing check-in record.</summary>
public sealed record CreateWorkerWellBeingCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid TenantId,
    Guid? SiteId,
    Guid WorkerUserId,
    string WellBeingRiskLevel,
    string CheckInType,
    int WellBeingScore,
    bool FollowUpRequired,
    Guid RequesterUserId) : ICommand<Result<CreateWorkerWellBeingResult>>
{
    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => RequesterUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<CreateWorkerWellBeingResult>>.CausationId => CausationId;

    /// <summary>Builds a <see cref="CreateWorkerWellBeingCommand"/> from raw, untrusted transport values.</summary>
    public static CreateWorkerWellBeingCommand FromDto(
        Guid tenantId,
        Guid? siteId,
        Guid workerUserId,
        string wellBeingRiskLevel,
        string checkInType,
        int wellBeingScore,
        bool followUpRequired,
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
            WorkerUserId: workerUserId,
            WellBeingRiskLevel: (wellBeingRiskLevel ?? "Low").Trim(),
            CheckInType: (checkInType ?? string.Empty).Trim(),
            WellBeingScore: wellBeingScore,
            FollowUpRequired: followUpRequired,
            RequesterUserId: requesterUserId);
}
