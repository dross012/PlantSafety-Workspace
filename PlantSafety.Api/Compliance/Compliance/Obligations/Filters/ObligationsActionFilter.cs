// ============================================================================
// File: ObligationsActionFilter.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Optional per-sub-area action filter for Obligations endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(ObligationsActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows Obligations-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for Obligations-specific validation or enrichment.
//
// ============================================================================

namespace PlantSafety.Api.Compliance.Compliance.Obligations.Filters;

/// <summary>
/// Optional action filter for Obligations. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class ObligationsActionFilter : IActionFilter
{
    private readonly ILogger<ObligationsActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="ObligationsActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public ObligationsActionFilter(ILogger<ObligationsActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("Obligations action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}