// ============================================================================
// File: SchedulesActionFilter.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Optional per-sub-area action filter for Schedules endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(SchedulesActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows Schedules-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for Schedules-specific validation or enrichment.
//
// ============================================================================

namespace PlantSafety.Api.Analytics.Schedules.Filters;

/// <summary>
/// Optional action filter for Schedules. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class SchedulesActionFilter : IActionFilter
{
    private readonly ILogger<SchedulesActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="SchedulesActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public SchedulesActionFilter(ILogger<SchedulesActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("Schedules action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}