// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: UI-layer mapping service projecting ChecklistItemDto
//   into the display-ready ChecklistViewModel shape. Named and shaped after
//   the platform's standard Mapster DTO -> ViewModel convention (Adapt-style
//   pure static projections, no side effects); implemented as hand-written
//   static methods rather than the Mapster package (see
//   Modules/PlanningProjects/Mappings/PlanningProjectsMappingService.cs for
//   the precedent establishing this convention).
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System.Collections.Generic;
using System.Linq;

using PlantSafety.UI.Modules.Checklist.Models;

namespace PlantSafety.UI.Modules.Checklist.Mappings;

/// <summary>Pure, stateless DTO -&gt; ViewModel projections for the Checklist module.</summary>
public static class ChecklistMappingService
{
    /// <summary>Projects a checklist item into a display-ready view model.</summary>
    public static ChecklistViewModel ToViewModel(this ChecklistItemDto dto) => new()
    {
        ChecklistId = dto.ChecklistId,
        Name = dto.Name,
        Category = dto.Category,
        Status = dto.Status,
        StatusIndicatorState = ToStatusIndicatorState(dto.Status),
        CompletedAtUtc = dto.CompletedAtUtc,
        CompletedBy = dto.CompletedBy,
        TenantId = dto.TenantId,
        SiteId = dto.SiteId,
    };

    /// <summary>Projects a list of checklist items into display-ready view models.</summary>
    public static IReadOnlyList<ChecklistViewModel> ToViewModels(this IEnumerable<ChecklistItemDto> items) =>
        items.Select(ToViewModel).ToList();

    /// <summary>Maps a raw Checklist status code to the closest StatusIndicatorComponent operational state.</summary>
    /// <param name="status">The raw status code (case-insensitive).</param>
    public static string ToStatusIndicatorState(string status) => status.Trim().ToLowerInvariant() switch
    {
        "completed" or "passed" or "signedoff" => "Healthy",
        "inprogress" or "pending" or "draft" => "Warning",
        "failed" or "overdue" => "Critical",
        "cancelled" or "archived" => "Offline",
        _ => "Offline",
    };
}
