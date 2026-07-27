// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Application.Operations.AuditViewer.Handlers
//  Layer:          Tests (Unit)
//  File:           SearchAuditLogQueryHandlerTests.cs
//  Created:        2026-07-20
//  SchemaVersion:  2026.1
//  Description:    Unit tests for SearchAuditLogQueryHandler.
// ============================================================================
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using NSubstitute;
using NSubstitute.ExceptionExtensions;

using PlantSafety.Application.Operations.AuditViewer.Handlers;
using PlantSafety.Application.Operations.AuditViewer.Queries;
using PlantSafety.Tests.Common.TestBase;
using Shared.AuditLogging.Abstractions.Models;
using Shared.AuditLogging.Abstractions.Repositories;

using Xunit;

namespace PlantSafety.Tests.Application.Operations.AuditViewer.Handlers;

public sealed class SearchAuditLogQueryHandlerTests : UnitTestBase
{
    private readonly IAuditLogRepository _repository = Substitute.For<IAuditLogRepository>();

    private SearchAuditLogQueryHandler Sut() => new(_repository, NullLogger<SearchAuditLogQueryHandler>());

    private static AuditLogEntry SampleEntry() => new(
        "AiGovernance",
        "PlantSafety.CoreDB",
        "INSERT",
        "svc_plantsafety",
        FixedUtcNow,
        false,
        "AiGovernanceId=11111111-1111-1111-1111-111111111111; CreatedBy=22222222-2222-2222-2222-222222222222",
        0,
        "sp_AiGovernance_Create",
        null,
        null);

    [Fact]
    public async Task Handle_ShouldReturnResultFromRepository()
    {
        var expected = new AuditLogSearchResult([SampleEntry()], 1, 50, 1);
        _repository.SearchAsync(Arg.Any<AuditLogSearchCriteria>(), Arg.Any<CancellationToken>()).Returns(expected);

        var result = await Sut().Handle(new SearchAuditLogQuery(new AuditLogSearchCriteria()), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.Items);
        Assert.Equal(1, result.Value.TotalCount);
    }

    [Fact]
    public async Task Handle_ShouldPassCriteriaThrough()
    {
        var criteria = new AuditLogSearchCriteria(TableName: "AiGovernance", Operation: "INSERT");
        _repository.SearchAsync(criteria, Arg.Any<CancellationToken>())
            .Returns(new AuditLogSearchResult([], 1, 50, 0));

        await Sut().Handle(new SearchAuditLogQuery(criteria), CancellationToken.None);

        await _repository.Received(1).SearchAsync(criteria, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ShouldReturnFailureResult()
    {
        _repository.SearchAsync(Arg.Any<AuditLogSearchCriteria>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("PlantSafety.AuditDB unreachable"));

        var result = await Sut().Handle(new SearchAuditLogQuery(new AuditLogSearchCriteria()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Operations.AuditViewer.SearchFailed", result.Error.ErrorCode);
    }
}
