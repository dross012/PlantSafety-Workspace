// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Compliance.Certification.Handlers
//  Layer:          Tests (Unit)
//  File:           GetCertificationByIdQueryHandlerTests.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Unit tests for GetCertificationByIdQueryHandler -- Phase 4
//                  Compliance.Certification full build-out. Covers: successful retrieval with
//                  ETag projection, not-found behavior with correct severity, and
//                  tenant-isolation. Mirrors
//                  PlantSafety.Tests.Compliance.Permits.Handlers.GetPermitsByIdQueryHandlerTests.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Compliance.Certification.Handlers;
using PlantSafety.Application.Compliance.Certification.Models;
using PlantSafety.Application.Compliance.Certification.Queries;
using PlantSafety.Application.Compliance.Certification.Repositories;

namespace PlantSafety.Tests.Compliance.Certification.Handlers;

public sealed class GetCertificationByIdQueryHandlerTests : UnitTestBase
{
    private readonly ICertificationRepository _repo = Substitute.For<ICertificationRepository>();

    private GetCertificationByIdQueryHandler Sut() => new(_repo, NullLogger<GetCertificationByIdQueryHandler>());

    private static CertificationItemDto CertificationFor(Guid certificationId, Guid tenantId, string? eTag = "0x0000000000009ABC") => new()
    {
        CertificationId = certificationId,
        TenantId = tenantId,
        Status = "Created",
        HolderWorkerId = TestUserId.Value,
        CertificationName = "Confined Space Entry Supervisor",
        CertificationBody = "OSHA",
        CertificationNumber = "CS-2026-0001",
        IssuedAtUtc = FixedUtcNow,
        RenewalRequired = true,
        CreatedAtUtc = FixedUtcNow,
        CreatedBy = TestUserId.Value,
        ETag = eTag,
    };

    [Fact]
    public async Task Handle_WhenCertificationExistsForTenant_ReturnsItemWithEtag()
    {
        var certificationId = Guid.NewGuid();
        var certification = CertificationFor(certificationId, TestTenantId.Value);
        _repo.GetByIdAsync(certificationId, TestTenantId.Value, Arg.Any<CancellationToken>()).Returns(certification);

        var result = await Sut().Handle(new GetCertificationByIdQuery(TestTenantId.Value, certificationId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.CertificationId.Should().Be(certificationId);
        result.Value.ETag.Should().Be("0x0000000000009ABC");
    }

    [Fact]
    public async Task Handle_WhenCertificationDoesNotExist_ReturnsNotFoundWithCorrectSeverity()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((CertificationItemDto?)null);

        var result = await Sut().Handle(new GetCertificationByIdQuery(TestTenantId.Value, Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("Compliance.Certification.NotFound");
        result.Error.Severity.Should().Be(ErrorSeverity.NotFound, "a missing Certification record must map to HTTP 404, not the default 500");
    }

    [Fact]
    public async Task Handle_WhenCertificationBelongsToDifferentTenant_ReturnsNotFound()
    {
        var certificationId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        _repo.GetByIdAsync(certificationId, otherTenantId, Arg.Any<CancellationToken>()).Returns(CertificationFor(certificationId, otherTenantId));

        var result = await Sut().Handle(new GetCertificationByIdQuery(TestTenantId.Value, certificationId), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("Compliance.Certification.NotFound");
    }
}
