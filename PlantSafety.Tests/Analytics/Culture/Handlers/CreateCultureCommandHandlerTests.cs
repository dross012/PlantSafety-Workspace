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
using PlantSafety.SharedKernel.Abstractions;
using CultureAggregate = PlantSafety.Domain.Analytics.Culture.Aggregates.Culture;

namespace PlantSafety.Tests.Analytics.Culture.Handlers;

public sealed class CreateCultureCommandHandlerTests : UnitTestBase
{
    private readonly ICultureRepository   _repo    = Substitute.For<ICultureRepository>();
    private readonly IUnitOfWork          _uow     = Substitute.For<IUnitOfWork>();
    private readonly IOutboxWriter        _outbox  = Substitute.For<IOutboxWriter>();

    private CreateCultureCommandHandler Sut() => new(_repo, _uow, _outbox, NullLogger<CreateCultureCommandHandler>());

    [Fact]
    public async Task Handle_WithValidCommand_ShouldPersistAggregate()
    {
        var cmd = new CreateCultureCommand(TestTenantId, TestUserId, TestCorrelationId, TestCausationId, TestActor);
        await Sut().Handle(cmd, CancellationToken.None);
        await _repo.Received(1).AddAsync(Arg.Any<CultureAggregate>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCommitUnitOfWork()
    {
        var cmd = new CreateCultureCommand(TestTenantId, TestUserId, TestCorrelationId, TestCausationId, TestActor);
        await Sut().Handle(cmd, CancellationToken.None);
        await _uow.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }
}
