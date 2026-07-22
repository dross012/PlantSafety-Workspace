// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Command to add a new inspection line item to an existing ChecklistTemplate (draft status only). Carries the full command envelope (CorrelationId, CausationId, CommandId, RequestedAtUtc, ApiVersionToken) per Handbook Rule 2.1.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Checklists.Commands;

/// <summary>
/// Adds a new inspection line item to an existing ChecklistTemplate under the Checklists bounded context.
/// </summary>
/// <param name="CommandId">Unique identifier for this command instance (Uuid7).</param>
/// <param name="CorrelationId">Operation-group correlation identifier, constant across the whole request.</param>
/// <param name="CausationId">Identifier of the immediate parent command or event. Null only for root operations.</param>
/// <param name="RequestedAtUtc">UTC instant the command was issued, generated once at the API boundary.</param>
/// <param name="ApiVersionToken">API version token the request was issued under.</param>
/// <param name="OwnerId">Identity of the accountable owner / CRUD-authorship user.</param>
/// <param name="ChecklistTemplateId">Identity of the ChecklistTemplate to add the item to.</param>
/// <param name="Description">Short inspection instruction text.</param>
/// <param name="IsCritical">Whether a failed answer on this item is a stop-work/critical condition.</param>
/// <param name="RegulatoryReference">Optional item-level regulatory citation.</param>
public sealed record AddChecklistTemplateItemCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid OwnerId,
    Guid ChecklistTemplateId,
    string Description,
    bool IsCritical,
    string? RegulatoryReference) : ICommand<Result<AddChecklistTemplateItemResult>>
{
    /// <summary>Placeholder: no tenant field exists on this command's payload; defaults to <see cref="Guid.Empty"/> until tenant scoping is threaded through.</summary>
    public Guid TenantId { get; init; } = Guid.Empty;
    /// <inheritdoc/>
    public Guid ActorId => OwnerId;
    Guid? ICommand<Result<AddChecklistTemplateItemResult>>.CausationId => CausationId;

    /// <summary>
    /// Builds a <see cref="AddChecklistTemplateItemCommand"/> from raw, untrusted transport values, normalizing
    /// strings and generating envelope identifiers where the caller did not supply them.
    /// </summary>
    public static AddChecklistTemplateItemCommand FromDto(
        Guid ownerId,
        Guid checklistTemplateId,
        string description,
        bool isCritical,
        string? regulatoryReference,
        Guid? correlationId = null,
        Guid? causationId = null,
        string apiVersionToken = "v1")
        => new(
            CommandId: Guid.NewGuid(),
            CorrelationId: correlationId ?? Guid.NewGuid(),
            CausationId: causationId ?? Guid.NewGuid(),
            RequestedAtUtc: DateTime.UtcNow,
            ApiVersionToken: apiVersionToken,
            OwnerId: ownerId,
            ChecklistTemplateId: checklistTemplateId,
            Description: (description ?? string.Empty).Trim(),
            IsCritical: isCritical,
            RegulatoryReference: regulatoryReference);
}
