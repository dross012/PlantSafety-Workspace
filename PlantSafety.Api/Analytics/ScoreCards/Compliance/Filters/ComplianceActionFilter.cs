// ============================================================================
// File: ComplianceActionFilter.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Optional per-sub-area action filter for Compliance endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(ComplianceActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows Compliance-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for Compliance-specific validation or enrichment.
//
// ============================================================================

namespace PlantSafety.Api.Analytics.ScoreCards.Compliance.Filters;

/// <summary>
/// Optional action filter for Compliance. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class ComplianceActionFilter : IActionFilter
{
    private readonly ILogger<ComplianceActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="ComplianceActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public ComplianceActionFilter(ILogger<ComplianceActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("Compliance action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}