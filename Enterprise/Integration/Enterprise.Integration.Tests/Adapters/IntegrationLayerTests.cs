// ============================================================================
//  Project:        Enterprise.Integration.Tests
//  File:           IntegrationLayerTests.cs
//  Created:        2026-08-08
//  Description:    Phase D.9 unit tests for EnterpriseIntegrationAdapter and
//                  EnterpriseIntegrationController: field mapping (mapped
//                  fields translate, unmapped fields drop), blank event
//                  type rejection, and controller routing (known system
//                  adapts, unknown system 404s).
// ============================================================================
using Microsoft.AspNetCore.Mvc;

using Enterprise.Integration.Adapters;
using Enterprise.Integration.Controllers;
using Enterprise.Integration.Events;
using Enterprise.Integration.Mapping;

using FluentAssertions;

using Xunit;

namespace Enterprise.Integration.Tests.Adapters;

public sealed class IntegrationLayerTests
{
    [Fact]
    public void Adapt_MapsKnownFieldsAndDropsUnmapped()
    {
        var adapter = new EnterpriseIntegrationAdapter(
            EnterpriseIntegrationSystem.HR,
            [new EnterpriseIntegrationMapping("emp_id", "EmployeeId"), new EnterpriseIntegrationMapping("emp_name", "EmployeeName")]);
        var rawPayload = new Dictionary<string, string> { ["emp_id"] = "123", ["emp_name"] = "Jane Doe", ["internal_hr_note"] = "unmapped" };

        EnterpriseIntegrationEvent result = adapter.Adapt("EmployeeHired", rawPayload, DateTime.UtcNow);

        result.SourceSystem.Should().Be(EnterpriseIntegrationSystem.HR);
        result.MappedPayload.Should().BeEquivalentTo(new Dictionary<string, string> { ["EmployeeId"] = "123", ["EmployeeName"] = "Jane Doe" });
    }

    [Fact]
    public void Adapt_MissingMappedField_OmittedFromResult()
    {
        var adapter = new EnterpriseIntegrationAdapter(
            EnterpriseIntegrationSystem.CMMS,
            [new EnterpriseIntegrationMapping("asset_id", "MachineId"), new EnterpriseIntegrationMapping("status", "MaintenanceStatus")]);
        var rawPayload = new Dictionary<string, string> { ["asset_id"] = "M-1" };

        EnterpriseIntegrationEvent result = adapter.Adapt("MaintenanceScheduled", rawPayload, DateTime.UtcNow);

        result.MappedPayload.Should().ContainKey("MachineId");
        result.MappedPayload.Should().NotContainKey("MaintenanceStatus");
    }

    [Fact]
    public void Adapt_BlankEventType_Throws()
    {
        var adapter = new EnterpriseIntegrationAdapter(EnterpriseIntegrationSystem.ERP, []);

        Action act = () => adapter.Adapt(" ", new Dictionary<string, string>(), DateTime.UtcNow);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void IngestEvent_KnownSystem_ReturnsOkWithAdaptedEvent()
    {
        var adapter = new EnterpriseIntegrationAdapter(EnterpriseIntegrationSystem.MES, [new EnterpriseIntegrationMapping("batch", "BatchId")]);
        var adapters = new Dictionary<EnterpriseIntegrationSystem, IEnterpriseIntegrationAdapter> { [EnterpriseIntegrationSystem.MES] = adapter };
        var controller = new EnterpriseIntegrationController(adapters);
        var request = new EnterpriseIntegrationIngestRequest("BatchCompleted", new Dictionary<string, string> { ["batch"] = "B-42" });

        ActionResult<EnterpriseIntegrationEvent> result = controller.IngestEvent(EnterpriseIntegrationSystem.MES, request);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var evt = okResult.Value.Should().BeOfType<EnterpriseIntegrationEvent>().Subject;
        evt.MappedPayload["BatchId"].Should().Be("B-42");
    }

    [Fact]
    public void IngestEvent_UnknownSystem_ReturnsNotFound()
    {
        var adapters = new Dictionary<EnterpriseIntegrationSystem, IEnterpriseIntegrationAdapter>();
        var controller = new EnterpriseIntegrationController(adapters);
        var request = new EnterpriseIntegrationIngestRequest("SomeEvent", new Dictionary<string, string>());

        ActionResult<EnterpriseIntegrationEvent> result = controller.IngestEvent(EnterpriseIntegrationSystem.EHS, request);

        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }
}
