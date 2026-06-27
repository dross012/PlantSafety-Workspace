// ============================================================================
// File: UsersActionFilter.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Optional per-sub-area action filter for Users endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(UsersActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows Users-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for Users-specific validation or enrichment.
//
// ============================================================================

namespace PlantSafety.Api.CrossDomain.Users.Filters;

/// <summary>
/// Optional action filter for Users. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class UsersActionFilter : IActionFilter
{
    private readonly ILogger<UsersActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="UsersActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public UsersActionFilter(ILogger<UsersActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("Users action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}