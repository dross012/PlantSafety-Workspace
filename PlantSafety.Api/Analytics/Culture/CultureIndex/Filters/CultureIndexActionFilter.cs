// ============================================================================
// File: CultureIndexActionFilter.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Optional per-sub-area action filter for CultureIndex endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(CultureIndexActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows CultureIndex-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for CultureIndex-specific validation or enrichment.
//
// ============================================================================

namespace PlantSafety.Api.Analytics.Culture.CultureIndex.Filters;

/// <summary>
/// Optional action filter for CultureIndex. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class CultureIndexActionFilter : IActionFilter
{
    private readonly ILogger<CultureIndexActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="CultureIndexActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public CultureIndexActionFilter(ILogger<CultureIndexActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("CultureIndex action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}