// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Intelligence.DataLake.Handlers
//  Layer:          Tests (Unit)
//  File:           GetDataLakeActivityByIdQueryHandlerTests.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Unit tests for GetDataLakeActivityByIdQueryHandler -- Phase 4
//                  Intelligence.DataLake full build-out. Covers: successful retrieval with ETag
//                  projection, not-found behavior with correct severity, and tenant-isolation (a
//                  DataLakeActivity record belonging to a different tenant must surface as
//                  NotFound, never leaking cross-tenant existence). Mirrors
//                  PlantSafety.Tests.ContinuousImprovement.RCA.Handlers.GetRCAByIdQueryHandlerTests.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Intelligence.DataLake.Handlers;
using PlantSafety.Application.Intelligence.DataLake.Models;
using PlantSafety.Application.Intelligence.DataLake.Queries;
using PlantSafety.Application.Intelligence.DataLake.Repositories;

namespace PlantSafety.Tests.Intelligence.DataLake.Handlers;

public sealed class GetDataLakeActivityByIdQueryHandlerTests : UnitTestBase
{
    private readonly IDataLakeActivityDetailRepository _repo = Substitute.For<IDataLakeActivityDetailRepository>();

    private GetDataLakeActivityByIdQueryHandler Sut() => new(_repo, NullLogger<GetDataLakeActivityByIdQueryHandler>());

    private static DataLakeActivityDetailDto ActivityFor(Guid dataLakeActivityId, Guid tenantId, string? eTag = "0x0000000000004DEF") => new()
    {
        DataLakeActivityId = dataLakeActivityId,
        TenantId = tenantId,
        DatasetName = "WorkerFatigueTelemetry",
        Zone = "Raw",
        RecordCount = 10_000,
        Status = "Ingested",
        ProcessedAtUtc = FixedUtcNow,
        ProcessedBy = TestUserId.Value,
        CreatedAtUtc = FixedUtcNow,
        CreatedBy = TestUserId.Value,
        ETag = eTag,
    };

    [Fact]
    public async Task Handle_WhenDataLakeActivityExistsForTenant_ReturnsItemWithEtag()
    {
        var dataLakeActivityId = Guid.NewGuid();
        var activity = ActivityFor(dataLakeActivityId, TestTenantId.Value);
        _repo.GetByIdAsync(dataLakeActivityId, TestTenantId.Value, Arg.Any<CancellationToken>()).Returns(activity);

        var result = await Sut().Handle(new GetDataLakeActivityByIdQuery(TestTenantId.Value, dataLakeActivityId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.DataLakeActivityId.Should().Be(dataLakeActivityId);
        result.Value.ETag.Should().Be("0x0000000000004DEF");
    }

    [Fact]
    public async Task Handle_WhenDataLakeActivityDoesNotExist_ReturnsNotFoundWithCorrectSeverity()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((DataLakeActivityDetailDto?)null);

        var result = await Sut().Handle(new GetDataLakeActivityByIdQuery(TestTenantId.Value, Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("Intelligence.DataLake.NotFound");
        result.Error.Severity.Should().Be(ErrorSeverity.NotFound, "a missing DataLakeActivity record must map to HTTP 404, not the default 500");
    }

    [Fact]
    public async Task Handle_WhenDataLakeActivityBelongsToDifferentTenant_ReturnsNotFound()
    {
        // Mirrors production behavior: usp_DataLakeActivity_GetById filters by TenantId in SQL, so
        // a lookup scoped to TestTenantId for a DataLakeActivity record that actually belongs to
        // another tenant returns no row at all -- the repository is not stubbed for that
        // (dataLakeActivityId, TestTenantId) combination and NSubstitute's default (null) applies,
        // exactly as the real proc would.
        var dataLakeActivityId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        _repo.GetByIdAsync(dataLakeActivityId, otherTenantId, Arg.Any<CancellationToken>()).Returns(ActivityFor(dataLakeActivityId, otherTenantId));

        var result = await Sut().Handle(new GetDataLakeActivityByIdQuery(TestTenantId.Value, dataLakeActivityId), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("Intelligence.DataLake.NotFound");
    }
}
