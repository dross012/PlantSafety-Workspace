// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Application.Operations.Archival.Handlers
//  Layer:          Tests (Unit)
//  File:           GetPurgeStatusQueryHandlerTests.cs
//  Created:        2026-07-20
//  SchemaVersion:  2026.1
//  Description:    Unit tests for GetPurgeStatusQueryHandler.
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

public sealed class GetPurgeStatusQueryHandlerTests : UnitTestBase
{
    private readonly IArchivalRepository _repository = Substitute.For<IArchivalRepository>();

    private GetPurgeStatusQueryHandler Sut() => new(_repository, NullLogger<GetPurgeStatusQueryHandler>());

    [Fact]
    public async Task Handle_ShouldReturnItemsFromRepository()
    {
        IReadOnlyList<ArchivalJobDto> expected =
        [
            new ArchivalJobDto(Guid.NewGuid(), Guid.NewGuid(), Guid.Empty, "AuditDB.dbo.AuditLog", "Hot", "Purged", "Completed", FixedUtcNow, FixedUtcNow, "SUCCESS", Guid.NewGuid(), FixedUtcNow, Guid.Empty)
        ];
        _repository.GetPurgeStatusAsync(null, 50, Arg.Any<CancellationToken>()).Returns(expected);

        var result = await Sut().Handle(new GetPurgeStatusQuery(null), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value);
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ShouldReturnFailureResult()
    {
        _repository.GetPurgeStatusAsync(Arg.Any<string?>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("ArchiveDB unreachable"));

        var result = await Sut().Handle(new GetPurgeStatusQuery(null), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Operations.Archival.StatusRetrievalFailed", result.Error.ErrorCode);
    }
}
