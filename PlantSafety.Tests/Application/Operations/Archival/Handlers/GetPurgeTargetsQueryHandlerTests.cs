// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Application.Operations.Archival.Handlers
//  Layer:          Tests (Unit)
//  File:           GetPurgeTargetsQueryHandlerTests.cs
//  Created:        2026-07-20
//  SchemaVersion:  2026.1
//  Description:    Unit tests for GetPurgeTargetsQueryHandler.
// ============================================================================
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using NSubstitute;
using NSubstitute.ExceptionExtensions;

using PlantSafety.Application.Operations.Archival.Handlers;
using PlantSafety.Application.Operations.Archival.Models;
using PlantSafety.Application.Operations.Archival.Queries;
using PlantSafety.Application.Operations.Archival.Repositories;
using PlantSafety.Tests.Common.TestBase;

using Xunit;

namespace PlantSafety.Tests.Application.Operations.Archival.Handlers;

public sealed class GetPurgeTargetsQueryHandlerTests : UnitTestBase
{
    private readonly IArchivalRepository _repository = Substitute.For<IArchivalRepository>();

    private GetPurgeTargetsQueryHandler Sut() => new(_repository, NullLogger<GetPurgeTargetsQueryHandler>());

    [Fact]
    public async Task Handle_ShouldReturnFiveTargetsFromRepository()
    {
        IReadOnlyList<PurgeTargetDto> expected =
        [
            new PurgeTargetDto(PurgeTarget.AuditLog, "AuditDB.dbo.AuditLog", "PlantSafety.AuditDB", "dbo.sp_AuditLog_Purge", "Purges dbo.AuditLog rows."),
            new PurgeTargetDto(PurgeTarget.ErrorLog, "ErrorDB.dbo.ErrorLog", "PlantSafety.ErrorDB", "dbo.sp_ErrorLog_Purge", "Blocked by TR_ErrorLog_NoDelete."),
            new PurgeTargetDto(PurgeTarget.IntegrationLogEntry, "IntegrationDB.Integration.IntegrationLogEntry", "PlantSafety.IntegrationDB", "Integration.sp_IntegrationLogEntry_Purge", "Purges IntegrationLogEntry rows."),
            new PurgeTargetDto(PurgeTarget.ReplayRecord, "IntegrationDB.Integration.ReplayRecord", "PlantSafety.IntegrationDB", "Integration.sp_ReplayRecord_Purge", "Purges ReplayRecord rows."),
            new PurgeTargetDto(PurgeTarget.DiagnosticEvent, "DiagnosticsDB.Diagnostics.DiagnosticEvent", "PlantSafety.DiagnosticsDB", "Diagnostics.sp_DiagnosticEvent_Purge", "Purges DiagnosticEvent rows."),
        ];
        _repository.GetPurgeTargets().Returns(expected);

        var result = await Sut().Handle(new GetPurgeTargetsQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(5, result.Value.Count);
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ShouldReturnFailureResult()
    {
        _repository.GetPurgeTargets().Throws(new InvalidOperationException("unexpected"));

        var result = await Sut().Handle(new GetPurgeTargetsQuery(), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Operations.Archival.TargetsRetrievalFailed", result.Error.ErrorCode);
    }
}
