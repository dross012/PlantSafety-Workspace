// ============================================================================
// File: OperatorsActionFilter.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Optional per-sub-area action filter for Operators endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(OperatorsActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows Operators-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for Operators-specific validation or enrichment.
//
// ============================================================================

namespace PlantSafety.Api.CrossDomain.Operators.Filters;

/// <summary>
/// Optional action filter for Operators. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class OperatorsActionFilter : IActionFilter
{
    private readonly ILogger<OperatorsActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="OperatorsActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public OperatorsActionFilter(ILogger<OperatorsActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("Operators action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}