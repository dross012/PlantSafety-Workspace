// ============================================================================
// File: TrainingActionFilter.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Optional per-sub-area action filter for Training endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(TrainingActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows Training-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for Training-specific validation or enrichment.
//
// ============================================================================

namespace PlantSafety.Api.Analytics.ScoreCards.Training.Filters;

/// <summary>
/// Optional action filter for Training. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class TrainingActionFilter : IActionFilter
{
    private readonly ILogger<TrainingActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="TrainingActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public TrainingActionFilter(ILogger<TrainingActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("Training action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}