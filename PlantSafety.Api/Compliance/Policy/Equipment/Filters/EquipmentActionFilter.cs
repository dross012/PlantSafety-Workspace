// ============================================================================
// File: EquipmentActionFilter.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Optional per-sub-area action filter for Equipment endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(EquipmentActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows Equipment-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for Equipment-specific validation or enrichment.
//
// ============================================================================

namespace PlantSafety.Api.Compliance.Policy.Equipment.Filters;

/// <summary>
/// Optional action filter for Equipment. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class EquipmentActionFilter : IActionFilter
{
    private readonly ILogger<EquipmentActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="EquipmentActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public EquipmentActionFilter(ILogger<EquipmentActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("Equipment action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}