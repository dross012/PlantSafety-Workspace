// ============================================================================
// File: AlertsActionFilter.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Optional per-sub-area action filter for Alerts endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(AlertsActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows Alerts-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for Alerts-specific validation or enrichment.
//
// ============================================================================

namespace PlantSafety.Api.Operational.Alerts.Filters;

/// <summary>
/// Optional action filter for Alerts. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class AlertsActionFilter : IActionFilter
{
    private readonly ILogger<AlertsActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="AlertsActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public AlertsActionFilter(ILogger<AlertsActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("Alerts action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}