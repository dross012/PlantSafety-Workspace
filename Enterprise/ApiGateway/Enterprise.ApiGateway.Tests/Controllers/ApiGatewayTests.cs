// ============================================================================
//  Project:        Enterprise.ApiGateway.Tests
//  File:           ApiGatewayTests.cs
//  Created:        2026-08-08
//  Description:    Phase D.3 unit tests for EnterpriseApiGatewayController:
//                  each of the eight GET actions returns 200 OK with the
//                  correctly-filtered data lake records (or the current
//                  graph), invoked directly against the controller (no
//                  HTTP host needed).
// ============================================================================
using Microsoft.AspNetCore.Mvc;

using Enterprise.ApiGateway.Controllers;
using Enterprise.DataLake.Abstractions.Enums;
using Enterprise.DataLake.Abstractions.Records;
using Enterprise.DataLake.Infrastructure.Query;
using Enterprise.DataLake.Infrastructure.Storage;
using Enterprise.DataLake.Infrastructure.Writing;
using Enterprise.Graph.Abstractions.Enums;
using Enterprise.Graph.Abstractions.Graph;
using Enterprise.Graph.Abstractions.Ids;

using FluentAssertions;

using Xunit;

namespace Enterprise.ApiGateway.Tests.Controllers;

public sealed class ApiGatewayTests
{
    private static EnterpriseApiGatewayController CreateController(IDataLakeStore store, IEnterpriseGraphProvider? graphProvider = null) =>
        new(new DataLakeQueryEngine(store), graphProvider ?? new InMemoryEnterpriseGraphProvider());

    [Fact]
    public void GetFacilities_ReturnsOnlyFacilityRecords()
    {
        var store = new DataLakeStore();
        var writer = new DataLakeWriter(store);
        writer.WriteFacilityRecord(Guid.NewGuid(), "facility payload", DateTime.UtcNow);
        writer.WriteEmployeeRecord(Guid.NewGuid(), "employee payload", DateTime.UtcNow);
        var controller = CreateController(store);

        ActionResult<IReadOnlyList<DataLakeRecord>> result = controller.GetFacilities();

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var records = okResult.Value.Should().BeAssignableTo<IReadOnlyList<DataLakeRecord>>().Subject;
        records.Should().ContainSingle().Which.RecordType.Should().Be(DataLakeRecordType.Facility);
    }

    [Theory]
    [InlineData(DataLakeRecordType.Employee)]
    [InlineData(DataLakeRecordType.Machine)]
    [InlineData(DataLakeRecordType.Hazard)]
    [InlineData(DataLakeRecordType.Event)]
    [InlineData(DataLakeRecordType.Workflow)]
    [InlineData(DataLakeRecordType.Prediction)]
    public void EachTypedEndpoint_ReturnsMatchingRecords(DataLakeRecordType recordType)
    {
        var store = new DataLakeStore();
        var writer = new DataLakeWriter(store);
        writer.WriteFacilityRecord(Guid.NewGuid(), "noise", DateTime.UtcNow);
        WriteOfType(writer, recordType, "target payload");
        var controller = CreateController(store);

        ActionResult<IReadOnlyList<DataLakeRecord>> result = InvokeFor(controller, recordType);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var records = okResult.Value.Should().BeAssignableTo<IReadOnlyList<DataLakeRecord>>().Subject;
        records.Should().ContainSingle().Which.Payload.Should().Be("target payload");
    }

    [Fact]
    public void GetGraph_ReturnsCurrentGraphFromProvider()
    {
        var provider = new InMemoryEnterpriseGraphProvider();
        EnterpriseGraph graph = new EnterpriseGraphBuilder()
            .AddNode(EnterpriseNodeId.New(), EnterpriseNodeType.Facility, Guid.NewGuid(), "Plant 1", DateTime.UtcNow)
            .Build();
        provider.SetCurrentGraph(graph);
        var controller = CreateController(new DataLakeStore(), provider);

        ActionResult<EnterpriseGraph> result = controller.GetGraph();

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(graph);
    }

    private static void WriteOfType(IDataLakeWriter writer, DataLakeRecordType recordType, string payload)
    {
        DateTime now = DateTime.UtcNow;
        Guid sourceId = Guid.NewGuid();

        _ = recordType switch
        {
            DataLakeRecordType.Employee => writer.WriteEmployeeRecord(sourceId, payload, now),
            DataLakeRecordType.Machine => writer.WriteMachineRecord(sourceId, payload, now),
            DataLakeRecordType.Hazard => writer.WriteHazardRecord(sourceId, payload, now),
            DataLakeRecordType.Event => writer.WriteEventRecord(sourceId, payload, now),
            DataLakeRecordType.Workflow => writer.WriteWorkflowRecord(sourceId, payload, now),
            DataLakeRecordType.Prediction => writer.WritePredictionRecord(sourceId, payload, now),
            _ => throw new ArgumentOutOfRangeException(nameof(recordType), recordType, "Unhandled record type in test helper."),
        };
    }

    private static ActionResult<IReadOnlyList<DataLakeRecord>> InvokeFor(EnterpriseApiGatewayController controller, DataLakeRecordType recordType) => recordType switch
    {
        DataLakeRecordType.Employee => controller.GetEmployees(),
        DataLakeRecordType.Machine => controller.GetMachines(),
        DataLakeRecordType.Hazard => controller.GetHazards(),
        DataLakeRecordType.Event => controller.GetEvents(),
        DataLakeRecordType.Workflow => controller.GetWorkflows(),
        DataLakeRecordType.Prediction => controller.GetPredictions(),
        _ => throw new ArgumentOutOfRangeException(nameof(recordType), recordType, "Unhandled record type in test helper."),
    };
}
