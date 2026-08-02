// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Compliance.RegulatoryCalendar.Handlers
//  Layer:          Tests (Unit)
//  File:           GetRegulatoryCalendarByIdQueryHandlerTests.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Unit tests for GetRegulatoryCalendarByIdQueryHandler -- Phase 4
//                  Compliance.RegulatoryCalendar full build-out. Covers: successful retrieval with
//                  ETag projection, not-found behavior with correct severity, and tenant-isolation
//                  (a RegulatoryCalendar record belonging to a different tenant must surface as
//                  NotFound, never leaking cross-tenant existence). Mirrors
//                  PlantSafety.Tests.Compliance.Permits.Handlers.GetPermitsByIdQueryHandlerTests.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Compliance.RegulatoryCalendar.Handlers;
using PlantSafety.Application.Compliance.RegulatoryCalendar.Models;
using PlantSafety.Application.Compliance.RegulatoryCalendar.Queries;
using PlantSafety.Application.Compliance.RegulatoryCalendar.Repositories;

namespace PlantSafety.Tests.Compliance.RegulatoryCalendar.Handlers;

public sealed class GetRegulatoryCalendarByIdQueryHandlerTests : UnitTestBase
{
    private readonly IRegulatoryCalendarRepository _repo = Substitute.For<IRegulatoryCalendarRepository>();

    private GetRegulatoryCalendarByIdQueryHandler Sut() => new(_repo, NullLogger<GetRegulatoryCalendarByIdQueryHandler>());

    private static RegulatoryCalendarItemDto RegulatoryCalendarFor(Guid regulatoryCalendarId, Guid tenantId, string? eTag = "0x0000000000009ABC") => new()
    {
        RegulatoryCalendarId = regulatoryCalendarId,
        TenantId = tenantId,
        Status = "Created",
        EventTitle = "Annual OSHA Filing",
        RegulatoryBody = "OSHA",
        JurisdictionCode = "US-FED",
        ComplianceCategory = "Reporting",
        ResponsiblePartyId = TestUserId.Value,
        ReminderLeadDays = 30,
        CreatedAtUtc = FixedUtcNow,
        CreatedBy = TestUserId.Value,
        ETag = eTag,
    };

    [Fact]
    public async Task Handle_WhenRegulatoryCalendarExistsForTenant_ReturnsItemWithEtag()
    {
        var regulatoryCalendarId = Guid.NewGuid();
        var regulatoryCalendar = RegulatoryCalendarFor(regulatoryCalendarId, TestTenantId.Value);
        _repo.GetByIdAsync(regulatoryCalendarId, TestTenantId.Value, Arg.Any<CancellationToken>()).Returns(regulatoryCalendar);

        var result = await Sut().Handle(new GetRegulatoryCalendarByIdQuery(TestTenantId.Value, regulatoryCalendarId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.RegulatoryCalendarId.Should().Be(regulatoryCalendarId);
        result.Value.ETag.Should().Be("0x0000000000009ABC");
    }

    [Fact]
    public async Task Handle_WhenRegulatoryCalendarDoesNotExist_ReturnsNotFoundWithCorrectSeverity()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((RegulatoryCalendarItemDto?)null);

        var result = await Sut().Handle(new GetRegulatoryCalendarByIdQuery(TestTenantId.Value, Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("Compliance.RegulatoryCalendar.NotFound");
        result.Error.Severity.Should().Be(ErrorSeverity.NotFound, "a missing RegulatoryCalendar record must map to HTTP 404, not the default 500");
    }

    [Fact]
    public async Task Handle_WhenRegulatoryCalendarBelongsToDifferentTenant_ReturnsNotFound()
    {
        // Mirrors production behavior: usp_RegulatoryCalendar_GetById filters by TenantId in SQL,
        // so a lookup scoped to TestTenantId for a RegulatoryCalendar record that actually belongs
        // to another tenant returns no row at all -- the repository is not stubbed for that
        // (RegulatoryCalendarId, TestTenantId) combination and NSubstitute's default (null)
        // applies, exactly as the real proc would.
        var regulatoryCalendarId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        _repo.GetByIdAsync(regulatoryCalendarId, otherTenantId, Arg.Any<CancellationToken>()).Returns(RegulatoryCalendarFor(regulatoryCalendarId, otherTenantId));

        var result = await Sut().Handle(new GetRegulatoryCalendarByIdQuery(TestTenantId.Value, regulatoryCalendarId), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("Compliance.RegulatoryCalendar.NotFound");
    }
}
