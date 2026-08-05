// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.HazardousWasteManagement.Domain
//  Layer:          Tests (Unit)
//  File:           DisposalCertificateTests.cs
//  Created:        2026-08-04
//  Author:         Testing Agent
//  SchemaVersion:  2026.2
//  Description:    Lifecycle/invariant tests for the DisposalCertificate aggregate root: record,
//                  verify, and invalid transition guards.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Domain.HazardousWasteManagement.DisposalCertificates.Enums;
using PlantSafety.Domain.HazardousWasteManagement.DisposalCertificates.Events;
using Agg = PlantSafety.Domain.HazardousWasteManagement.DisposalCertificates.Aggregates.DisposalCertificate;

namespace PlantSafety.Tests.HazardousWasteManagement.Domain;

public sealed class DisposalCertificateTests : UnitTestBase
{
    private static Agg RecordCertificate(DisposalMethod method = DisposalMethod.Incineration) =>
        Agg.Create(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            TestTenantId.Value, siteId: null,
            manifestId: Guid.NewGuid(), tsdfFacilityId: Guid.NewGuid(),
            disposalMethod: method, disposalDateUtc: FixedTimestamp);

    [Fact]
    public void Create_WithValidData_RecordsReceivedCertificateAndRaisesEvent()
    {
        var certificate = RecordCertificate();

        certificate.Status.Should().Be(DisposalCertificateStatus.Received);
        certificate.DisposalMethod.Should().Be(DisposalMethod.Incineration);
        certificate.CertificateReceivedAtUtc.Should().Be(FixedTimestamp);

        certificate.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<DisposalCertificateReceivedEvent>();
    }

    [Fact]
    public void Verify_FromReceived_TransitionsAndRaisesEvent()
    {
        var certificate = RecordCertificate();
        certificate.ClearDomainEvents();

        certificate.Verify(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        certificate.Status.Should().Be(DisposalCertificateStatus.Verified);
        certificate.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<DisposalCertificateVerifiedEvent>();
    }

    [Fact]
    public void Verify_WhenAlreadyVerified_ThrowsInvalidOperationException()
    {
        var certificate = RecordCertificate();
        certificate.Verify(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => certificate.Verify(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<InvalidOperationException>();
    }

    [Theory]
    [InlineData(DisposalMethod.Incineration)]
    [InlineData(DisposalMethod.Landfill)]
    [InlineData(DisposalMethod.Recycling)]
    [InlineData(DisposalMethod.Treatment)]
    public void Create_AcceptsAllDisposalMethods(DisposalMethod method)
    {
        var certificate = RecordCertificate(method);

        certificate.DisposalMethod.Should().Be(method);
    }
}
