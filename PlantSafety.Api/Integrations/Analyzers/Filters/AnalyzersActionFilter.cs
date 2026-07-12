// ============================================================================
// File: AnalyzersActionFilter.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-07-12
// Last Updated: 2026-07-12
// Description: Optional per-sub-area action filter for Integrations.Analyzers endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(AnalyzersActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows Integrations.Analyzers-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for Integrations.Analyzers-specific validation or enrichment.
//
// ============================================================================

namespace PlantSafety.Api.Integrations.Analyzers.Filters;

/// <summary>
/// Optional action filter for Integrations.Analyzers. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class AnalyzersActionFilter : IActionFilter
{
    private readonly ILogger<AnalyzersActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="AnalyzersActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public AnalyzersActionFilter(ILogger<AnalyzersActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("Integrations.Analyzers action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}
