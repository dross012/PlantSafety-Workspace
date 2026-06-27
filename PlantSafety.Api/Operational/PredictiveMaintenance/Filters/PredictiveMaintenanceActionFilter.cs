// ============================================================================
// File: PredictiveMaintenanceActionFilter.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Optional per-sub-area action filter for PredictiveMaintenance endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(PredictiveMaintenanceActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows PredictiveMaintenance-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for PredictiveMaintenance-specific validation or enrichment.
//
// ============================================================================

namespace PlantSafety.Api.Operational.PredictiveMaintenance.Filters;

/// <summary>
/// Optional action filter for PredictiveMaintenance. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class PredictiveMaintenanceActionFilter : IActionFilter
{
    private readonly ILogger<PredictiveMaintenanceActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="PredictiveMaintenanceActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public PredictiveMaintenanceActionFilter(ILogger<PredictiveMaintenanceActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("PredictiveMaintenance action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}