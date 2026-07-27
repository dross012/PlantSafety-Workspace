// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Application.Operations.SystemHealth.Handlers
//  Layer:          Tests (Unit)
//  File:           GetSystemHealthReportQueryHandlerTests.cs
//  Created:        2026-07-20
//  SchemaVersion:  2026.1
//  Description:    Unit tests for GetSystemHealthReportQueryHandler. Covers the happy
//                  path (every section available) and the resilience contract: a single
//                  failing sub-query must degrade only its own section, not fail the
//                  whole report.
// ============================================================================
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using MediatR;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Operations.Archival.Models;
using PlantSafety.Application.Operations.Archival.Queries;
using PlantSafety.Application.Operations.AuditViewer.Queries;
using Shared.AuditLogging.Abstractions.Models;
using PlantSafety.Application.Operations.Backup.Models;
using PlantSafety.Application.Operations.Backup.Queries;
using PlantSafety.Application.Operations.Configuration.Models;
using PlantSafety.Application.Operations.Configuration.Queries;
using PlantSafety.Application.Operations.Monitoring.Models;
using PlantSafety.Application.Operations.Monitoring.Queries;
using PlantSafety.Application.Operations.SchemaGuard.Models;
using PlantSafety.Application.Operations.SchemaGuard.Queries;
using PlantSafety.Application.Operations.SystemHealth.Handlers;
using PlantSafety.Application.Operations.SystemHealth.Models;
using PlantSafety.Application.Operations.SystemHealth.Queries;
using PlantSafety.Application.Operations.SystemHealth.Repositories;
using PlantSafety.Tests.Common.TestBase;

using Xunit;

namespace PlantSafety.Tests.Application.Operations.SystemHealth.Handlers;

public sealed class GetSystemHealthReportQueryHandlerTests : UnitTestBase
{
    private readonly ISender _sender = Substitute.For<ISender>();
    private readonly ISystemHealthRepository _repository = Substitute.For<ISystemHealthRepository>();

    private GetSystemHealthReportQueryHandler Sut() => new(_sender, _repository, NullLogger<GetSystemHealthReportQueryHandler>());

    private void StubAllSectionsHealthy()
    {
        _sender.Send(Arg.Any<GetBackupHealthQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<IReadOnlyList<BackupHealthDto>>.Success(
                [new BackupHealthDto("PlantSafety.CoreDB", FixedUtcNow, "Green", "Last FULL backup 2.0h ago.")]));

        _sender.Send(Arg.Any<GetSchemaHealthQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<IReadOnlyList<SchemaDatabaseHealthDto>>.Success(
                [new SchemaDatabaseHealthDto("CoreDB", "Current", [], 42)]));

        _sender.Send(Arg.Any<GetDatabaseHealthReportQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<DatabaseHealthReportDto>.Success(
                new DatabaseHealthReportDto("Healthy", FixedUtcNow, 12.5, [])));

        _sender.Send(Arg.Any<GetErrorLogQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<IReadOnlyList<ErrorLogItemDto>>.Success([]));

        _sender.Send(Arg.Any<GetQueryPerformanceQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<IReadOnlyList<QueryPerformanceItemDto>>.Success([]));

        _sender.Send(Arg.Any<GetPurgeStatusQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<IReadOnlyList<ArchivalJobDto>>.Success([]));

        _sender.Send(Arg.Any<SearchAuditLogQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<AuditLogSearchResult>.Success(new AuditLogSearchResult([], 1, 1, 7)));

        _sender.Send(Arg.Any<GetAllFeatureFlagsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<IReadOnlyList<FeatureFlagDto>>.Success([]));

        _repository.GetTaskSchedulerStatusAsync(Arg.Any<CancellationToken>())
            .Returns((IReadOnlyList<TaskSchedulerJobStatusDto>)[
                new TaskSchedulerJobStatusDto("PlantSafety - Full Backup (All Databases)", "Backup", "Ready", FixedUtcNow, "0")
            ]);

        _repository.GetDatabaseVersionAsync(Arg.Any<CancellationToken>())
            .Returns(new SystemHealthDatabaseVersionDto("16.0.1000.6", "RTM", "Express Edition", 4));
    }

