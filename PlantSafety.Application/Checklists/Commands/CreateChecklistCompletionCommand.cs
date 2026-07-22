// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Command to create a new ChecklistCompletion record within the Checklists module. Carries the full command envelope (CorrelationId, CausationId, CommandId, RequestedAtUtc, ApiVersionToken) per Handbook Rule 2.1.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Checklists.Commands;

/// <summary>
/// Creates a new ChecklistCompletion record under the Checklists bounded context.
/// </summary>
/// <param name="CommandId">Unique identifier for this command instance (Uuid7).</param>
/// <param name="CorrelationId">Operation-group correlation identifier, constant across the whole request.</param>
/// <param name="CausationId">Identifier of the immediate parent command or event. Null only for root operations.</param>
/// <param name="RequestedAtUtc">UTC instant the command was issued, generated once at the API boundary.</param>
/// <param name="ApiVersionToken">API version token the request was issued under.</param>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="SiteId">Optional site scope. Null applies tenant-wide.</param>
/// <param name="OwnerId">Identity of the accountable owner / CRUD-authorship user.</param>
/// <param name="ChecklistTemplateId">Identifier of the ChecklistTemplate this completion is performed against.</param>
/// <param name="ItemsTotal">Total number of checklist items evaluated.</param>
/// <param name="ItemsPassed">Number of checklist items that passed.</param>
/// <param name="ItemsFailed">Number of checklist items that failed.</param>
/// <param name="Outcome">Overall outcome classification (Pending/Pass/Fail/PartialWithFindings).</param>
public sealed record CreateChecklistCompletionCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid TenantId,
    Guid? SiteId,
    Guid OwnerId,
    Guid ChecklistTemplateId,
    int ItemsTotal,
    int ItemsPassed,
    int ItemsFailed,
    string Outcome) : ICommand<Result<CreateChecklistCompletionResult>>
{
    /// <inheritdoc/>
    public Guid ActorId => OwnerId;
    Guid? ICommand<Result<CreateChecklistCompletionResult>>.CausationId => CausationId;

    /// <summary>
    /// Builds a <see cref="CreateChecklistCompletionCommand"/> from raw, untrusted transport values, normalizing
    /// strings and generating envelope identifiers where the caller did not supply them.
    /// </summary>
    public static CreateChecklistCompletionCommand FromDto(
        Guid tenantId,
        Guid? siteId,
        Guid ownerId,
        Guid checklistTemplateId,
        int itemsTotal,
        int itemsPassed,
        int itemsFailed,
        string outcome,
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
            OwnerId: ownerId,
            ChecklistTemplateId: checklistTemplateId,
            ItemsTotal: itemsTotal,
            ItemsPassed: itemsPassed,
            ItemsFailed: itemsFailed,
            Outcome: (outcome ?? "Pending").Trim());
}