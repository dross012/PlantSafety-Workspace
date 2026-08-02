// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Compliance.Certification.Commands
//  Layer:          Tests (Unit)
//  File:           VerifyCertificationCommandHandlerTests.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Regression tests for the Phase 4 Compliance.Certification full build-out.
//                  VerifyCertificationCommandHandler must forward the caller's ETag to
//                  ICertificationRepository.UpdateAsync, and must let a ConcurrencyConflictException
//                  raised by the repository propagate unchanged rather than swallowing it into a
//                  generic ApplicationError. Mirrors
//                  PlantSafety.Tests.Compliance.Permits.Commands.ApprovePermitsCommandHandlerTests.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Compliance.Certification.Commands;
using PlantSafety.Domain.Compliance.Certification.Interfaces;
using PlantSafety.Domain.Compliance.Certification.ValueObjects.Ids;
using CertificationAggregate = PlantSafety.Domain.Compliance.Certification.Aggregates.Certification;

namespace PlantSafety.Tests.Compliance.Certification.Commands;

public sealed class VerifyCertificationCommandHandlerTests : UnitTestBase
{
    private readonly ICertificationRepository _repo = Substitute.For<ICertificationRepository>();

    private VerifyCertificationCommandHandler Sut() => new(_repo, NullLogger<VerifyCertificationCommandHandler>());

    private static CertificationAggregate CreatedCertification() =>
        CertificationAggregate.Create(
            createdBy: TestUserId,
            createdAtUtc: FixedTimestamp,
            correlationId: TestCorrelationId,
            causationId: TestCausationId,
            actor: TestActor,
            tenantId: TestTenantId.Value,
            holderWorkerId: TestUserId,
            certificationName: "Confined Space Entry Supervisor",
            certificationBody: "OSHA",
            certificationNumber: "CS-2026-0001");

    private static VerifyCertificationCommand VerifyCommandFor(CertificationAggregate certification, string? eTag) =>
        new(
            CommandId: Guid.NewGuid(),
            CorrelationId: TestCorrelationId.Value,
            CausationId: TestCausationId.Value,
            TenantId: TestTenantId.Value,
            CertificationId: certification.CertificationId.Value,
            VerifiedByUserId: TestUserId.Value)
        { ETag = eTag };

    [Fact]
    public async Task Handle_WithValidCommand_ForwardsCallerEtagToRepository()
    {
        var certification = CreatedCertification();
        _repo.GetByIdAsync(Arg.Any<CertificationId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(certification);

        var command = VerifyCommandFor(certification, "0x0000000000005678");

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _repo.Received(1).UpdateAsync(certification, "0x0000000000005678", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRepositoryReportsConcurrencyConflict_PropagatesException()
    {
        var certification = CreatedCertification();
        _repo.GetByIdAsync(Arg.Any<CertificationId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(certification);
        _repo.UpdateAsync(Arg.Any<CertificationAggregate>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new ConcurrencyConflictException("Certification was modified by another process or the ETag is stale.", TestCorrelationId.Value));

        var command = VerifyCommandFor(certification, "stale-etag");

        await Assert.ThrowsAsync<ConcurrencyConflictException>(() => Sut().Handle(command, CancellationToken.None));
    }
}
