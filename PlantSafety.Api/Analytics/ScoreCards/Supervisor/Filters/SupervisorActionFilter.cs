// ============================================================================
// File: SupervisorActionFilter.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Optional per-sub-area action filter for Supervisor endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(SupervisorActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows Supervisor-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for Supervisor-specific validation or enrichment.
//
// ============================================================================

namespace PlantSafety.Api.Analytics.ScoreCards.Supervisor.Filters;

/// <summary>
/// Optional action filter for Supervisor. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class SupervisorActionFilter : IActionFilter
{
    private readonly ILogger<SupervisorActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="SupervisorActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public SupervisorActionFilter(ILogger<SupervisorActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("Supervisor action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}