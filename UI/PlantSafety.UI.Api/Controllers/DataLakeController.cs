// ============================================================================
//  Project:        PlantSafety.UI.Api
//  Module:         Controllers
//  File:           DataLakeController.cs
//  Created:        2026-08-09
//  Description:    Write + count surface over Enterprise.DataLake's real
//                  IDataLakeWriter/IDataLakeStore (Phase D.2). Read-by-type
//                  is already exposed via Enterprise.ApiGateway's
//                  EnterpriseApiGatewayController (mounted as an
//                  ApplicationPart in Program.cs) -- this controller adds
//                  only what the gateway doesn't: writing new records and a
//                  per-type count summary for the DataLake dashboard page.
// ============================================================================
using Microsoft.AspNetCore.Mvc;

using Enterprise.DataLake.Abstractions.Enums;
using Enterprise.DataLake.Abstractions.Records;
using Enterprise.DataLake.Infrastructure.Query;
using Enterprise.DataLake.Infrastructure.Writing;

namespace PlantSafety.UI.Api.Controllers;

/// <summary>Request body for writing a new data lake record.</summary>
public sealed record DataLakeWriteRequest(Guid SourceId, string Payload);

/// <summary>Exposes Enterprise.DataLake's write API and per-type counts to PlantSafety.UI.</summary>
[ApiController]
[Route("api/v1/ui/datalake")]
public sealed class DataLakeController(IDataLakeWriter writer, IDataLakeQueryEngine queryEngine) : ControllerBase
{
    /// <summary>GET api/v1/ui/datalake/counts -- record count per DataLakeRecordType.</summary>
    [HttpGet("counts")]
    public ActionResult<IReadOnlyDictionary<DataLakeRecordType, int>> GetCounts() => Ok(queryEngine.CountByType());

    /// <summary>POST api/v1/ui/datalake/{recordType} -- writes a new record of the given type.</summary>
    [HttpPost("{recordType}")]
    public ActionResult<DataLakeRecord> Write(DataLakeRecordType recordType, [FromBody] DataLakeWriteRequest request)
    {
        DateTime now = DateTime.UtcNow;
        DataLakeRecord record = recordType switch
        {
            DataLakeRecordType.Facility => writer.WriteFacilityRecord(request.SourceId, request.Payload, now),
            DataLakeRecordType.Employee => writer.WriteEmployeeRecord(request.SourceId, request.Payload, now),
            DataLakeRecordType.Machine => writer.WriteMachineRecord(request.SourceId, request.Payload, now),
            DataLakeRecordType.Hazard => writer.WriteHazardRecord(request.SourceId, request.Payload, now),
            DataLakeRecordType.Event => writer.WriteEventRecord(request.SourceId, request.Payload, now),
            DataLakeRecordType.Workflow => writer.WriteWorkflowRecord(request.SourceId, request.Payload, now),
            DataLakeRecordType.Prediction => writer.WritePredictionRecord(request.SourceId, request.Payload, now),
            _ => throw new ArgumentOutOfRangeException(nameof(recordType)),
        };

        return Ok(record);
    }
}
