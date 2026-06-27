// ============================================================================
// File: EmergencyManagementActionFilter.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Optional per-sub-area action filter for EmergencyManagement endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(EmergencyManagementActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows EmergencyManagement-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for EmergencyManagement-specific validation or enrichment.
//
// ============================================================================

namespace PlantSafety.Api.SafetyEvents.EmergencyManagement.Filters;

/// <summary>
/// Optional action filter for EmergencyManagement. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class EmergencyManagementActionFilter : IActionFilter
{
    private readonly ILogger<EmergencyManagementActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="EmergencyManagementActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public EmergencyManagementActionFilter(ILogger<EmergencyManagementActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("EmergencyManagement action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}