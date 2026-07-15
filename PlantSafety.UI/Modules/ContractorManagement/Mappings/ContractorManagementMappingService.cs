// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: UI-layer mapping service projecting ContractorItemDto
//   into the display-ready ContractorViewModel shape. Named and shaped
//   after the platform's standard Mapster DTO -> ViewModel convention
//   (Adapt-style pure static projections, no side effects); implemented as
//   hand-written static methods rather than the Mapster package (see
//   Modules/PlanningProjects/Mappings/PlanningProjectsMappingService.cs for
//   the precedent establishing this convention).
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System.Collections.Generic;
using System.Linq;

using PlantSafety.UI.Modules.ContractorManagement.Models;

namespace PlantSafety.UI.Modules.ContractorManagement.Mappings;

/// <summary>Pure, stateless DTO -&gt; ViewModel projections for the Contractor Management module.</summary>
public static class ContractorManagementMappingService
{
    /// <summary>Projects a contractor item into a display-ready view model.</summary>
    public static ContractorViewModel ToViewModel(this ContractorItemDto dto) => new()
    {
        ContractorRecordId = dto.ContractorRecordId,
        ContractorName = dto.ContractorName,
        ServiceType = dto.ServiceType,
        Status = dto.Status,
        StatusIndicatorState = ToStatusIndicatorState(dto.Status),
        OnboardedAtUtc = dto.OnboardedAtUtc,
        ApprovedBy = dto.ApprovedBy,
        TenantId = dto.TenantId,
        SiteId = dto.SiteId,
    };

    /// <summary>Projects a list of contractor items into display-ready view models.</summary>
    public static IReadOnlyList<ContractorViewModel> ToViewModels(this IEnumerable<ContractorItemDto> items) =>
        items.Select(ToViewModel).ToList();

    /// <summary>Maps a raw Contractor status code to the closest StatusIndicatorComponent operational state.</summary>
    /// <param name="status">The raw status code (case-insensitive).</param>
    public static string ToStatusIndicatorState(string status) => status.Trim().ToLowerInvariant() switch
    {
        "active" or "approved" or "onboarded" => "Healthy",
        "pending" or "underreview" or "onboarding" => "Warning",
        "suspended" or "rejected" or "expired" => "Critical",
        "terminated" or "offboarded" or "inactive" => "Offline",
        _ => "Offline",
    };
}
