// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Operational.WorkforceSafety.Handlers
//  Layer:          Tests (Unit)
//  File:           GetWorkforceSafetyQueryHandlerTests.cs
//  Created:        2026-08-20
//  SchemaVersion:  2026.1
//  Description:    Unit tests for GetWorkforceSafetyQueryHandler -- covers successful retrieval
//                  (tenant-wide and site-scoped), repository failure mapping to a sanitized
//                  ApplicationError, and the empty-result-set case. Mirrors the
//                  PlantSafety.Tests.AccidentManagement.Handlers.GetAccidentByIdQueryHandlerTests
//                  shape, adapted for a list-returning read-model query.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Operational.WorkforceSafety.Handlers;
using PlantSafety.Application.Operational.WorkforceSafety.Models;
using PlantSafety.Application.Operational.WorkforceSafety.Queries;
using PlantSafety.Application.Operational.WorkforceSafety.Repositories;

namespace PlantSafety.Tests.Operational.WorkforceSafety.Handlers;

public sealed class GetWorkforceSafetyQueryHandlerTests : UnitTestBase
{
    private readonly IWorkforceSafetyRepository _repo = Substitute.For<IWorkforceSafetyRepository>();

    private GetWorkforceSafetyQueryHandler Sut() => new(_repo, NullLogger<GetWorkforceSafetyQueryHandler>());

    private static WorkforceSafetyItemDto ItemFor(Guid recordId, Guid tenantId, Guid? siteId = null) => new()
    {
        WorkforceSafetyRecordId = recordId,
        TenantId = tenantId,
        SiteId = siteId,
        MetricId = Guid.NewGuid(),
        MetricName = "Near-Miss Rate",
        MetricValue = 1.8m,
        Status = "WithinTarget",
        RecordedAtUtc = FixedUtcNow,
        RecordedBy = TestUserId.Value,
    };

    [Fact]
    public async Task Handle_WhenRepositoryReturnsItems_ReturnsSuccessWithTenantAndItems()
    {
        var recordId = Guid.NewGuid();
        var item = ItemFor(recordId, TestTenantId.Value);
        var from = FixedUtcNow.AddDays(-7);
        var to = FixedUtcNow;

        _repo.GetWorkforceSafetyAsync(TestTenantId.Value, null, from, to, Arg.Any<CancellationToken>())
            .Returns(new List<WorkforceSafetyItemDto> { item });

        var result = await Sut().Handle(new GetWorkforceSafetyQuery(TestTenantId.Value, null, from, to), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.TenantId.Should().Be(TestTenantId.Value);
        result.Value.SiteId.Should().BeNull();
        result.Value.Items.Should().ContainSingle().Which.WorkforceSafetyRecordId.Should().Be(recordId);
    }

    [Fact]
    public async Task Handle_WhenSiteScoped_PassesSiteIdThroughToRepositoryAndResult()
    {
        var siteId = Guid.NewGuid();
        var recordId = Guid.NewGuid();
        var item = ItemFor(recordId, TestTenantId.Value, siteId);
        var from = FixedUtcNow.AddDays(-1);
        var to = FixedUtcNow;

        _repo.GetWorkforceSafetyAsync(TestTenantId.Value, siteId, from, to, Arg.Any<CancellationToken>())
            .Returns(new List<WorkforceSafetyItemDto> { item });

        var result = await Sut().Handle(new GetWorkforceSafetyQuery(TestTenantId.Value, siteId, from, to), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.SiteId.Should().Be(siteId);
        result.Value.Items.Should().ContainSingle().Which.SiteId.Should().Be(siteId);
    }

    [Fact]
    public async Task Handle_WhenRepositoryReturnsNoRows_ReturnsSuccessWithEmptyItems()
    {
        var from = FixedUtcNow.AddDays(-1);
        var to = FixedUtcNow;

        _repo.GetWorkforceSafetyAsync(TestTenantId.Value, null, from, to, Arg.Any<CancellationToken>())
            .Returns(new List<WorkforceSafetyItemDto>());

        var result = await Sut().Handle(new GetWorkforceSafetyQuery(TestTenantId.Value, null, from, to), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ReturnsSanitizedFailureResult()
    {
        var from = FixedUtcNow.AddDays(-1);
        var to = FixedUtcNow;

        _repo.GetWorkforceSafetyAsync(TestTenantId.Value, null, from, to, Arg.Any<CancellationToken>())
            .Returns<Task<IReadOnlyList<WorkforceSafetyItemDto>>>(_ => throw new InvalidOperationException("simulated SQL timeout"));

        var result = await Sut().Handle(new GetWorkforceSafetyQuery(TestTenantId.Value, null, from, to), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("Operational.WorkforceSafety.RetrievalFailed");
        result.Error.Message.Should().NotContain("simulated SQL timeout", "the error message must never leak raw exception detail");
    }
}