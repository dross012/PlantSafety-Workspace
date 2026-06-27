// ============================================================================
// File: HazardousMaterialsActionFilter.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Optional per-sub-area action filter for HazardousMaterials endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(HazardousMaterialsActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows HazardousMaterials-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for HazardousMaterials-specific validation or enrichment.
//
// ============================================================================

namespace PlantSafety.Api.Operational.HazardousMaterials.Filters;

/// <summary>
/// Optional action filter for HazardousMaterials. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class HazardousMaterialsActionFilter : IActionFilter
{
    private readonly ILogger<HazardousMaterialsActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="HazardousMaterialsActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public HazardousMaterialsActionFilter(ILogger<HazardousMaterialsActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("HazardousMaterials action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}