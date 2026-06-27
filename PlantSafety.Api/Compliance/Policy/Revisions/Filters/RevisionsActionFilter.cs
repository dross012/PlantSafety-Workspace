// ============================================================================
// File: RevisionsActionFilter.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Optional per-sub-area action filter for Revisions endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(RevisionsActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows Revisions-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for Revisions-specific validation or enrichment.
//
// ============================================================================

namespace PlantSafety.Api.Compliance.Policy.Revisions.Filters;

/// <summary>
/// Optional action filter for Revisions. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class RevisionsActionFilter : IActionFilter
{
    private readonly ILogger<RevisionsActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="RevisionsActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public RevisionsActionFilter(ILogger<RevisionsActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("Revisions action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}