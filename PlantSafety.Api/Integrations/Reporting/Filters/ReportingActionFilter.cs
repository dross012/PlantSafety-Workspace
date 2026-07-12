// ============================================================================
// File: ReportingActionFilter.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-07-12
// Last Updated: 2026-07-12
// Description: Optional per-sub-area action filter for Integrations.Reporting endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(ReportingActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows Integrations.Reporting-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for Integrations.Reporting-specific validation or enrichment.
//
// ============================================================================

namespace PlantSafety.Api.Integrations.Reporting.Filters;

/// <summary>
/// Optional action filter for Integrations.Reporting. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class ReportingActionFilter : IActionFilter
{
    private readonly ILogger<ReportingActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="ReportingActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public ReportingActionFilter(ILogger<ReportingActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("Integrations.Reporting action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}
