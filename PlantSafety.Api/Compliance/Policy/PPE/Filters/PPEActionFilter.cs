// ============================================================================
// File: PPEActionFilter.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Optional per-sub-area action filter for PPE endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(PPEActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows PPE-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for PPE-specific validation or enrichment.
//
// ============================================================================

namespace PlantSafety.Api.Compliance.Policy.PPE.Filters;

/// <summary>
/// Optional action filter for PPE. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class PPEActionFilter : IActionFilter
{
    private readonly ILogger<PPEActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="PPEActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public PPEActionFilter(ILogger<PPEActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("PPE action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}