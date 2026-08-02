// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Financials
//  Layer:          Application
//  File:           AddFinancialRecordLineItemCommand.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Command to append a new line item to a Draft FinancialRecord. Implements
//                  IConcurrencyAwareCommand from the start. Mirrors
//                  PlantSafety.Application.Compliance.Auditing.Commands.StartAuditingCommand's
//                  ETag-forwarding shape.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Behaviors.Concurrency;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Financials.Commands;

/// <summary>Appends a new line item to a Draft FinancialRecord.</summary>
public sealed record AddFinancialRecordLineItemCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    Guid TenantId,
    Guid FinancialRecordId,
    string Description,
    decimal Amount,
    Guid ActorUserId) : ICommand<Result<FinancialRecordStatusResult>>, IConcurrencyAwareCommand
{
    /// <summary>Gets the UTC timestamp when this command was created at the API boundary.</summary>
    public DateTime RequestedAtUtc { get; init; } = DateTime.UtcNow;

    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";

    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => ActorUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<FinancialRecordStatusResult>>.CausationId => CausationId;

    /// <inheritdoc/>
    public string? ETag { get; init; }
}
