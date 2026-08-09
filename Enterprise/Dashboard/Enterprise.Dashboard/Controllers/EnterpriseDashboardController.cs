// ============================================================================
//  Project:        Enterprise.Dashboard
//  Module:         Controllers
//  File:           EnterpriseDashboardController.cs
//  Created:        2026-08-08
//  Description:    Phase D.4: exposes the aggregated enterprise dashboard.
//                  Thin -- all aggregation logic lives in
//                  EnterpriseDashboardProfile.Build; this controller only
//                  wires the request to it.
// ============================================================================
using Microsoft.AspNetCore.Mvc;

using Enterprise.Dashboard.Dtos;
using Enterprise.Dashboard.Profiles;
using Enterprise.DataLake.Infrastructure.Query;

namespace Enterprise.Dashboard.Controllers;

/// <summary>Exposes the aggregated enterprise dashboard.</summary>
[ApiController]
[Route("api/v1/enterprise/dashboard")]
public sealed class EnterpriseDashboardController(IDataLakeQueryEngine dataLakeQueryEngine) : ControllerBase
{
    /// <summary>GET api/v1/enterprise/dashboard -- the aggregated enterprise dashboard payload.</summary>
    [HttpGet]
    public ActionResult<EnterpriseDashboardDto> GetDashboard() =>
        Ok(EnterpriseDashboardProfile.Build(dataLakeQueryEngine));
}
