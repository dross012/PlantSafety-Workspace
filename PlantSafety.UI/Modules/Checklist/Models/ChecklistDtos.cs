// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Client-side DTOs mirroring the Checklists read-model
//   endpoint (GET v1/operational/checklists). Deserialized directly into
//   the inner Data shape of the ApiResponse&lt;T&gt; envelope, matching the
//   established sibling convention (see
//   Modules/PlanningProjects/Models/PlanningProjectsListResponseDto.cs).
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §1.4
// ============================================================

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PlantSafety.UI.Modules.Checklist.Models;

/// <summary>A single Checklist template/instance record (GET v1/operational/checklists).</summary>
public sealed class ChecklistItemDto
{
    /// <summary>Checklist record identifier.</summary>
    public Guid ChecklistId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Checklist name.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Checklist category grouping.</summary>
    public string Category { get; init; } = string.Empty;

    /// <summary>Current status of this record.</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>UTC timestamp when this record was last actioned.</summary>
    public DateTime CompletedAtUtc { get; init; }

    /// <summary>Identity of the user who last actioned this record.</summary>
    public Guid CompletedBy { get; init; }
}

/// <summary>Response payload for the Checklists read-model list endpoint.</summary>
public sealed class ChecklistsResponseDto
{
    /// <summary>Gets the tenant the data belongs to.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Gets the site the data is scoped to, or null for tenant-wide results.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Gets the UTC timestamp when this read-model snapshot was generated.</summary>
    public DateTime GeneratedAt { get; init; }

    /// <summary>Gets the matching checklist items. Never null; empty when no results.</summary>
    public IReadOnlyList<ChecklistItemDto> Items { get; init; } = [];
}

// ============================================================
// Checklists write/lifecycle surface (api/v1/checklists/templates) —
// Create -> AddItem -> Publish -> Retire.
// ============================================================

/// <summary>Request body for submitting a new ChecklistTemplate (POST v1/checklists/templates). Mirrors <c>CreateChecklistTemplateRequest</c>.</summary>
public sealed class CreateChecklistTemplateRequest
{
    /// <summary>Gets or sets the optional site scope. Null applies tenant-wide.</summary>
    public Guid? SiteId { get; set; }

    /// <summary>Gets or sets the human-readable checklist template title.</summary>
    [Required]
    [StringLength(256)]
    public string TemplateTitle { get; set; } = string.Empty;

    /// <summary>Gets or sets the ChecklistCategory code.</summary>
    [Required]
    public string Category { get; set; } = string.Empty;

    /// <summary>Gets or sets the narrative description of what this checklist template covers.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Gets or sets the number of checklist items defined on this template.</summary>
    [Range(0, int.MaxValue)]
    public int ItemCount { get; set; }

    /// <summary>Gets or sets a value indicating whether completion of this checklist is mandatory.</summary>
    public bool IsMandatory { get; set; }

    /// <summary>Gets or sets the optional regulatory citation this checklist supports.</summary>
    [StringLength(256)]
    public string? RegulatoryReference { get; set; }
}

/// <summary>Result of submitting a new ChecklistTemplate. Mirrors <c>CreateChecklistTemplateResult</c>.</summary>
public sealed record CreateChecklistTemplateResultDto(Guid ChecklistTemplateId, string Status);

/// <summary>Request body for adding an item to a ChecklistTemplate (POST .../items). Mirrors <c>AddChecklistTemplateItemRequest</c>.</summary>
public sealed class AddChecklistTemplateItemRequest
{
    /// <summary>Gets or sets the narrative description of the checklist item.</summary>
    [Required]
    [StringLength(512)]
    public string Description { get; set; } = string.Empty;

    /// <summary>Gets or sets a value indicating whether a failure of this item is a critical (stop-work) failure.</summary>
    public bool IsCritical { get; set; }

    /// <summary>Gets or sets the optional regulatory citation for this item.</summary>
    [StringLength(256)]
    public string? RegulatoryReference { get; set; }
}

/// <summary>Result of adding an item to a ChecklistTemplate. Mirrors <c>AddChecklistTemplateItemResult</c>.</summary>
public sealed record AddChecklistTemplateItemResultDto(Guid ChecklistTemplateId, Guid ItemId, int Sequence);

/// <summary>Result of publishing a ChecklistTemplate. Mirrors <c>PublishChecklistTemplateResult</c>.</summary>
public sealed record PublishChecklistTemplateResultDto(Guid ChecklistTemplateId, string Status, int ItemCount);

/// <summary>Request body for retiring a ChecklistTemplate (POST .../retire). Mirrors <c>RetireChecklistTemplateRequest</c>.</summary>
public sealed class RetireChecklistTemplateRequest
{
    /// <summary>Gets or sets the narrative reason the template is being retired.</summary>
    [Required]
    [StringLength(512)]
    public string Reason { get; set; } = string.Empty;
}

/// <summary>Result of retiring a ChecklistTemplate. Mirrors <c>RetireChecklistTemplateResult</c>.</summary>
public sealed record RetireChecklistTemplateResultDto(Guid ChecklistTemplateId, string Status);
