// ============================================================================
// File: AITrainingActionFilter.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-12
// Last Updated: 2026-07-12
// Description: Optional per-sub-area action filter for AITraining endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(AITrainingActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows AITraining-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for AITraining-specific validation or enrichment.
//
// ============================================================================
namespace PlantSafety.Api.Intelligence.AITraining.Filters;

/// <summary>
/// Optional action filter for AITraining. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class AITrainingActionFilter : IActionFilter
{
    private readonly ILogger<AITrainingActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="AITrainingActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public AITrainingActionFilter(ILogger<AITrainingActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("AITraining action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}