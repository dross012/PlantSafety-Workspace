// ============================================================================
// File: EquipmentSafetyActionFilter.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Optional per-sub-area action filter for EquipmentSafety endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(EquipmentSafetyActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows EquipmentSafety-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for EquipmentSafety-specific validation or enrichment.
//
// ============================================================================

namespace PlantSafety.Api.Operational.EquipmentSafety.Filters;

/// <summary>
/// Optional action filter for EquipmentSafety. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class EquipmentSafetyActionFilter : IActionFilter
{
    private readonly ILogger<EquipmentSafetyActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="EquipmentSafetyActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public EquipmentSafetyActionFilter(ILogger<EquipmentSafetyActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("EquipmentSafety action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}