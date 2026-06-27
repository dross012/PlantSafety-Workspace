// ============================================================================
// File: PlanningProjectsActionFilter.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Optional per-sub-area action filter for PlanningProjects endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(PlanningProjectsActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows PlanningProjects-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for PlanningProjects-specific validation or enrichment.
//
// ============================================================================

namespace PlantSafety.Api.PlanningProjects.Filters;

/// <summary>
/// Optional action filter for PlanningProjects. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class PlanningProjectsActionFilter : IActionFilter
{
    private readonly ILogger<PlanningProjectsActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="PlanningProjectsActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public PlanningProjectsActionFilter(ILogger<PlanningProjectsActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("PlanningProjects action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}