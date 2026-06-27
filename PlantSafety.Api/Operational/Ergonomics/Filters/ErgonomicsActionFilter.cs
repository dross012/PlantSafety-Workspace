// ============================================================================
// File: ErgonomicsActionFilter.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Optional per-sub-area action filter for Ergonomics endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(ErgonomicsActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows Ergonomics-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for Ergonomics-specific validation or enrichment.
//
// ============================================================================

namespace PlantSafety.Api.Operational.Ergonomics.Filters;

/// <summary>
/// Optional action filter for Ergonomics. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class ErgonomicsActionFilter : IActionFilter
{
    private readonly ILogger<ErgonomicsActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="ErgonomicsActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public ErgonomicsActionFilter(ILogger<ErgonomicsActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("Ergonomics action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}