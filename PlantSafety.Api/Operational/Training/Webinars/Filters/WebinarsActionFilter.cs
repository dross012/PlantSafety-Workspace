// ============================================================================
// File: WebinarsActionFilter.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Optional per-sub-area action filter for Webinars endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(WebinarsActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows Webinars-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for Webinars-specific validation or enrichment.
//
// ============================================================================

namespace PlantSafety.Api.Operational.Training.Webinars.Filters;

/// <summary>
/// Optional action filter for Webinars. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class WebinarsActionFilter : IActionFilter
{
    private readonly ILogger<WebinarsActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="WebinarsActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public WebinarsActionFilter(ILogger<WebinarsActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("Webinars action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}