// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Financials
//  Layer:          Application
//  File:           CreateFinancialRecordCommand.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Command to create a new FinancialRecord (Status=Draft) under the Financials
//                  bounded context. Carries the full command envelope per Handbook Rule 2.1.
//                  Distinct namespace/name from the pre-existing, unrelated
//                  PlantSafety.Application.Financials.Financials.* read-only vertical. Mirrors
//                  PlantSafety.Application.Compliance.Auditing.Commands.CreateAuditingCommand.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Financials.Commands;

/// <summary>Creates a new FinancialRecord (Status=Draft).</summary>
public sealed record CreateFinancialRecordCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid TenantId,
    Guid? SiteId,
    string Category,
    decimal Amount,
    Guid RequesterUserId) : ICommand<Result<CreateFinancialRecordResult>>
{
    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => RequesterUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<CreateFinancialRecordResult>>.CausationId => CausationId;

    /// <summary>Builds a <see cref="CreateFinancialRecordCommand"/> from raw, untrusted transport values.</summary>
    public static CreateFinancialRecordCommand FromDto(
        Guid tenantId,
        Guid? siteId,
        string category,
        decimal amount,
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
            Category: (category ?? string.Empty).Trim(),
            Amount: amount,
            RequesterUserId: requesterUserId);
}
