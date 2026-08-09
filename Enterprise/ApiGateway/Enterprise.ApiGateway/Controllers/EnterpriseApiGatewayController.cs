// ============================================================================
//  Project:        Enterprise.ApiGateway
//  Module:         Controllers
//  File:           EnterpriseApiGatewayController.cs
//  Created:        2026-08-08
//  Description:    Phase D.3: the unified enterprise API gateway --
//                  exposes exactly the eight GET endpoints Path D's spec
//                  requires. Facilities/Employees/Machines/Hazards/Events/
//                  Workflows/Predictions each read the matching
//                  DataLakeRecordType from Enterprise.DataLake's
//                  IDataLakeQueryEngine (Phase D.2); Graph reads the
//                  current EnterpriseGraph via IEnterpriseGraphProvider
//                  (Phase D.1). No new query logic lives here -- this
//                  controller is purely a thin, unified read surface over
//                  those two already-built services.
// ============================================================================
using Microsoft.AspNetCore.Mvc;

using Enterprise.ApiGateway.Routing;
using Enterprise.DataLake.Abstractions.Enums;
using Enterprise.DataLake.Abstractions.Records;
using Enterprise.DataLake.Infrastructure.Query;
using Enterprise.Graph.Abstractions.Graph;

namespace Enterprise.ApiGateway.Controllers;

/// <summary>The unified enterprise API gateway.</summary>
public sealed class EnterpriseApiGatewayController(IDataLakeQueryEngine dataLakeQueryEngine, IEnterpriseGraphProvider graphProvider) : EnterpriseApiControllerBase
{
    /// <summary>GET api/v1/enterprise/facilities -- every Facility record in the enterprise data lake.</summary>
    [HttpGet(EnterpriseApiRoute.Facilities)]
    public ActionResult<IReadOnlyList<DataLakeRecord>> GetFacilities() =>
        Ok(dataLakeQueryEngine.QueryByType(DataLakeRecordType.Facility));

    /// <summary>GET api/v1/enterprise/employees -- every Employee record in the enterprise data lake.</summary>
    [HttpGet(EnterpriseApiRoute.Employees)]
    public ActionResult<IReadOnlyList<DataLakeRecord>> GetEmployees() =>
        Ok(dataLakeQueryEngine.QueryByType(DataLakeRecordType.Employee));

    /// <summary>GET api/v1/enterprise/machines -- every Machine record in the enterprise data lake.</summary>
    [HttpGet(EnterpriseApiRoute.Machines)]
    public ActionResult<IReadOnlyList<DataLakeRecord>> GetMachines() =>
        Ok(dataLakeQueryEngine.QueryByType(DataLakeRecordType.Machine));

    /// <summary>GET api/v1/enterprise/hazards -- every Hazard record in the enterprise data lake.</summary>
    [HttpGet(EnterpriseApiRoute.Hazards)]
    public ActionResult<IReadOnlyList<DataLakeRecord>> GetHazards() =>
        Ok(dataLakeQueryEngine.QueryByType(DataLakeRecordType.Hazard));

    /// <summary>GET api/v1/enterprise/events -- every Event record in the enterprise data lake.</summary>
    [HttpGet(EnterpriseApiRoute.Events)]
    public ActionResult<IReadOnlyList<DataLakeRecord>> GetEvents() =>
        Ok(dataLakeQueryEngine.QueryByType(DataLakeRecordType.Event));

    /// <summary>GET api/v1/enterprise/workflows -- every Workflow record in the enterprise data lake.</summary>
    [HttpGet(EnterpriseApiRoute.Workflows)]
    public ActionResult<IReadOnlyList<DataLakeRecord>> GetWorkflows() =>
        Ok(dataLakeQueryEngine.QueryByType(DataLakeRecordType.Workflow));

    /// <summary>GET api/v1/enterprise/predictions -- every Prediction record in the enterprise data lake.</summary>
    [HttpGet(EnterpriseApiRoute.Predictions)]
    public ActionResult<IReadOnlyList<DataLakeRecord>> GetPredictions() =>
        Ok(dataLakeQueryEngine.QueryByType(DataLakeRecordType.Prediction));

    /// <summary>GET api/v1/enterprise/graph -- the current unified enterprise safety graph.</summary>
    [HttpGet(EnterpriseApiRoute.Graph)]
    public ActionResult<EnterpriseGraph> GetGraph() =>
        Ok(graphProvider.GetCurrentGraph());
}
