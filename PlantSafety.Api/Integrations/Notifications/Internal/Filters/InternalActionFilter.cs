// ============================================================================
// File: InternalActionFilter.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-12
// Last Updated: 2026-07-12
// Description: Optional per-sub-area action filter for Internal endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(InternalActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows Internal-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for Internal-specific validation or enrichment.
//
// ============================================================================
namespace PlantSafety.Api.Integrations.Notifications.Internal.Filters;

/// <summary>
/// Optional action filter for Internal. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class InternalActionFilter : IActionFilter
{
    private readonly ILogger<InternalActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="InternalActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public InternalActionFilter(ILogger<InternalActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("Internal action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}