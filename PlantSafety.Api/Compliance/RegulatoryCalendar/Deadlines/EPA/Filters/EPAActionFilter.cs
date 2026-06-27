// ============================================================================
// File: EPAActionFilter.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Optional per-sub-area action filter for EPA endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(EPAActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows EPA-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for EPA-specific validation or enrichment.
//
// ============================================================================

namespace PlantSafety.Api.Compliance.RegulatoryCalendar.Deadlines.EPA.Filters;

/// <summary>
/// Optional action filter for EPA. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class EPAActionFilter : IActionFilter
{
    private readonly ILogger<EPAActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="EPAActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public EPAActionFilter(ILogger<EPAActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("EPA action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}