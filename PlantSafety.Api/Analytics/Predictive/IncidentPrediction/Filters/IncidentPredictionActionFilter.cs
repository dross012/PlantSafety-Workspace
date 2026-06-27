// ============================================================================
// File: IncidentPredictionActionFilter.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Optional per-sub-area action filter for IncidentPrediction endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(IncidentPredictionActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows IncidentPrediction-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for IncidentPrediction-specific validation or enrichment.
//
// ============================================================================

namespace PlantSafety.Api.Analytics.Predictive.IncidentPrediction.Filters;

/// <summary>
/// Optional action filter for IncidentPrediction. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class IncidentPredictionActionFilter : IActionFilter
{
    private readonly ILogger<IncidentPredictionActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="IncidentPredictionActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public IncidentPredictionActionFilter(ILogger<IncidentPredictionActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("IncidentPrediction action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}