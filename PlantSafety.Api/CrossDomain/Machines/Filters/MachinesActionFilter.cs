// ============================================================================
// File: MachinesActionFilter.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Optional per-sub-area action filter for Machines endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(MachinesActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows Machines-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for Machines-specific validation or enrichment.
//
// ============================================================================

namespace PlantSafety.Api.CrossDomain.Machines.Filters;

/// <summary>
/// Optional action filter for Machines. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class MachinesActionFilter : IActionFilter
{
    private readonly ILogger<MachinesActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="MachinesActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public MachinesActionFilter(ILogger<MachinesActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("Machines action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}