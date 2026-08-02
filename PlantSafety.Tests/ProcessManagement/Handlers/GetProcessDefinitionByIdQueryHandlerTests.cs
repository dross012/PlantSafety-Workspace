// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.ProcessManagement.Handlers
//  Layer:          Tests (Unit)
//  File:           GetProcessDefinitionByIdQueryHandlerTests.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Unit tests for GetProcessDefinitionByIdQueryHandler -- Phase 4
//                  ProcessManagement full build-out. Covers: successful retrieval with ETag
//                  projection, not-found behavior with correct severity, and tenant-isolation.
//                  Mirrors
//                  PlantSafety.Tests.Compliance.Auditing.Handlers.GetAuditingByIdQueryHandlerTests.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.ProcessManagement.Handlers;
using PlantSafety.Application.ProcessManagement.Models;
using PlantSafety.Application.ProcessManagement.Queries;
using PlantSafety.Application.ProcessManagement.Repositories;

namespace PlantSafety.Tests.ProcessManagement.Handlers;

public sealed class GetProcessDefinitionByIdQueryHandlerTests : UnitTestBase
{
    private readonly IProcessDefinitionRepository _repo = Substitute.For<IProcessDefinitionRepository>();

    private GetProcessDefinitionByIdQueryHandler Sut() => new(_repo, NullLogger<GetProcessDefinitionByIdQueryHandler>());

    private static ProcessDefinitionItemDto ProcessDefinitionFor(Guid processDefinitionId, Guid tenantId, string? eTag = "0x0000000000009ABC") => new()
    {
        ProcessDefinitionId = processDefinitionId,
        TenantId = tenantId,
        ProcessName = "Confined Space Entry - Tank Farm B",
        Category = "ConfinedSpace",
        Status = "Draft",
        Version = 1,
        OwnerId = TestUserId.Value,
        CreatedAtUtc = FixedUtcNow,
        CreatedBy = TestUserId.Value,
        ETag = eTag,
    };

    [Fact]
    public async Task Handle_WhenProcessDefinitionExistsForTenant_ReturnsItemWithEtag()
    {
        var processDefinitionId = Guid.NewGuid();
        var processDefinition = ProcessDefinitionFor(processDefinitionId, TestTenantId.Value);
        _repo.GetByIdAsync(processDefinitionId, TestTenantId.Value, Arg.Any<CancellationToken>()).Returns(processDefinition);

        var result = await Sut().Handle(new GetProcessDefinitionByIdQuery(TestTenantId.Value, processDefinitionId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.ProcessDefinitionId.Should().Be(processDefinitionId);
        result.Value.ETag.Should().Be("0x0000000000009ABC");
    }

    [Fact]
    public async Task Handle_WhenProcessDefinitionDoesNotExist_ReturnsNotFoundWithCorrectSeverity()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((ProcessDefinitionItemDto?)null);

        var result = await Sut().Handle(new GetProcessDefinitionByIdQuery(TestTenantId.Value, Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("ProcessManagement.ProcessDefinition.NotFound");
        result.Error.Severity.Should().Be(ErrorSeverity.NotFound, "a missing ProcessDefinition record must map to HTTP 404, not the default 500");
    }

    [Fact]
    public async Task Handle_WhenProcessDefinitionBelongsToDifferentTenant_ReturnsNotFound()
    {
        // Mirrors production behavior: usp_ProcessDefinitionAggregate_GetById filters by TenantId
        // in SQL, so a lookup scoped to TestTenantId for a record that actually belongs to
        // another tenant returns no row at all -- the repository is not stubbed for that
        // (ProcessDefinitionId, TestTenantId) combination and NSubstitute's default (null)
        // applies, exactly as the real proc would.
        var processDefinitionId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        _repo.GetByIdAsync(processDefinitionId, otherTenantId, Arg.Any<CancellationToken>()).Returns(ProcessDefinitionFor(processDefinitionId, otherTenantId));

        var result = await Sut().Handle(new GetProcessDefinitionByIdQuery(TestTenantId.Value, processDefinitionId), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("ProcessManagement.ProcessDefinition.NotFound");
    }
}
