// ============================================================================
// File: SeminarsActionFilter.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Optional per-sub-area action filter for Seminars endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(SeminarsActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows Seminars-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for Seminars-specific validation or enrichment.
//
// ============================================================================

namespace PlantSafety.Api.Operational.Training.Seminars.Filters;

/// <summary>
/// Optional action filter for Seminars. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class SeminarsActionFilter : IActionFilter
{
    private readonly ILogger<SeminarsActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="SeminarsActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public SeminarsActionFilter(ILogger<SeminarsActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("Seminars action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}