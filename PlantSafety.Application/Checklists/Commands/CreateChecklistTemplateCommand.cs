// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Command to create a new ChecklistTemplate record within the Checklists module. Carries the full command envelope (CorrelationId, CausationId, CommandId, RequestedAtUtc, ApiVersionToken) per Handbook Rule 2.1.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Checklists.Commands;

/// <summary>
/// Creates a new ChecklistTemplate record under the Checklists bounded context.
/// </summary>
/// <param name="CommandId">Unique identifier for this command instance (Uuid7).</param>
/// <param name="CorrelationId">Operation-group correlation identifier, constant across the whole request.</param>
/// <param name="CausationId">Identifier of the immediate parent command or event. Null only for root operations.</param>
/// <param name="RequestedAtUtc">UTC instant the command was issued, generated once at the API boundary.</param>
/// <param name="ApiVersionToken">API version token the request was issued under.</param>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="SiteId">Optional site scope. Null applies tenant-wide.</param>
/// <param name="OwnerId">Identity of the accountable owner / CRUD-authorship user.</param>
/// <param name="TemplateTitle">Human-readable checklist template title.</param>
/// <param name="Category">ChecklistCategory code (see Checklists.SmartEnums.ChecklistCategory).</param>
/// <param name="Description">Narrative description of what this checklist template covers.</param>
/// <param name="ItemCount">Number of checklist items defined on this template.</param>
/// <param name="IsMandatory">Whether completion of this checklist is mandatory.</param>
/// <param name="RegulatoryReference">Optional regulatory citation this checklist supports.</param>
public sealed record CreateChecklistTemplateCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid TenantId,
    Guid? SiteId,
    Guid OwnerId,
    string TemplateTitle,
    string Category,
    string Description,
    int ItemCount,
    bool IsMandatory,
    string? RegulatoryReference) : ICommand<Result<CreateChecklistTemplateResult>>
{
    /// <inheritdoc/>
    public Guid ActorId => OwnerId;
    Guid? ICommand<Result<CreateChecklistTemplateResult>>.CausationId => CausationId;

    /// <summary>
    /// Builds a <see cref="CreateChecklistTemplateCommand"/> from raw, untrusted transport values, normalizing
    /// strings and generating envelope identifiers where the caller did not supply them.
    /// </summary>
    public static CreateChecklistTemplateCommand FromDto(
        Guid tenantId,
        Guid? siteId,
        Guid ownerId,
        string templateTitle,
        string category,
        string description,
        int itemCount,
        bool isMandatory,
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
            TenantId: tenantId,
            SiteId: siteId,
            OwnerId: ownerId,
            TemplateTitle: (templateTitle ?? string.Empty).Trim(),
            Category: (category ?? string.Empty).Trim(),
            Description: (description ?? string.Empty).Trim(),
            ItemCount: itemCount,
            IsMandatory: isMandatory,
            RegulatoryReference: regulatoryReference);
}