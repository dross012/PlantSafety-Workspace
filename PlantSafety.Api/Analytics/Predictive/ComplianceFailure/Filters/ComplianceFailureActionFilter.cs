// ============================================================================
// File: ComplianceFailureActionFilter.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Optional per-sub-area action filter for ComplianceFailure endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(ComplianceFailureActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows ComplianceFailure-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for ComplianceFailure-specific validation or enrichment.
//
// ============================================================================

namespace PlantSafety.Api.Analytics.Predictive.ComplianceFailure.Filters;

/// <summary>
/// Optional action filter for ComplianceFailure. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class ComplianceFailureActionFilter : IActionFilter
{
    private readonly ILogger<ComplianceFailureActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="ComplianceFailureActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public ComplianceFailureActionFilter(ILogger<ComplianceFailureActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("ComplianceFailure action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}