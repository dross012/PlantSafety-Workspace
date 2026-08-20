// ============================================================================
// File: EnvironmentalMonitoringActionFilter.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-08-20
// Last Updated: 2026-08-20
// Description: Optional per-sub-area action filter for EnvironmentalMonitoring endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(EnvironmentalMonitoringActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows EnvironmentalMonitoring-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for EnvironmentalMonitoring-specific validation or enrichment.
//
// ============================================================================

namespace PlantSafety.Api.Operational.EnvironmentalMonitoring.Filters;

/// <summary>
/// Optional action filter for EnvironmentalMonitoring. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class EnvironmentalMonitoringActionFilter : IActionFilter
{
    private readonly ILogger<EnvironmentalMonitoringActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="EnvironmentalMonitoringActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public EnvironmentalMonitoringActionFilter(ILogger<EnvironmentalMonitoringActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("EnvironmentalMonitoring action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}