// ============================================================================
// File: WorkInstructionsActionFilter.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Optional per-sub-area action filter for WorkInstructions endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(WorkInstructionsActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows WorkInstructions-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for WorkInstructions-specific validation or enrichment.
//
// ============================================================================

namespace PlantSafety.Api.Compliance.Procedures.WorkInstructions.Filters;

/// <summary>
/// Optional action filter for WorkInstructions. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class WorkInstructionsActionFilter : IActionFilter
{
    private readonly ILogger<WorkInstructionsActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="WorkInstructionsActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public WorkInstructionsActionFilter(ILogger<WorkInstructionsActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("WorkInstructions action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}