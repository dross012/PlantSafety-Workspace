// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.CyberPhysicalSecurity.Domain
//  Layer:          Tests (Unit)
//  File:           OtAssetTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Testing Agent)
//  SchemaVersion:  2026.1
//  Description:    Functional/lifecycle unit tests for the OtAsset aggregate root (OT Asset
//                  Inventory subdomain of CyberPhysicalSecurity). Covers Register/Reconcile/
//                  Quarantine behavior, invariant enforcement, and invalid status transitions.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Domain.CyberPhysicalSecurity.OtAsset.Enums;
using PlantSafety.Domain.CyberPhysicalSecurity.OtAsset.Events;
using PlantSafety.Domain.CyberPhysicalSecurity.OtAsset.Exceptions;
using OtAssetAggregate = PlantSafety.Domain.CyberPhysicalSecurity.OtAsset.Aggregates.OtAsset;

namespace PlantSafety.Tests.CyberPhysicalSecurity.Domain;

public sealed class OtAssetTests : UnitTestBase
{
    private static OtAssetAggregate RegisteredAsset(OtAssetCriticality criticality = OtAssetCriticality.Medium) =>
        OtAssetAggregate.Register(
            createdBy: TestUserId,
            createdAtUtc: FixedTimestamp,
            correlationId: TestCorrelationId,
            causationId: TestCausationId,
            actor: TestActor,
            tenantId: TestTenantId.Value,
            siteId: Guid.NewGuid(),
            assetTag: "PLC-101",
            assetName: "Line 3 PLC",
            assetType: OtAssetType.Plc,
            criticality: criticality);

    [Fact]
    public void Register_WithValidData_CreatesActiveAssetAndRaisesEvent()
    {
        var asset = RegisteredAsset();

        asset.Status.Should().Be(OtAssetStatus.Active);
        asset.AssetTag.Should().Be("PLC-101");
        asset.CreatedBy.Should().Be(TestUserId);
        asset.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<OtAssetRegisteredEvent>();
    }

    [Fact]
    public void Register_TrimsWhitespaceFromTextFields()
    {
        var asset = OtAssetAggregate.Register(
            createdBy: TestUserId, createdAtUtc: FixedTimestamp, correlationId: TestCorrelationId,
            causationId: TestCausationId, actor: TestActor, tenantId: TestTenantId.Value, siteId: Guid.NewGuid(),
            assetTag: "  PLC-102  ", assetName: "  Line 4 PLC  ", assetType: OtAssetType.Plc,
            vendor: "  Siemens  ");

        asset.AssetTag.Should().Be("PLC-102");
        asset.AssetName.Should().Be("Line 4 PLC");
        asset.Vendor.Should().Be("Siemens");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Register_WithMissingAssetTag_ThrowsDomainException(string? assetTag)
    {
        var act = () => OtAssetAggregate.Register(
            createdBy: TestUserId, createdAtUtc: FixedTimestamp, correlationId: TestCorrelationId,
            causationId: TestCausationId, actor: TestActor, tenantId: TestTenantId.Value, siteId: Guid.NewGuid(),
            assetTag: assetTag!, assetName: "Line 3 PLC", assetType: OtAssetType.Plc);

        act.Should().Throw<OtAssetDomainException>();
    }

    [Fact]
    public void Reconcile_SetsLastReconciledTimestampAndRaisesEvent()
    {
        var asset = RegisteredAsset();
        asset.ClearDomainEvents();
        var reconciledAt = UtcTimestamp.From(FixedUtcNow.AddHours(1));

        asset.Reconcile(reconciledAt, TestActor, TestCorrelationId, TestCausationId);

        asset.LastReconciledAtUtc.Should().Be(reconciledAt);
        asset.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<OtAssetReconciledEvent>();
    }

    [Fact]
    public void Quarantine_FromActive_TransitionsToQuarantinedAndRaisesEvent()
    {
        var asset = RegisteredAsset();
        asset.ClearDomainEvents();

        asset.Quarantine("Unauthorized firmware detected", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        asset.Status.Should().Be(OtAssetStatus.Quarantined);
        asset.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<OtAssetQuarantinedEvent>();
    }

    [Fact]
    public void Quarantine_WhenAlreadyQuarantined_ThrowsInvalidStatusTransition()
    {
        var asset = RegisteredAsset();
        asset.Quarantine("Reason 1", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => asset.Quarantine("Reason 2", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<OtAssetInvalidStatusTransitionException>();
    }

    [Fact]
    public void ClearDomainEvents_RemovesAllPendingEvents()
    {
        var asset = RegisteredAsset();
        asset.DomainEvents.Should().NotBeEmpty();

        asset.ClearDomainEvents();

        asset.DomainEvents.Should().BeEmpty();
    }
}
