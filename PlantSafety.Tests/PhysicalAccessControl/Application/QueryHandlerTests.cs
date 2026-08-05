// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Application.PhysicalAccessControl
//  Layer:          Tests (Unit)
//  File:           QueryHandlerTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    Unit tests for all 5 PhysicalAccessControl read-model query handlers
//                  (GetAccessPointById, GetCredentialStatus, ListAccessEventsByZone,
//                  GetZoneOccupancyFromAccessEvents, ListActiveAnomalies) against mocked
//                  read repositories: success/found, not-found, and repository-throws paths.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.PhysicalAccessControl.AccessAnomalies.Handlers;
using PlantSafety.Application.PhysicalAccessControl.AccessAnomalies.Models;
using PlantSafety.Application.PhysicalAccessControl.AccessAnomalies.Queries;
using PlantSafety.Application.PhysicalAccessControl.AccessAnomalies.Repositories;
using PlantSafety.Application.PhysicalAccessControl.AccessEvents.Handlers;
using PlantSafety.Application.PhysicalAccessControl.AccessEvents.Models;
using PlantSafety.Application.PhysicalAccessControl.AccessEvents.Queries;
using PlantSafety.Application.PhysicalAccessControl.AccessEvents.Repositories;
using PlantSafety.Application.PhysicalAccessControl.AccessPoints.Handlers;
using PlantSafety.Application.PhysicalAccessControl.AccessPoints.Models;
using PlantSafety.Application.PhysicalAccessControl.AccessPoints.Queries;
using PlantSafety.Application.PhysicalAccessControl.AccessPoints.Repositories;
using PlantSafety.Application.PhysicalAccessControl.Credentials.Handlers;
using PlantSafety.Application.PhysicalAccessControl.Credentials.Models;
using PlantSafety.Application.PhysicalAccessControl.Credentials.Queries;
using PlantSafety.Application.PhysicalAccessControl.Credentials.Repositories;
using PlantSafety.Application.PhysicalAccessControl.SafetyZones.Handlers;
using PlantSafety.Application.PhysicalAccessControl.SafetyZones.Models;
using PlantSafety.Application.PhysicalAccessControl.SafetyZones.Queries;
using PlantSafety.Application.PhysicalAccessControl.SafetyZones.Repositories;

namespace PlantSafety.Tests.PhysicalAccessControl.Application;

public sealed class QueryHandlerTests : UnitTestBase
{
    // ---- GetAccessPointByIdQueryHandler ---------------------------------------------

