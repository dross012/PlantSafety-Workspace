// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Application.Operations.Archival.Handlers
//  Layer:          Tests (Unit)
//  File:           GetRetentionSettingsQueryHandlerTests.cs
//  Created:        2026-07-20
//  SchemaVersion:  2026.1
//  Description:    Unit tests for GetRetentionSettingsQueryHandler.
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

public sealed class GetRetentionSettingsQueryHandlerTests : UnitTestBase
{
    private readonly IArchivalRepository _repository = Substitute.For<IArchivalRepository>();

    private GetRetentionSettingsQueryHandler Sut() => new(_repository, NullLogger<GetRetentionSettingsQueryHandler>());

    [Fact]
    public async Task Handle_ShouldReturnItemsFromRepository()
    {
        IReadOnlyList<TierPolicyDto> expected =
        [
            new TierPolicyDto(Guid.NewGuid(), "AuditDB.dbo.AuditLog", "Hot", "Purged", 90, FixedUtcNow, Guid.Empty)
        ];
        _repository.GetRetentionSettingsAsync(Arg.Any<CancellationToken>()).Returns(expected);

        var result = await Sut().Handle(new GetRetentionSettingsQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value);
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ShouldReturnFailureResult()
    {
        _repository.GetRetentionSettingsAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("ArchiveDB unreachable"));

        var result = await Sut().Handle(new GetRetentionSettingsQuery(), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Operations.Archival.RetentionSettingsRetrievalFailed", result.Error.ErrorCode);
    }
}
