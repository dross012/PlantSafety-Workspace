// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Command to retire an active/completed ChecklistTemplate, removing it from active circulation. Carries the full command envelope per Handbook Rule 2.1.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Checklists.Commands;

/// <summary>Retires a ChecklistTemplate under the Checklists bounded context.</summary>
/// <param name="CommandId">Unique identifier for this command instance (Uuid7).</param>
/// <param name="CorrelationId">Operation-group correlation identifier, constant across the whole request.</param>
/// <param name="CausationId">Identifier of the immediate parent command or event. Null only for root operations.</param>
/// <param name="RequestedAtUtc">UTC instant the command was issued, generated once at the API boundary.</param>
/// <param name="ApiVersionToken">API version token the request was issued under.</param>
/// <param name="OwnerId">Identity of the accountable owner / CRUD-authorship user.</param>
/// <param name="ChecklistTemplateId">Identity of the ChecklistTemplate to retire.</param>
/// <param name="Reason">Narrative reason for retirement (e.g. "Superseded by 2027 revision").</param>
public sealed record RetireChecklistTemplateCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid OwnerId,
    Guid ChecklistTemplateId,
    string Reason) : ICommand<Result<RetireChecklistTemplateResult>>
{
    /// <summary>Placeholder: no tenant field exists on this command's payload; defaults to <see cref="Guid.Empty"/> until tenant scoping is threaded through.</summary>
    public Guid TenantId { get; init; } = Guid.Empty;
    /// <inheritdoc/>
    public Guid ActorId => OwnerId;
    Guid? ICommand<Result<RetireChecklistTemplateResult>>.CausationId => CausationId;

    /// <summary>
    /// Builds a <see cref="RetireChecklistTemplateCommand"/> from raw, untrusted transport values, normalizing
    /// strings and generating envelope identifiers where the caller did not supply them.
    /// </summary>
    public static RetireChecklistTemplateCommand FromDto(
        Guid ownerId,
        Guid checklistTemplateId,
        string reason,
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
            Reason: (reason ?? string.Empty).Trim());
}
