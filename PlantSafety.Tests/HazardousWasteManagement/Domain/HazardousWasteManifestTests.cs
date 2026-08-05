// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.HazardousWasteManagement.Domain
//  Layer:          Tests (Unit)
//  File:           HazardousWasteManifestTests.cs
//  Created:        2026-08-04
//  Author:         Testing Agent
//  SchemaVersion:  2026.2
//  Description:    Lifecycle/invariant tests for the HazardousWasteManifest aggregate root:
//                  generate, transporter pickup (immutability), TSDF receipt, close, discrepancy
//                  flagging, 3-year retention invariant, and invalid transition guards.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Domain.HazardousWasteManagement.Manifests.Enums;
using PlantSafety.Domain.HazardousWasteManagement.Manifests.Events;
using Agg = PlantSafety.Domain.HazardousWasteManagement.Manifests.Aggregates.HazardousWasteManifest;

namespace PlantSafety.Tests.HazardousWasteManagement.Domain;

public sealed class HazardousWasteManifestTests : UnitTestBase
{
    private static Agg GenerateManifest() =>
        Agg.Create(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            TestTenantId.Value, siteId: null,
            manifestNumber: "MAN-2026-000123",
            generatorEpaId: "EPA123456789",
            wasteStreamId: Guid.NewGuid());

    [Fact]
    public void Create_WithValidData_GeneratesManifestAndSetsThreeYearRetention()
    {
        var manifest = GenerateManifest();

        manifest.Status.Should().Be(HazardousWasteManifestStatus.Generated);
        manifest.IsImmutable.Should().BeFalse();
        manifest.RetentionExpiresAtUtc.Value.Should().Be(FixedUtcNow.AddYears(Agg.MinimumRetentionYears));
        manifest.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<ManifestGeneratedEvent>();
    }

    [Fact]
    public void RecordTransporterPickup_FromGenerated_BecomesImmutableAndInTransit()
    {
        var manifest = GenerateManifest();
        manifest.ClearDomainEvents();
        var transporterId = Guid.NewGuid();

        manifest.RecordTransporterPickup(transporterId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        manifest.Status.Should().Be(HazardousWasteManifestStatus.InTransit);
        manifest.IsImmutable.Should().BeTrue();
        manifest.TransporterSignedAtUtc.Should().Be(FixedTimestamp);
        manifest.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<TransporterPickupRecordedEvent>();
    }

    [Fact]
    public void RecordTransporterPickup_CalledTwice_ThrowsInvalidOperationException()
    {
        var manifest = GenerateManifest();
        manifest.RecordTransporterPickup(Guid.NewGuid(), FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => manifest.RecordTransporterPickup(Guid.NewGuid(), FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void RecordTsdfReceipt_FromInTransit_TransitionsAndRaisesEvent()
    {
        var manifest = GenerateManifest();
        manifest.RecordTransporterPickup(Guid.NewGuid(), FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        manifest.ClearDomainEvents();
        var tsdfId = Guid.NewGuid();

        manifest.RecordTsdfReceipt(tsdfId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        manifest.Status.Should().Be(HazardousWasteManifestStatus.ReceivedAtTsdf);
        manifest.TsdfReceivedAtUtc.Should().Be(FixedTimestamp);
        manifest.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<TsdfReceiptRecordedEvent>();
    }

    [Fact]
    public void RecordTsdfReceipt_BeforeTransporterPickup_ThrowsInvalidOperationException()
    {
        var manifest = GenerateManifest();

        var act = () => manifest.RecordTsdfReceipt(Guid.NewGuid(), FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void RecordTsdfReceipt_WhenDiscrepancyFlagged_ThrowsInvalidOperationException()
    {
        // NOTE: FlagDiscrepancy transitions Status away from InTransit to DiscrepancyFlagged, so
        // the subsequent RecordTsdfReceipt call is rejected by the EnsureStatusIsOneOf(InTransit)
        // guard before EnsureNotDiscrepant() is ever reached -- the resulting message is the
        // generic "Invalid lifecycle transition" text, not the open-discrepancy-specific one.
        var manifest = GenerateManifest();
        manifest.RecordTransporterPickup(Guid.NewGuid(), FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        manifest.FlagDiscrepancy("Quantity mismatch", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => manifest.RecordTsdfReceipt(Guid.NewGuid(), FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Invalid lifecycle transition*");
    }

    [Fact]
    public void Close_FromReceivedAtTsdf_TransitionsAndRaisesEvent()
    {
        var manifest = GenerateManifest();
        manifest.RecordTransporterPickup(Guid.NewGuid(), FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        manifest.RecordTsdfReceipt(Guid.NewGuid(), FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        manifest.ClearDomainEvents();

        manifest.Close(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        manifest.Status.Should().Be(HazardousWasteManifestStatus.Closed);
        manifest.ClosedAtUtc.Should().Be(FixedTimestamp);
        manifest.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<ManifestClosedEvent>();
    }

    [Fact]
    public void Close_BeforeTsdfReceipt_ThrowsInvalidOperationException()
    {
        var manifest = GenerateManifest();
        manifest.RecordTransporterPickup(Guid.NewGuid(), FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => manifest.Close(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void FlagDiscrepancy_WithReason_SetsDiscrepancyFlaggedStatusAndRaisesEvent()
    {
        var manifest = GenerateManifest();
        manifest.ClearDomainEvents();

        manifest.FlagDiscrepancy("Waste code mismatch on line 3", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        manifest.Status.Should().Be(HazardousWasteManifestStatus.DiscrepancyFlagged);
        manifest.DiscrepancyReason.Should().Be("Waste code mismatch on line 3");
        manifest.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<ManifestDiscrepancyFlaggedEvent>();
    }

    [Fact]
    public void FlagDiscrepancy_WithEmptyReason_ThrowsArgumentException()
    {
        var manifest = GenerateManifest();

        var act = () => manifest.FlagDiscrepancy("   ", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void FlagDiscrepancy_DoesNotMutateImmutableCoreFieldsAfterTransporterSigned()
    {
        var manifest = GenerateManifest();
        manifest.RecordTransporterPickup(Guid.NewGuid(), FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        var signedAt = manifest.TransporterSignedAtUtc;

        manifest.FlagDiscrepancy("Signature mismatch", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        manifest.IsImmutable.Should().BeTrue();
        manifest.TransporterSignedAtUtc.Should().Be(signedAt);
    }
}
