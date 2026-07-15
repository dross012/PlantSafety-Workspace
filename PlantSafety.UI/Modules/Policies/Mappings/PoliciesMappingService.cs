// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: UI-layer mapping service projecting Policies & Procedures
//   read-model DTOs into display-ready view models. Named and shaped after
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

using PlantSafety.UI.Modules.Policies.Models;

namespace PlantSafety.UI.Modules.Policies.Mappings;

/// <summary>Pure, stateless DTO -&gt; ViewModel projections for the Policies &amp; Procedures module.</summary>
public static class PoliciesMappingService
{
    /// <summary>Projects a policy item into a detail view model (also used for list rows).</summary>
    public static PolicyDetailViewModel ToDetailViewModel(this PolicyItemDto dto) => new()
    {
        PolicyId = dto.PolicyId,
        PolicyName = dto.PolicyName,
        Category = dto.Category,
        Status = dto.Status,
        StatusIndicatorState = ToStatusIndicatorState(dto.Status),
        EffectiveAtUtc = dto.EffectiveAtUtc,
        OwnerId = dto.OwnerId,
        TenantId = dto.TenantId,
        SiteId = dto.SiteId,
    };

    /// <summary>Projects a procedure item into a detail view model (also used for list rows).</summary>
    public static ProcedureDetailViewModel ToDetailViewModel(this ProcedureItemDto dto) => new()
    {
        SOPId = dto.SOPId,
        Title = dto.Title,
        ProcessArea = dto.ProcessArea,
        Status = dto.Status,
        StatusIndicatorState = ToStatusIndicatorState(dto.Status),
        EffectiveAtUtc = dto.EffectiveAtUtc,
        OwnerId = dto.OwnerId,
        TenantId = dto.TenantId,
        SiteId = dto.SiteId,
    };

    /// <summary>Projects a list of policy items into view models.</summary>
    public static IReadOnlyList<PolicyDetailViewModel> ToViewModels(this IEnumerable<PolicyItemDto> items) =>
        items.Select(ToDetailViewModel).ToList();

    /// <summary>Projects a list of procedure items into view models.</summary>
    public static IReadOnlyList<ProcedureDetailViewModel> ToViewModels(this IEnumerable<ProcedureItemDto> items) =>
        items.Select(ToDetailViewModel).ToList();

    /// <summary>Maps a raw Policy/Procedure approval status code to the closest StatusIndicatorComponent operational state.</summary>
    /// <param name="status">The raw status code (case-insensitive).</param>
    public static string ToStatusIndicatorState(string status) => status.Trim().ToLowerInvariant() switch
    {
        "approved" or "published" or "active" or "effective" => "Healthy",
        "draft" or "pendingapproval" or "underreview" or "inreview" => "Warning",
        "rejected" or "expired" => "Critical",
        "superseded" or "archived" or "retired" => "Offline",
        _ => "Offline",
    };
}
