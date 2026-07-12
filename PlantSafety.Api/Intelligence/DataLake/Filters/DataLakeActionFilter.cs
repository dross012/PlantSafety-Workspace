// ============================================================================
// File: DataLakeActionFilter.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-12
// Last Updated: 2026-07-12
// Description: Optional per-sub-area action filter for DataLake endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(DataLakeActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows DataLake-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for DataLake-specific validation or enrichment.
//
// ============================================================================
namespace PlantSafety.Api.Intelligence.DataLake.Filters;

/// <summary>
/// Optional action filter for DataLake. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class DataLakeActionFilter : IActionFilter
{
    private readonly ILogger<DataLakeActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="DataLakeActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public DataLakeActionFilter(ILogger<DataLakeActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("DataLake action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}