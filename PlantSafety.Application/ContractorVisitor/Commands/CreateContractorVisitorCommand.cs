// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContractorVisitor
//  Layer:          Application
//  File:           CreateContractorVisitorCommand.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Command to register a new ContractorVisitor site-access record. Carries the
//                  full command envelope per Handbook Rule 2.1. Mirrors
//                  PlantSafety.Application.Compliance.Auditing.Commands.CreateAuditingCommand.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.ContractorVisitor.Commands;

/// <summary>Registers a new ContractorVisitor site-access record.</summary>
public sealed record CreateContractorVisitorCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid TenantId,
    Guid? SiteId,
    string FullName,
    string CompanyName,
    string VisitorType,
    string PurposeOfVisit,
    Guid HostUserId,
    DateTime? ScheduledArrivalUtc,
    Guid RequesterUserId) : ICommand<Result<CreateContractorVisitorResult>>
{
    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => RequesterUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<CreateContractorVisitorResult>>.CausationId => CausationId;

    /// <summary>Builds a <see cref="CreateContractorVisitorCommand"/> from raw, untrusted transport values.</summary>
    public static CreateContractorVisitorCommand FromDto(
        Guid tenantId,
        Guid? siteId,
        string fullName,
        string companyName,
        string visitorType,
        string purposeOfVisit,
        Guid hostUserId,
        DateTime? scheduledArrivalUtc,
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
            FullName: (fullName ?? string.Empty).Trim(),
            CompanyName: (companyName ?? string.Empty).Trim(),
            VisitorType: (visitorType ?? "Visitor").Trim(),
            PurposeOfVisit: (purposeOfVisit ?? string.Empty).Trim(),
            HostUserId: hostUserId,
            ScheduledArrivalUtc: scheduledArrivalUtc,
            RequesterUserId: requesterUserId);
}
