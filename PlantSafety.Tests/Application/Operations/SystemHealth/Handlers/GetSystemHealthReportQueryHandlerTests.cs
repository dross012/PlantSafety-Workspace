// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Application.Operations.SystemHealth.Handlers
//  Layer:          Tests (Unit)
//  File:           GetSystemHealthReportQueryHandlerTests.cs
//  Created:        2026-07-20
//  Last Updated:   2026-08-03
//  SchemaVersion:  2026.2
//  Description:    Unit tests for GetSystemHealthReportQueryHandler. Covers the happy
//                  path (every section available) and the resilience contract: a single
//                  failing sub-query must degrade only its own section, not fail the
//                  whole report.
//  2026-08-03: rewritten to mock the handler's real dependencies -- eight sibling-submodule
//              IReportSectionProvider<,> contracts plus ISystemHealthRepository -- after the
//              handler was refactored away from direct ISender.Send(<SiblingModuleQuery>)
//              cross-module dispatch (Handbook Part IX §9.3.2). Assertions/behavior unchanged.
// ============================================================================
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using NSubstitute;
using NSubstitute.ExceptionExtensions;

using PlantSafety.Application.Operations.Archival.Contracts;
using PlantSafety.Application.Operations.Archival.Models;
using PlantSafety.Application.Operations.AuditViewer.Contracts;
using PlantSafety.Application.Operations.Backup.Models;
using PlantSafety.Application.Operations.Configuration.Models;
using PlantSafety.Application.Operations.Monitoring.Contracts;
using PlantSafety.Application.Operations.Monitoring.Models;
using PlantSafety.Application.Operations.SchemaGuard.Models;
using PlantSafety.Application.Operations.SystemHealth.Handlers;
using PlantSafety.Application.Operations.SystemHealth.Models;
using PlantSafety.Application.Operations.SystemHealth.Queries;
using PlantSafety.Application.Operations.SystemHealth.Repositories;
using PlantSafety.SharedKernel.Contracts.ReadModelComposition;
using PlantSafety.SharedKernel.Primitives.Errors;
using PlantSafety.SharedKernel.Primitives.Results;
using PlantSafety.Tests.Common.TestBase;

using Xunit;

namespace PlantSafety.Tests.Application.Operations.SystemHealth.Handlers;

public sealed class GetSystemHealthReportQueryHandlerTests : UnitTestBase
{
    private readonly IReportSectionProvider<NoSectionRequest, IReadOnlyList<BackupHealthDto>> _backupHealthProvider =
        Substitute.For<IReportSectionProvider<NoSectionRequest, IReadOnlyList<BackupHealthDto>>>();

    private readonly IReportSectionProvider<NoSectionRequest, IReadOnlyList<SchemaDatabaseHealthDto>> _schemaHealthProvider =
        Substitute.For<IReportSectionProvider<NoSectionRequest, IReadOnlyList<SchemaDatabaseHealthDto>>>();

    private readonly IReportSectionProvider<NoSectionRequest, DatabaseHealthReportDto> _databaseHealthProvider =
        Substitute.For<IReportSectionProvider<NoSectionRequest, DatabaseHealthReportDto>>();

    private readonly IReportSectionProvider<ErrorLogSectionRequest, IReadOnlyList<ErrorLogItemDto>> _errorLogProvider =
        Substitute.For<IReportSectionProvider<ErrorLogSectionRequest, IReadOnlyList<ErrorLogItemDto>>>();

    private readonly IReportSectionProvider<QueryPerformanceSectionRequest, IReadOnlyList<QueryPerformanceItemDto>> _queryPerformanceProvider =
        Substitute.For<IReportSectionProvider<QueryPerformanceSectionRequest, IReadOnlyList<QueryPerformanceItemDto>>>();

    private readonly IReportSectionProvider<PurgeStatusSectionRequest, IReadOnlyList<ArchivalJobDto>> _purgeStatusProvider =
        Substitute.For<IReportSectionProvider<PurgeStatusSectionRequest, IReadOnlyList<ArchivalJobDto>>>();

    private readonly IReportSectionProvider<RecentAuditActivitySectionRequest, int> _recentAuditActivityProvider =
        Substitute.For<IReportSectionProvider<RecentAuditActivitySectionRequest, int>>();

    private readonly IReportSectionProvider<NoSectionRequest, IReadOnlyList<FeatureFlagDto>> _featureFlagsProvider =
        Substitute.For<IReportSectionProvider<NoSectionRequest, IReadOnlyList<FeatureFlagDto>>>();

    private readonly ISystemHealthRepository _repository = Substitute.For<ISystemHealthRepository>();

    private GetSystemHealthReportQueryHandler Sut() => new(
        _backupHealthProvider,
        _schemaHealthProvider,
        _databaseHealthProvider,
        _errorLogProvider,
        _queryPerformanceProvider,
        _purgeStatusProvider,
        _recentAuditActivityProvider,
        _featureFlagsProvider,
        _repository,
        NullLogger<GetSystemHealthReportQueryHandler>());

    private void StubAllSectionsHealthy()
    {
        _backupHealthProvider.GetSectionAsync(Arg.Any<NoSectionRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result<IReadOnlyList<BackupHealthDto>>.Success(
                [new BackupHealthDto("PlantSafety.CoreDB", FixedUtcNow, "Green", "Last FULL backup 2.0h ago.")]));

        _schemaHealthProvider.GetSectionAsync(Arg.Any<NoSectionRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result<IReadOnlyList<SchemaDatabaseHealthDto>>.Success(
                [new SchemaDatabaseHealthDto("CoreDB", "Current", [], 42)]));

        _databaseHealthProvider.GetSectionAsync(Arg.Any<NoSectionRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result<DatabaseHealthReportDto>.Success(
                new DatabaseHealthReportDto("Healthy", FixedUtcNow, 12.5, [])));

        _errorLogProvider.GetSectionAsync(Arg.Any<ErrorLogSectionRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result<IReadOnlyList<ErrorLogItemDto>>.Success([]));

        _queryPerformanceProvider.GetSectionAsync(Arg.Any<QueryPerformanceSectionRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result<IReadOnlyList<QueryPerformanceItemDto>>.Success([]));

        _purgeStatusProvider.GetSectionAsync(Arg.Any<PurgeStatusSectionRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result<IReadOnlyList<ArchivalJobDto>>.Success([]));

        _recentAuditActivityProvider.GetSectionAsync(Arg.Any<RecentAuditActivitySectionRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result<int>.Success(7));

        _featureFlagsProvider.GetSectionAsync(Arg.Any<NoSectionRequest>(), Arg.Any<CancellationToken>())
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
        _backupHealthProvider.GetSectionAsync(Arg.Any<NoSectionRequest>(), Arg.Any<CancellationToken>())
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
        _backupHealthProvider.GetSectionAsync(Arg.Any<NoSectionRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result<IReadOnlyList<BackupHealthDto>>.Failure(ErrorFactory.Create(
                "Operations.Backup.HealthRetrievalFailed", ErrorType.DependencyFailure, "Unable to retrieve backup health.")));

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
        _errorLogProvider.GetSectionAsync(Arg.Any<ErrorLogSectionRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result<IReadOnlyList<ErrorLogItemDto>>.Failure(ErrorFactory.Create(
                "Operations.Monitoring.ErrorLogRetrievalFailed", ErrorType.DependencyFailure, "Unable to retrieve errors.")));

        var result = await Sut().Handle(new GetSystemHealthReportQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.False(result.Value.MonitoringSummary.IsAvailable);
        Assert.True(result.Value.PurgeStatus.IsAvailable);
    }
}