    [Fact]
    public async Task Handle_WhenEverySectionSucceeds_ShouldReturnFullyAvailableReport()
    {
        StubAllSectionsHealthy();

        var result = await Sut().Handle(new GetSystemHealthReportQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var report = result.Value;

        Assert.True(report.BackupHealth.IsAvailable);
        Assert.True(report.SchemaHealth.IsAvailable);
        Assert.True(report.DatabaseHealth.IsAvailable);
        Assert.True(report.MonitoringSummary.IsAvailable);
        Assert.True(report.PurgeStatus.IsAvailable);
        Assert.True(report.RecentAuditActivityCount.IsAvailable);
        Assert.Equal(7, report.RecentAuditActivityCount.Data);
        Assert.True(report.FeatureFlags.IsAvailable);
        Assert.True(report.TaskSchedulerJobs.IsAvailable);
        Assert.True(report.DatabaseVersion.IsAvailable);
        Assert.Equal("16.0.1000.6", report.DatabaseVersion.Data!.ProductVersion);
        Assert.False(string.IsNullOrWhiteSpace(report.AppVersion));
    }

    [Fact]
    public async Task Handle_WhenBackupHealthQueryThrows_ShouldMarkOnlyBackupSectionUnavailable()
    {
        StubAllSectionsHealthy();
        _sender.Send(Arg.Any<GetBackupHealthQuery>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("msdb unreachable"));

        var result = await Sut().Handle(new GetSystemHealthReportQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var report = result.Value;

        Assert.False(report.BackupHealth.IsAvailable);
        Assert.Null(report.BackupHealth.Data);
        Assert.NotNull(report.BackupHealth.UnavailableReason);

        // Every other section still populated -- the whole dashboard did not blank out.
        Assert.True(report.SchemaHealth.IsAvailable);
        Assert.True(report.DatabaseHealth.IsAvailable);
        Assert.True(report.MonitoringSummary.IsAvailable);
        Assert.True(report.PurgeStatus.IsAvailable);
        Assert.True(report.RecentAuditActivityCount.IsAvailable);
        Assert.True(report.FeatureFlags.IsAvailable);
        Assert.True(report.TaskSchedulerJobs.IsAvailable);
        Assert.True(report.DatabaseVersion.IsAvailable);
    }

    [Fact]
    public async Task Handle_WhenBackupHealthQueryReturnsFailureResult_ShouldMarkOnlyBackupSectionUnavailable()
    {
        StubAllSectionsHealthy();
        _sender.Send(Arg.Any<GetBackupHealthQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<IReadOnlyList<BackupHealthDto>>.Failure(new ApplicationError(
                "Operations.Backup.HealthRetrievalFailed", "Unable to retrieve backup health.", Guid.NewGuid())));

        var result = await Sut().Handle(new GetSystemHealthReportQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.False(result.Value.BackupHealth.IsAvailable);
        Assert.Equal("Unable to retrieve backup health.", result.Value.BackupHealth.UnavailableReason);
        Assert.True(result.Value.SchemaHealth.IsAvailable);
    }

    [Fact]
    public async Task Handle_WhenTaskSchedulerRepositoryThrows_ShouldMarkOnlyTaskSchedulerSectionUnavailable()
    {
        StubAllSectionsHealthy();
        _repository.GetTaskSchedulerStatusAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("schtasks.exe not found"));

        var result = await Sut().Handle(new GetSystemHealthReportQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.False(result.Value.TaskSchedulerJobs.IsAvailable);
        Assert.True(result.Value.DatabaseVersion.IsAvailable);
        Assert.True(result.Value.BackupHealth.IsAvailable);
    }

    [Fact]
    public async Task Handle_WhenMonitoringErrorQueryFails_ShouldMarkMonitoringSummaryUnavailable()
    {
        StubAllSectionsHealthy();
        _sender.Send(Arg.Any<GetErrorLogQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<IReadOnlyList<ErrorLogItemDto>>.Failure(new ApplicationError(
                "Operations.Monitoring.ErrorLogRetrievalFailed", "Unable to retrieve errors.", Guid.NewGuid())));

        var result = await Sut().Handle(new GetSystemHealthReportQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.False(result.Value.MonitoringSummary.IsAvailable);
        Assert.True(result.Value.PurgeStatus.IsAvailable);
    }
}
