// ============================================================================
// File: PredictiveSafetyActionFilter.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-12
// Last Updated: 2026-07-12
// Description: Optional per-sub-area action filter for PredictiveSafety endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(PredictiveSafetyActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows PredictiveSafety-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for PredictiveSafety-specific validation or enrichment.
//
// ============================================================================
namespace PlantSafety.Api.Intelligence.PredictiveSafety.Filters;

/// <summary>
/// Optional action filter for PredictiveSafety. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class PredictiveSafetyActionFilter : IActionFilter
{
    private readonly ILogger<PredictiveSafetyActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="PredictiveSafetyActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public PredictiveSafetyActionFilter(ILogger<PredictiveSafetyActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("PredictiveSafety action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}