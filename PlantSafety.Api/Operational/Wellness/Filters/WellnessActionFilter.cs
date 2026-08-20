// ============================================================================
// File: WellnessActionFilter.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-08-20
// Last Updated: 2026-08-20
// Description: Optional per-sub-area action filter for Wellness endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(WellnessActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows Wellness-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for Wellness-specific validation or enrichment.
//
// ============================================================================

namespace PlantSafety.Api.Operational.Wellness.Filters;

/// <summary>
/// Optional action filter for Wellness. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class WellnessActionFilter : IActionFilter
{
    private readonly ILogger<WellnessActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="WellnessActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public WellnessActionFilter(ILogger<WellnessActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("Wellness action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}