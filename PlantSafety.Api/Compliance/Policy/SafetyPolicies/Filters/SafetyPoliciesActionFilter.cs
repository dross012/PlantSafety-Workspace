// ============================================================================
// File: SafetyPoliciesActionFilter.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Optional per-sub-area action filter for SafetyPolicies endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(SafetyPoliciesActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows SafetyPolicies-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for SafetyPolicies-specific validation or enrichment.
//
// ============================================================================

namespace PlantSafety.Api.Compliance.Policy.SafetyPolicies.Filters;

/// <summary>
/// Optional action filter for SafetyPolicies. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class SafetyPoliciesActionFilter : IActionFilter
{
    private readonly ILogger<SafetyPoliciesActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="SafetyPoliciesActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public SafetyPoliciesActionFilter(ILogger<SafetyPoliciesActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("SafetyPolicies action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}