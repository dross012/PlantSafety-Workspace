// ============================================================================
// File: ISO14001ActionFilter.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Optional per-sub-area action filter for ISO14001 endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(ISO14001ActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows ISO14001-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for ISO14001-specific validation or enrichment.
//
// ============================================================================

namespace PlantSafety.Api.Compliance.Certification.ISO14001.Filters;

/// <summary>
/// Optional action filter for ISO14001. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class ISO14001ActionFilter : IActionFilter
{
    private readonly ILogger<ISO14001ActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="ISO14001ActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public ISO14001ActionFilter(ILogger<ISO14001ActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("ISO14001 action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}