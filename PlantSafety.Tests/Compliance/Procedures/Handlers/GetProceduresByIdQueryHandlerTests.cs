// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Compliance.Procedures.Handlers
//  Layer:          Tests (Unit)
//  File:           GetProceduresByIdQueryHandlerTests.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Unit tests for GetProceduresByIdQueryHandler -- Phase 4 Compliance.Procedures
//                  full build-out. Covers: successful retrieval with ETag projection, not-found
//                  behavior with correct severity, and tenant-isolation (a Procedures record
//                  belonging to a different tenant must surface as NotFound, never leaking
//                  cross-tenant existence). Mirrors
//                  PlantSafety.Tests.Compliance.Permits.Handlers.GetPermitsByIdQueryHandlerTests.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Compliance.Procedures.Handlers;
using PlantSafety.Application.Compliance.Procedures.Models;
using PlantSafety.Application.Compliance.Procedures.Queries;
using PlantSafety.Application.Compliance.Procedures.Repositories;

namespace PlantSafety.Tests.Compliance.Procedures.Handlers;

public sealed class GetProceduresByIdQueryHandlerTests : UnitTestBase
{
    private readonly IProceduresRepository _repo = Substitute.For<IProceduresRepository>();

    private GetProceduresByIdQueryHandler Sut() => new(_repo, NullLogger<GetProceduresByIdQueryHandler>());

    private static ProceduresItemDto ProceduresFor(Guid proceduresId, Guid tenantId, string? eTag = "0x0000000000009ABC") => new()
    {
        ProceduresId = proceduresId,
        TenantId = tenantId,
        Status = "Created",
        ProcedureTitle = "Confined Space Entry SOP",
        ProcedureNumber = "SOP-2026-0001",
        Category = "ConfinedSpace",
        RevisionNumber = 1,
        OwnerId = TestUserId.Value,
        CreatedAtUtc = FixedUtcNow,
        CreatedBy = TestUserId.Value,
        ETag = eTag,
    };

    [Fact]
    public async Task Handle_WhenProceduresExistsForTenant_ReturnsItemWithEtag()
    {
        var proceduresId = Guid.NewGuid();
        var procedures = ProceduresFor(proceduresId, TestTenantId.Value);
        _repo.GetByIdAsync(proceduresId, TestTenantId.Value, Arg.Any<CancellationToken>()).Returns(procedures);

        var result = await Sut().Handle(new GetProceduresByIdQuery(TestTenantId.Value, proceduresId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.ProceduresId.Should().Be(proceduresId);
        result.Value.ETag.Should().Be("0x0000000000009ABC");
    }

    [Fact]
    public async Task Handle_WhenProceduresDoesNotExist_ReturnsNotFoundWithCorrectSeverity()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((ProceduresItemDto?)null);

        var result = await Sut().Handle(new GetProceduresByIdQuery(TestTenantId.Value, Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("Compliance.Procedures.NotFound");
        result.Error.Severity.Should().Be(ErrorSeverity.NotFound, "a missing Procedures record must map to HTTP 404, not the default 500");
    }

    [Fact]
    public async Task Handle_WhenProceduresBelongsToDifferentTenant_ReturnsNotFound()
    {
        // Mirrors production behavior: usp_Procedures_GetById filters by TenantId in SQL, so a
        // lookup scoped to TestTenantId for a Procedures record that actually belongs to another
        // tenant returns no row at all -- the repository is not stubbed for that
        // (ProceduresId, TestTenantId) combination and NSubstitute's default (null) applies,
        // exactly as the real proc would.
        var proceduresId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        _repo.GetByIdAsync(proceduresId, otherTenantId, Arg.Any<CancellationToken>()).Returns(ProceduresFor(proceduresId, otherTenantId));

        var result = await Sut().Handle(new GetProceduresByIdQuery(TestTenantId.Value, proceduresId), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("Compliance.Procedures.NotFound");
    }
}
