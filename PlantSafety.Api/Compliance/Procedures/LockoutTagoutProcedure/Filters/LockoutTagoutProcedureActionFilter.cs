// ============================================================================
// File: LockoutTagoutProcedureActionFilter.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Optional per-sub-area action filter for LockoutTagoutProcedure endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(LockoutTagoutProcedureActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows LockoutTagoutProcedure-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for LockoutTagoutProcedure-specific validation or enrichment.
//
// ============================================================================

namespace PlantSafety.Api.Compliance.Procedures.LockoutTagoutProcedure.Filters;

/// <summary>
/// Optional action filter for LockoutTagoutProcedure. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class LockoutTagoutProcedureActionFilter : IActionFilter
{
    private readonly ILogger<LockoutTagoutProcedureActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="LockoutTagoutProcedureActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public LockoutTagoutProcedureActionFilter(ILogger<LockoutTagoutProcedureActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("LockoutTagoutProcedure action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}