    [Fact]
    public async Task GetAccessPointById_WhenFound_ShouldReturnDto()
    {
        var repo = Substitute.For<IAccessPointReadRepository>();
        var dto = new AccessPointDto
        {
            AccessPointId = Guid.NewGuid(),
            TenantId = TestTenantId.Value,
            AccessPointCode = "AP-1",
            DeviceType = "BadgeReader",
            Status = "Active",
            CreatedAtUtc = FixedUtcNow,
            CreatedBy = TestUserId.Value,
        };
        repo.GetByIdAsync(TestTenantId.Value, dto.AccessPointId, Arg.Any<CancellationToken>()).Returns(dto);

        var result = await new GetAccessPointByIdQueryHandler(repo, NullLogger<GetAccessPointByIdQueryHandler>())
            .Handle(new GetAccessPointByIdQuery(TestTenantId.Value, dto.AccessPointId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.AccessPoint.Should().Be(dto);
    }

    [Fact]
    public async Task GetAccessPointById_WhenNotFound_ShouldReturnSuccessWithNullPayload()
    {
        var repo = Substitute.For<IAccessPointReadRepository>();
        repo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((AccessPointDto?)null);

        var result = await new GetAccessPointByIdQueryHandler(repo, NullLogger<GetAccessPointByIdQueryHandler>())
            .Handle(new GetAccessPointByIdQuery(TestTenantId.Value, Guid.NewGuid()), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.AccessPoint.Should().BeNull();
    }

    [Fact]
    public async Task GetAccessPointById_WhenRepositoryThrows_ShouldReturnFailureResult()
    {
        var repo = Substitute.For<IAccessPointReadRepository>();
        repo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns<Task<AccessPointDto?>>(_ => throw new InvalidOperationException("boom"));

        var result = await new GetAccessPointByIdQueryHandler(repo, NullLogger<GetAccessPointByIdQueryHandler>())
            .Handle(new GetAccessPointByIdQuery(TestTenantId.Value, Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("PhysicalAccessControl.AccessPoint.RetrievalFailed");
    }

    // ---- GetCredentialStatusQueryHandler --------------------------------------------

    [Fact]
    public async Task GetCredentialStatus_WhenFound_ShouldReturnMaskedDto()
    {
        var repo = Substitute.For<ICredentialReadRepository>();
        var dto = new CredentialStatusDto
        {
            CredentialId = Guid.NewGuid(),
            TenantId = TestTenantId.Value,
            BadgeNumberMasked = "****1234",
            HolderType = "Employee",
            AccessLevel = "General",
            Status = "Issued",
            IssuedAtUtc = FixedUtcNow,
        };
        repo.GetStatusAsync(TestTenantId.Value, dto.CredentialId, Arg.Any<CancellationToken>()).Returns(dto);

        var result = await new GetCredentialStatusQueryHandler(repo, NullLogger<GetCredentialStatusQueryHandler>())
            .Handle(new GetCredentialStatusQuery(TestTenantId.Value, dto.CredentialId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Credential!.BadgeNumberMasked.Should().StartWith("****");
    }

    [Fact]
    public async Task GetCredentialStatus_WhenRepositoryThrows_ShouldReturnFailureResult()
    {
        var repo = Substitute.For<ICredentialReadRepository>();
        repo.GetStatusAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns<Task<CredentialStatusDto?>>(_ => throw new InvalidOperationException("boom"));

        var result = await new GetCredentialStatusQueryHandler(repo, NullLogger<GetCredentialStatusQueryHandler>())
            .Handle(new GetCredentialStatusQuery(TestTenantId.Value, Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("PhysicalAccessControl.Credential.RetrievalFailed");
    }

    // ---- ListAccessEventsByZoneQueryHandler -----------------------------------------

    [Fact]
    public async Task ListAccessEventsByZone_WhenItemsExist_ShouldReturnThem()
    {
        var repo = Substitute.For<IAccessEventReadRepository>();
        var zoneId = Guid.NewGuid();
        var items = new List<AccessEventDto>
        {
            new()
            {
                AccessEventId = Guid.NewGuid(),
                TenantId = TestTenantId.Value,
                AccessPointId = Guid.NewGuid(),
                EventType = "Granted",
                Direction = "In",
                Status = "Ingested",
                OccurredAtUtc = FixedUtcNow,
            },
        };
        repo.ListByZoneAsync(TestTenantId.Value, zoneId, Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(items);

        var result = await new ListAccessEventsByZoneQueryHandler(repo, NullLogger<ListAccessEventsByZoneQueryHandler>())
            .Handle(new ListAccessEventsByZoneQuery(TestTenantId.Value, zoneId, FixedUtcNow.AddDays(-1), FixedUtcNow), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEquivalentTo(items);
        result.Value.SafetyZoneId.Should().Be(zoneId);
    }

    [Fact]
    public async Task ListAccessEventsByZone_WhenRepositoryThrows_ShouldReturnFailureResult()
    {
        var repo = Substitute.For<IAccessEventReadRepository>();
        repo.ListByZoneAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns<Task<IReadOnlyList<AccessEventDto>>>(_ => throw new InvalidOperationException("boom"));

        var result = await new ListAccessEventsByZoneQueryHandler(repo, NullLogger<ListAccessEventsByZoneQueryHandler>())
            .Handle(new ListAccessEventsByZoneQuery(TestTenantId.Value, Guid.NewGuid(), FixedUtcNow.AddDays(-1), FixedUtcNow), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("PhysicalAccessControl.AccessEvent.RetrievalFailed");
    }

    // ---- GetZoneOccupancyFromAccessEventsQueryHandler -------------------------------

    [Fact]
    public async Task GetZoneOccupancy_WhenFound_ShouldReturnDto()
    {
        var repo = Substitute.For<ISafetyZoneReadRepository>();
        var dto = new SafetyZoneOccupancyDto
        {
            SafetyZoneId = Guid.NewGuid(),
            TenantId = TestTenantId.Value,
            ZoneCode = "ZONE-A",
            ZoneName = "Zone A",
            MaxOccupancy = 25,
            CurrentOccupancy = 10,
            Status = "Active",
            AsOfUtc = FixedUtcNow,
        };
        repo.GetOccupancyAsync(TestTenantId.Value, dto.SafetyZoneId, Arg.Any<CancellationToken>()).Returns(dto);

        var result = await new GetZoneOccupancyFromAccessEventsQueryHandler(repo, NullLogger<GetZoneOccupancyFromAccessEventsQueryHandler>())
            .Handle(new GetZoneOccupancyFromAccessEventsQuery(TestTenantId.Value, dto.SafetyZoneId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Occupancy.Should().Be(dto);
    }

    [Fact]
    public async Task GetZoneOccupancy_WhenRepositoryThrows_ShouldReturnFailureResult()
    {
        var repo = Substitute.For<ISafetyZoneReadRepository>();
        repo.GetOccupancyAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns<Task<SafetyZoneOccupancyDto?>>(_ => throw new InvalidOperationException("boom"));

        var result = await new GetZoneOccupancyFromAccessEventsQueryHandler(repo, NullLogger<GetZoneOccupancyFromAccessEventsQueryHandler>())
            .Handle(new GetZoneOccupancyFromAccessEventsQuery(TestTenantId.Value, Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("PhysicalAccessControl.SafetyZone.RetrievalFailed");
    }

    // ---- ListActiveAnomaliesQueryHandler ---------------------------------------------

    [Fact]
    public async Task ListActiveAnomalies_WhenItemsExist_ShouldReturnThem()
    {
        var repo = Substitute.For<IAccessAnomalyReadRepository>();
        var items = new List<AccessAnomalyDto>
        {
            new()
            {
                AccessAnomalyId = Guid.NewGuid(),
                TenantId = TestTenantId.Value,
                AccessPointId = Guid.NewGuid(),
                AnomalyType = "Tailgating",
                SeverityScore = 70m,
                Status = "Detected",
                CreatedAtUtc = FixedUtcNow,
            },
        };
        repo.ListActiveAsync(TestTenantId.Value, null, Arg.Any<CancellationToken>()).Returns(items);

        var result = await new ListActiveAnomaliesQueryHandler(repo, NullLogger<ListActiveAnomaliesQueryHandler>())
            .Handle(new ListActiveAnomaliesQuery(TestTenantId.Value, null), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEquivalentTo(items);
    }

    [Fact]
    public async Task ListActiveAnomalies_WhenRepositoryThrows_ShouldReturnFailureResult()
    {
        var repo = Substitute.For<IAccessAnomalyReadRepository>();
        repo.ListActiveAsync(Arg.Any<Guid>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>())
            .Returns<Task<IReadOnlyList<AccessAnomalyDto>>>(_ => throw new InvalidOperationException("boom"));

        var result = await new ListActiveAnomaliesQueryHandler(repo, NullLogger<ListActiveAnomaliesQueryHandler>())
            .Handle(new ListActiveAnomaliesQuery(TestTenantId.Value, null), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("PhysicalAccessControl.AccessAnomaly.RetrievalFailed");
    }
}
