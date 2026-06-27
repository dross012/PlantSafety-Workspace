// ============================================================================
// File: ChecklistsActionFilter.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Optional per-sub-area action filter for Checklists endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(ChecklistsActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows Checklists-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for Checklists-specific validation or enrichment.
//
// ============================================================================

namespace PlantSafety.Api.Operational.Checklists.Filters;

/// <summary>
/// Optional action filter for Checklists. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class ChecklistsActionFilter : IActionFilter
{
    private readonly ILogger<ChecklistsActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="ChecklistsActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public ChecklistsActionFilter(ILogger<ChecklistsActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("Checklists action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}