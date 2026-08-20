// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Intelligence.ModelRegistry.Handlers
//  Layer:          Tests (Unit)
//  File:           GetModelRegistryEntryByIdQueryHandlerTests.cs
//  Created:        2026-08-20
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    Unit tests for GetModelRegistryEntryByIdQueryHandler. Covers successful
//                  retrieval with ETag projection, not-found behavior with correct severity, and
//                  tenant isolation (a record belonging to a different tenant must surface as
//                  NotFound, never leaking cross-tenant existence). Mirrors
//                  PlantSafety.Tests.Intelligence.DataLake.Handlers.GetDataLakeActivityByIdQueryHandlerTests.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Intelligence.ModelRegistry.Handlers;
using PlantSafety.Application.Intelligence.ModelRegistry.Models;
using PlantSafety.Application.Intelligence.ModelRegistry.Queries;
using PlantSafety.Application.Intelligence.ModelRegistry.Repositories;

namespace PlantSafety.Tests.Intelligence.ModelRegistry.Handlers;

public sealed class GetModelRegistryEntryByIdQueryHandlerTests : UnitTestBase
{
    private readonly IModelRegistryEntryDetailRepository _repo = Substitute.For<IModelRegistryEntryDetailRepository>();

    private GetModelRegistryEntryByIdQueryHandler Sut() => new(_repo, NullLogger<GetModelRegistryEntryByIdQueryHandler>());

    private static ModelRegistryEntryDetailDto EntryFor(Guid modelRegistryEntryId, Guid tenantId, string? eTag = "0x0000000000004DEF") => new()
    {
        ModelRegistryEntryId = modelRegistryEntryId,
        TenantId = tenantId,
        ModelName = "FailurePredictor",
        ModelVersion = "1.0.0",
        Status = "Approved",
        DatasetHash = "sha256:abc123",
        TrainingAccuracyScore = 0.9,
        MeetsMinimumConfidenceThreshold = true,
        RegisteredBy = TestUserId.Value,
        CreatedAtUtc = FixedUtcNow,
        CreatedBy = TestUserId.Value,
        ETag = eTag,
    };

    [Fact]
    public async Task Handle_WhenEntryExistsForTenant_ReturnsItemWithEtag()
    {
        var id = Guid.NewGuid();
        var entry = EntryFor(id, TestTenantId.Value);
        _repo.GetByIdAsync(id, TestTenantId.Value, Arg.Any<CancellationToken>()).Returns(entry);

        var result = await Sut().Handle(new GetModelRegistryEntryByIdQuery(TestTenantId.Value, id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.ModelRegistryEntryId.Should().Be(id);
        result.Value.ETag.Should().Be("0x0000000000004DEF");
    }

    [Fact]
    public async Task Handle_WhenEntryDoesNotExist_ReturnsNotFoundWithCorrectSeverity()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((ModelRegistryEntryDetailDto?)null);

        var result = await Sut().Handle(new GetModelRegistryEntryByIdQuery(TestTenantId.Value, Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("Intelligence.ModelRegistry.NotFound");
        result.Error.Severity.Should().Be(ErrorSeverity.NotFound, "a missing ModelRegistryEntry record must map to HTTP 404, not the default 500");
    }

    [Fact]
    public async Task Handle_WhenEntryBelongsToDifferentTenant_ReturnsNotFound()
    {
        var id = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        _repo.GetByIdAsync(id, otherTenantId, Arg.Any<CancellationToken>()).Returns(EntryFor(id, otherTenantId));

        var result = await Sut().Handle(new GetModelRegistryEntryByIdQuery(TestTenantId.Value, id), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("Intelligence.ModelRegistry.NotFound");
    }
}
