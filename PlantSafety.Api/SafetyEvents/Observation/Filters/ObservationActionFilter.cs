// ============================================================================
// File: ObservationActionFilter.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Optional per-sub-area action filter for Observation endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(ObservationActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows Observation-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for Observation-specific validation or enrichment.
//
// ============================================================================

namespace PlantSafety.Api.SafetyEvents.Observation.Filters;

/// <summary>
/// Optional action filter for Observation. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class ObservationActionFilter : IActionFilter
{
    private readonly ILogger<ObservationActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="ObservationActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public ObservationActionFilter(ILogger<ObservationActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("Observation action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}