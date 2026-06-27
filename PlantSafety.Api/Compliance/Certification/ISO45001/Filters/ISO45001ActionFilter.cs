// ============================================================================
// File: ISO45001ActionFilter.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Optional per-sub-area action filter for ISO45001 endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(ISO45001ActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows ISO45001-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for ISO45001-specific validation or enrichment.
//
// ============================================================================

namespace PlantSafety.Api.Compliance.Certification.ISO45001.Filters;

/// <summary>
/// Optional action filter for ISO45001. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class ISO45001ActionFilter : IActionFilter
{
    private readonly ILogger<ISO45001ActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="ISO45001ActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public ISO45001ActionFilter(ILogger<ISO45001ActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("ISO45001 action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}