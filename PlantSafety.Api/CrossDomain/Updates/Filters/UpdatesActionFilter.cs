// ============================================================================
// File: UpdatesActionFilter.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Optional per-sub-area action filter for Updates endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(UpdatesActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows Updates-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for Updates-specific validation or enrichment.
//
// ============================================================================

namespace PlantSafety.Api.CrossDomain.Updates.Filters;

/// <summary>
/// Optional action filter for Updates. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class UpdatesActionFilter : IActionFilter
{
    private readonly ILogger<UpdatesActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="UpdatesActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public UpdatesActionFilter(ILogger<UpdatesActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("Updates action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}