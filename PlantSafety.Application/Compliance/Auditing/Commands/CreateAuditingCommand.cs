// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Auditing
//  Layer:          Application
//  File:           CreateAuditingCommand.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Command to submit a new Auditing record under the Compliance.Auditing bounded
//                  context. Carries the full command envelope per Handbook Rule 2.1. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Commands.CreatePermitsCommand.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Compliance.Auditing.Commands;

/// <summary>Submits a new Auditing record.</summary>
public sealed record CreateAuditingCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid TenantId,
    Guid? SiteId,
    string AuditTitle,
    string AuditType,
    string Scope,
    Guid AuditorId,
    DateTime? ScheduledAtUtc,
    Guid RequesterUserId) : ICommand<Result<CreateAuditingResult>>
{
    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => RequesterUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<CreateAuditingResult>>.CausationId => CausationId;

    /// <summary>Builds a <see cref="CreateAuditingCommand"/> from raw, untrusted transport values.</summary>
    public static CreateAuditingCommand FromDto(
        Guid tenantId,
        Guid? siteId,
        string auditTitle,
        string auditType,
        string scope,
        Guid auditorId,
        DateTime? scheduledAtUtc,
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
            AuditTitle: (auditTitle ?? string.Empty).Trim(),
            AuditType: (auditType ?? "Internal").Trim(),
            Scope: (scope ?? string.Empty).Trim(),
            AuditorId: auditorId,
            ScheduledAtUtc: scheduledAtUtc,
            RequesterUserId: requesterUserId);
}
