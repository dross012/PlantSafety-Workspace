// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Analytics.Culture.Handlers
//  Layer:          Tests (Unit)
//  File:           CreateCultureCommandHandlerTests.cs
//  Created:        2026-06-17
//  SchemaVersion:  2026.1
//  Description:    Unit tests for CreateCultureCommandHandler.
// ============================================================================
using NSubstitute;
using PlantSafety.Application.Analytics.Culture.Commands;
using PlantSafety.Application.Common.Interfaces.Persistence;
using CultureAggregate = PlantSafety.Domain.Analytics.Culture.Aggregates.Culture;

namespace PlantSafety.Tests.Analytics.Culture.Handlers;

public sealed class CreateCultureCommandHandlerTests : UnitTestBase
{
    private readonly ICultureRepository _repo = Substitute.For<ICultureRepository>();

    private CreateCultureCommandHandler Sut() => new(_repo, NullLogger<CreateCultureCommandHandler>());

    [Fact]
    public async Task Handle_WithValidCommand_ShouldPersistAggregate()
    {
        var cmd = new CreateCultureCommand(TestTenantId, TestUserId, TestCorrelationId, TestCausationId, TestActor);
        await Sut().Handle(cmd, CancellationToken.None);
        await _repo.Received(1).AddAsync(Arg.Any<CultureAggregate>(), Arg.Any<CancellationToken>());
    }
}
