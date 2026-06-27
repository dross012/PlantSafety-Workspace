// ============================================================================
// File: EmergencyProcedureActionFilter.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Optional per-sub-area action filter for EmergencyProcedure endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(EmergencyProcedureActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows EmergencyProcedure-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for EmergencyProcedure-specific validation or enrichment.
//
// ============================================================================

namespace PlantSafety.Api.Compliance.Procedures.EmergencyProcedure.Filters;

/// <summary>
/// Optional action filter for EmergencyProcedure. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class EmergencyProcedureActionFilter : IActionFilter
{
    private readonly ILogger<EmergencyProcedureActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="EmergencyProcedureActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public EmergencyProcedureActionFilter(ILogger<EmergencyProcedureActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("EmergencyProcedure action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}