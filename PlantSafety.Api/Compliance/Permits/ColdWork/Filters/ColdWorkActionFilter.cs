// ============================================================================
// File: ColdWorkActionFilter.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Optional per-sub-area action filter for ColdWork endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(ColdWorkActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows ColdWork-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for ColdWork-specific validation or enrichment.
//
// ============================================================================

namespace PlantSafety.Api.Compliance.Permits.ColdWork.Filters;

/// <summary>
/// Optional action filter for ColdWork. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class ColdWorkActionFilter : IActionFilter
{
    private readonly ILogger<ColdWorkActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="ColdWorkActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public ColdWorkActionFilter(ILogger<ColdWorkActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("ColdWork action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}