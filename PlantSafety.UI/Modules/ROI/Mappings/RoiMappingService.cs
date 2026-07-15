// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: UI-layer mapping service projecting SafetyRoiDto into
//   the display-ready RoiViewModel shape. Named and shaped after the
//   platform's standard Mapster DTO -> ViewModel convention (Adapt-style
//   pure static projections, no side effects); implemented as hand-written
//   static methods rather than the Mapster package (see
//   Modules/PlanningProjects/Mappings/PlanningProjectsMappingService.cs for
//   the precedent establishing this convention).
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using PlantSafety.UI.Modules.ROI.Models;

namespace PlantSafety.UI.Modules.ROI.Mappings;

/// <summary>Pure, stateless DTO -&gt; ViewModel projections for the ROI module.</summary>
public static class RoiMappingService
{
    /// <summary>Projects a Safety ROI DTO into a display-ready view model.</summary>
    public static RoiViewModel ToViewModel(this SafetyRoiDto dto) => new()
    {
        RoiCalculationId = dto.RoiCalculationId,
        FiscalYear = dto.FiscalYear,
        TotalInvestment = dto.TotalInvestment,
        EstimatedAvoidedCosts = dto.EstimatedAvoidedCosts,
        NetBenefit = dto.NetBenefit,
        RoiPercentage = dto.RoiPercentage,
        StatusIndicatorState = ToStatusIndicatorState(dto.RoiPercentage),
        CurrencyCode = dto.CurrencyCode,
        CalculatedAtUtc = dto.CalculatedAtUtc,
    };

    /// <summary>Maps an ROI percentage to the closest StatusIndicatorComponent operational state.</summary>
    /// <param name="roiPercentage">The computed ROI percentage (NetBenefit / Investment * 100).</param>
    public static string ToStatusIndicatorState(decimal roiPercentage) => roiPercentage switch
    {
        >= 50m => "Healthy",
        >= 0m => "Warning",
        _ => "Critical",
    };
}
