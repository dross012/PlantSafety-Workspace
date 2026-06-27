// ============================================================================
// File: EmployeesActionFilter.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Optional per-sub-area action filter for Employees endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(EmployeesActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows Employees-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for Employees-specific validation or enrichment.
//
// ============================================================================

namespace PlantSafety.Api.CrossDomain.Employees.Filters;

/// <summary>
/// Optional action filter for Employees. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class EmployeesActionFilter : IActionFilter
{
    private readonly ILogger<EmployeesActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="EmployeesActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public EmployeesActionFilter(ILogger<EmployeesActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("Employees action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}