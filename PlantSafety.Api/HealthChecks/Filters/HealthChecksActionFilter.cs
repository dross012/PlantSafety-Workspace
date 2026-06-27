// ============================================================================
// File: HealthChecksActionFilter.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Optional per-sub-area action filter for HealthChecks endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(HealthChecksActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows HealthChecks-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for HealthChecks-specific validation or enrichment.
//
// ============================================================================

namespace PlantSafety.Api.HealthChecks.Filters;

/// <summary>
/// Optional action filter for HealthChecks. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class HealthChecksActionFilter : IActionFilter
{
    private readonly ILogger<HealthChecksActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="HealthChecksActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public HealthChecksActionFilter(ILogger<HealthChecksActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("HealthChecks action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}