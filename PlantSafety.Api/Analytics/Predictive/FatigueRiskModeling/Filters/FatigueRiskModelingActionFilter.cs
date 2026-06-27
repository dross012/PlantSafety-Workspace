// ============================================================================
// File: FatigueRiskModelingActionFilter.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Optional per-sub-area action filter for FatigueRiskModeling endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(FatigueRiskModelingActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows FatigueRiskModeling-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for FatigueRiskModeling-specific validation or enrichment.
//
// ============================================================================

namespace PlantSafety.Api.Analytics.Predictive.FatigueRiskModeling.Filters;

/// <summary>
/// Optional action filter for FatigueRiskModeling. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class FatigueRiskModelingActionFilter : IActionFilter
{
    private readonly ILogger<FatigueRiskModelingActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="FatigueRiskModelingActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public FatigueRiskModelingActionFilter(ILogger<FatigueRiskModelingActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("FatigueRiskModeling action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}