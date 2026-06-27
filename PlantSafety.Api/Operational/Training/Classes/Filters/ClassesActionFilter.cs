// ============================================================================
// File: ClassesActionFilter.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Optional per-sub-area action filter for Classes endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(ClassesActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows Classes-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for Classes-specific validation or enrichment.
//
// ============================================================================

namespace PlantSafety.Api.Operational.Training.Classes.Filters;

/// <summary>
/// Optional action filter for Classes. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class ClassesActionFilter : IActionFilter
{
    private readonly ILogger<ClassesActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="ClassesActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public ClassesActionFilter(ILogger<ClassesActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("Classes action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}