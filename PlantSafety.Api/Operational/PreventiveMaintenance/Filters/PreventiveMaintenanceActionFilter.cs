// ============================================================================
// File: PreventiveMaintenanceActionFilter.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Optional per-sub-area action filter for PreventiveMaintenance endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(PreventiveMaintenanceActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows PreventiveMaintenance-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for PreventiveMaintenance-specific validation or enrichment.
//
// ============================================================================

namespace PlantSafety.Api.Operational.PreventiveMaintenance.Filters;

/// <summary>
/// Optional action filter for PreventiveMaintenance. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class PreventiveMaintenanceActionFilter : IActionFilter
{
    private readonly ILogger<PreventiveMaintenanceActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="PreventiveMaintenanceActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public PreventiveMaintenanceActionFilter(ILogger<PreventiveMaintenanceActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("PreventiveMaintenance action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}