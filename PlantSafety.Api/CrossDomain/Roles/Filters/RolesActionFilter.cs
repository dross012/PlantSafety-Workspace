// ============================================================================
// File: RolesActionFilter.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Optional per-sub-area action filter for Roles endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(RolesActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows Roles-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for Roles-specific validation or enrichment.
//
// ============================================================================

namespace PlantSafety.Api.CrossDomain.Roles.Filters;

/// <summary>
/// Optional action filter for Roles. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class RolesActionFilter : IActionFilter
{
    private readonly ILogger<RolesActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="RolesActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public RolesActionFilter(ILogger<RolesActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("Roles action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}