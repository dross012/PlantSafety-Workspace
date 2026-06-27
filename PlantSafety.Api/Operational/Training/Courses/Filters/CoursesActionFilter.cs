// ============================================================================
// File: CoursesActionFilter.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Optional per-sub-area action filter for Courses endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(CoursesActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows Courses-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for Courses-specific validation or enrichment.
//
// ============================================================================

namespace PlantSafety.Api.Operational.Training.Courses.Filters;

/// <summary>
/// Optional action filter for Courses. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class CoursesActionFilter : IActionFilter
{
    private readonly ILogger<CoursesActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="CoursesActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public CoursesActionFilter(ILogger<CoursesActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("Courses action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}