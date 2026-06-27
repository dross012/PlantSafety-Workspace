// ============================================================================
// File: DepartmentActionFilter.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Optional per-sub-area action filter for Department endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(DepartmentActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows Department-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for Department-specific validation or enrichment.
//
// ============================================================================

namespace PlantSafety.Api.Analytics.ScoreCards.Department.Filters;

/// <summary>
/// Optional action filter for Department. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class DepartmentActionFilter : IActionFilter
{
    private readonly ILogger<DepartmentActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="DepartmentActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public DepartmentActionFilter(ILogger<DepartmentActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("Department action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}