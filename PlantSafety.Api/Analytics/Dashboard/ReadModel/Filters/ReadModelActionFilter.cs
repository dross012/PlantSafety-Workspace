// ============================================================================
// File: ReadModelActionFilter.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Optional per-sub-area action filter for ReadModel endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(ReadModelActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows ReadModel-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for ReadModel-specific validation or enrichment.
//
// ============================================================================

namespace PlantSafety.Api.Analytics.Dashboard.ReadModel.Filters;

/// <summary>
/// Optional action filter for ReadModel. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class ReadModelActionFilter : IActionFilter
{
    private readonly ILogger<ReadModelActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="ReadModelActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public ReadModelActionFilter(ILogger<ReadModelActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("ReadModel action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}