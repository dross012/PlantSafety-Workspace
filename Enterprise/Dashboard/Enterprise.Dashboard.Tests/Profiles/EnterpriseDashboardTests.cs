// ============================================================================
//  Project:        Enterprise.Dashboard.Tests
//  File:           EnterpriseDashboardTests.cs
//  Created:        2026-08-08
//  Description:    Phase D.4 unit tests for EnterpriseDashboardProfile.Build
//                  and EnterpriseDashboardController: correct aggregation
//                  of enterprise data lake record counts, caller-supplied
//                  distribution pass-through, and the controller's GET action.
// ============================================================================
using Microsoft.AspNetCore.Mvc;

using Enterprise.Dashboard.Controllers;
using Enterprise.Dashboard.Dtos;
using Enterprise.Dashboard.Profiles;
using Enterprise.DataLake.Infrastructure.Query;
using Enterprise.DataLake.Infrastructure.Storage;
using Enterprise.DataLake.Infrastructure.Writing;

using FluentAssertions;

using Xunit;

namespace Enterprise.Dashboard.Tests.Profiles;

public sealed class EnterpriseDashboardTests
{
    [Fact]
    public void Build_AggregatesCountsFromDataLake()
    {
        var store = new DataLakeStore();
        var writer = new DataLakeWriter(store);
        var now = DateTime.UtcNow;
        writer.WriteFacilityRecord(Guid.NewGuid(), "p", now);
        writer.WriteEmployeeRecord(Guid.NewGuid(), "p", now);
        writer.WriteEmployeeRecord(Guid.NewGuid(), "p", now);
        writer.WriteMachineRecord(Guid.NewGuid(), "p", now);
        writer.WriteHazardRecord(Guid.NewGuid(), "p", now);
        writer.WriteEventRecord(Guid.NewGuid(), "p", now);
        writer.WriteWorkflowRecord(Guid.NewGuid(), "p", now);
        writer.WritePredictionRecord(Guid.NewGuid(), "p", now);
        var queryEngine = new DataLakeQueryEngine(store);

        EnterpriseDashboardDto dashboard = EnterpriseDashboardProfile.Build(queryEngine, nowUtc: now);

        dashboard.RiskSummary.TotalFacilities.Should().Be(1);
        dashboard.RiskSummary.TotalHazardRecords.Should().Be(1);
        dashboard.ComplianceSummary.TotalEventRecords.Should().Be(1);
        dashboard.TotalEmployees.Should().Be(2);
        dashboard.TotalMachines.Should().Be(1);
        dashboard.PredictiveAlerts.TotalPredictionRecords.Should().Be(1);
        dashboard.WorkflowStatus.TotalWorkflowRecords.Should().Be(1);
        dashboard.WorkflowStatus.MostRecentWorkflowRecordAtUtc.Should().Be(now);
        dashboard.GeneratedAtUtc.Should().Be(now);
    }

    [Fact]
    public void Build_EmptyDataLake_ReturnsZeroedDashboard()
    {
        var queryEngine = new DataLakeQueryEngine(new DataLakeStore());

        EnterpriseDashboardDto dashboard = EnterpriseDashboardProfile.Build(queryEngine);

        dashboard.RiskSummary.TotalFacilities.Should().Be(0);
        dashboard.WorkflowStatus.TotalWorkflowRecords.Should().Be(0);
        dashboard.WorkflowStatus.MostRecentWorkflowRecordAtUtc.Should().BeNull();
    }

    [Fact]
    public void Build_PassesThroughCallerSuppliedDistributions()
    {
        var queryEngine = new DataLakeQueryEngine(new DataLakeStore());
        var hazardCounts = new Dictionary<string, int> { ["Fire"] = 3, ["ChemicalSpill"] = 1 };
        var severityCounts = new Dictionary<string, int> { ["Severe"] = 2, ["Minor"] = 5 };

        EnterpriseDashboardDto dashboard = EnterpriseDashboardProfile.Build(
            queryEngine,
            hazardCountsByLabel: hazardCounts,
            severityCountsByLabel: severityCounts,
            predictiveAlerts: ["High injury trend detected"]);

        dashboard.HazardDistribution.CountsByHazardLabel.Should().BeEquivalentTo(hazardCounts);
        dashboard.SeverityDistribution.CountsBySeverityLabel.Should().BeEquivalentTo(severityCounts);
        dashboard.SeverityDistribution.TotalClassifiedRecords.Should().Be(7);
        dashboard.PredictiveAlerts.Alerts.Should().ContainSingle().Which.Should().Be("High injury trend detected");
    }

    [Fact]
    public void GetDashboard_ReturnsOkWithDashboardDto()
    {
        var controller = new EnterpriseDashboardController(new DataLakeQueryEngine(new DataLakeStore()));

        ActionResult<EnterpriseDashboardDto> result = controller.GetDashboard();

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeOfType<EnterpriseDashboardDto>();
    }
}
