// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Operational.Wellness.Handlers
//  Layer:          Tests (Unit)
//  File:           GetWellnessQueryHandlerTests.cs
//  Created:        2026-08-20
//  SchemaVersion:  2026.1
//  Description:    Unit tests for GetWellnessQueryHandler -- covers successful retrieval
//                  (tenant-wide and site-scoped), repository failure mapping to a sanitized
//                  ApplicationError, and the empty-result-set case. Mirrors the
//                  PlantSafety.Tests.AccidentManagement.Handlers.GetAccidentByIdQueryHandlerTests
//                  shape, adapted for a list-returning read-model query.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Operational.Wellness.Handlers;
using PlantSafety.Application.Operational.Wellness.Models;
using PlantSafety.Application.Operational.Wellness.Queries;
using PlantSafety.Application.Operational.Wellness.Repositories;

namespace PlantSafety.Tests.Operational.Wellness.Handlers;

public sealed class GetWellnessQueryHandlerTests : UnitTestBase
{
    private readonly IWellnessRepository _repo = Substitute.For<IWellnessRepository>();

    private GetWellnessQueryHandler Sut() => new(_repo, NullLogger<GetWellnessQueryHandler>());

    private static WellnessItemDto ItemFor(Guid recordId, Guid tenantId, Guid? siteId = null) => new()
    {
        WellnessCheckInId = recordId,
        TenantId = tenantId,
        SiteId = siteId,
        EmployeeId = TestUserId.Value,
        EmployeeName = "Jordan Rivera",
        CheckInType = "DailyCheckIn",
        Status = "Submitted",
        CheckedInAtUtc = FixedUtcNow,
        CheckedInBy = TestUserId.Value,
    };

    [Fact]
    public async Task Handle_WhenRepositoryReturnsItems_ReturnsSuccessWithTenantAndItems()
    {
        var recordId = Guid.NewGuid();
        var item = ItemFor(recordId, TestTenantId.Value);
        var from = FixedUtcNow.AddDays(-7);
        var to = FixedUtcNow;

        _repo.GetWellnessAsync(TestTenantId.Value, null, from, to, Arg.Any<CancellationToken>())
            .Returns(new List<WellnessItemDto> { item });

        var result = await Sut().Handle(new GetWellnessQuery(TestTenantId.Value, null, from, to), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.TenantId.Should().Be(TestTenantId.Value);
        result.Value.SiteId.Should().BeNull();
        result.Value.Items.Should().ContainSingle().Which.WellnessCheckInId.Should().Be(recordId);
    }

    [Fact]
    public async Task Handle_WhenSiteScoped_PassesSiteIdThroughToRepositoryAndResult()
    {
        var siteId = Guid.NewGuid();
        var recordId = Guid.NewGuid();
        var item = ItemFor(recordId, TestTenantId.Value, siteId);
        var from = FixedUtcNow.AddDays(-1);
        var to = FixedUtcNow;

        _repo.GetWellnessAsync(TestTenantId.Value, siteId, from, to, Arg.Any<CancellationToken>())
            .Returns(new List<WellnessItemDto> { item });

        var result = await Sut().Handle(new GetWellnessQuery(TestTenantId.Value, siteId, from, to), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.SiteId.Should().Be(siteId);
        result.Value.Items.Should().ContainSingle().Which.SiteId.Should().Be(siteId);
    }

    [Fact]
    public async Task Handle_WhenRepositoryReturnsNoRows_ReturnsSuccessWithEmptyItems()
    {
        var from = FixedUtcNow.AddDays(-1);
        var to = FixedUtcNow;

        _repo.GetWellnessAsync(TestTenantId.Value, null, from, to, Arg.Any<CancellationToken>())
            .Returns(new List<WellnessItemDto>());

        var result = await Sut().Handle(new GetWellnessQuery(TestTenantId.Value, null, from, to), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ReturnsSanitizedFailureResult()
    {
        var from = FixedUtcNow.AddDays(-1);
        var to = FixedUtcNow;

        _repo.GetWellnessAsync(TestTenantId.Value, null, from, to, Arg.Any<CancellationToken>())
            .Returns<Task<IReadOnlyList<WellnessItemDto>>>(_ => throw new InvalidOperationException("simulated SQL timeout"));

        var result = await Sut().Handle(new GetWellnessQuery(TestTenantId.Value, null, from, to), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("Operational.Wellness.RetrievalFailed");
        result.Error.Message.Should().NotContain("simulated SQL timeout", "the error message must never leak raw exception detail");
    }
}