// ============================================================================
// File: DrillsAndExercisesActionFilter.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Optional per-sub-area action filter for DrillsAndExercises endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(DrillsAndExercisesActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows DrillsAndExercises-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for DrillsAndExercises-specific validation or enrichment.
//
// ============================================================================

namespace PlantSafety.Api.SafetyEvents.DrillsAndExercises.Filters;

/// <summary>
/// Optional action filter for DrillsAndExercises. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class DrillsAndExercisesActionFilter : IActionFilter
{
    private readonly ILogger<DrillsAndExercisesActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="DrillsAndExercisesActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public DrillsAndExercisesActionFilter(ILogger<DrillsAndExercisesActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("DrillsAndExercises action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}