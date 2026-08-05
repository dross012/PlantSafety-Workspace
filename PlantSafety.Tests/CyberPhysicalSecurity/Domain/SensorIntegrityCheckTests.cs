// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.CyberPhysicalSecurity.Domain
//  Layer:          Tests (Unit)
//  File:           SensorIntegrityCheckTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Testing Agent)
//  SchemaVersion:  2026.1
//  Description:    Functional unit tests for the SensorIntegrityCheck aggregate root (Sensor
//                  Integrity Monitoring subdomain). Covers Run behavior for each Result outcome and
//                  the recurring check-due scheduling hook.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Domain.CyberPhysicalSecurity.OtAsset.ValueObjects.Ids;
using PlantSafety.Domain.CyberPhysicalSecurity.SensorIntegrityCheck.Enums;
using PlantSafety.Domain.CyberPhysicalSecurity.SensorIntegrityCheck.Events;
using SensorIntegrityCheckAggregate = PlantSafety.Domain.CyberPhysicalSecurity.SensorIntegrityCheck.Aggregates.SensorIntegrityCheck;

namespace PlantSafety.Tests.CyberPhysicalSecurity.Domain;

public sealed class SensorIntegrityCheckTests : UnitTestBase
{
    private static SensorIntegrityCheckAggregate CheckWithResult(SensorIntegrityCheckResult result) =>
        SensorIntegrityCheckAggregate.Run(
            createdBy: TestUserId,
            createdAtUtc: FixedTimestamp,
            correlationId: TestCorrelationId,
            causationId: TestCausationId,
            actor: TestActor,
            tenantId: TestTenantId.Value,
            siteId: Guid.NewGuid(),
            otAssetId: OtAssetId.New(),
            checkType: SensorIntegrityCheckType.Calibration,
            result: result,
            details: "Calibration drift within tolerance",
            checkedAtUtc: FixedTimestamp,
            nextCheckDueUtc: UtcTimestamp.From(FixedUtcNow.AddDays(30)));

    [Fact]
    public void Run_WithPassResult_RaisesOnlyRunEvent()
    {
        var check = CheckWithResult(SensorIntegrityCheckResult.Pass);

        check.Result.Should().Be(SensorIntegrityCheckResult.Pass);
        check.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<SensorIntegrityCheckRunEvent>();
    }

    [Fact]
    public void Run_WithFailResult_RaisesRunAndFailedEvents()
    {
        var check = CheckWithResult(SensorIntegrityCheckResult.Fail);

        check.DomainEvents.Should().HaveCount(2);
        check.DomainEvents.Should().ContainSingle(e => e is SensorIntegrityCheckRunEvent);
        check.DomainEvents.Should().ContainSingle(e => e is SensorIntegrityCheckFailedEvent);
    }

    [Fact]
    public void Run_WithInconclusiveResult_RaisesOnlyRunEvent()
    {
        var check = CheckWithResult(SensorIntegrityCheckResult.Inconclusive);

        check.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<SensorIntegrityCheckRunEvent>();
    }

    [Fact]
    public void Run_SetsNextCheckDueUtc_ForRecurringSchedulingHook()
    {
        var check = CheckWithResult(SensorIntegrityCheckResult.Pass);

        check.NextCheckDueUtc.Should().Be(UtcTimestamp.From(FixedUtcNow.AddDays(30)));
    }

    [Fact]
    public void Run_WithoutNextCheckDue_LeavesItNull()
    {
        var check = SensorIntegrityCheckAggregate.Run(
            createdBy: TestUserId, createdAtUtc: FixedTimestamp, correlationId: TestCorrelationId,
            causationId: TestCausationId, actor: TestActor, tenantId: TestTenantId.Value, siteId: Guid.NewGuid(),
            otAssetId: OtAssetId.New(), checkType: SensorIntegrityCheckType.Signal,
            result: SensorIntegrityCheckResult.Pass, details: "OK", checkedAtUtc: FixedTimestamp);

        check.NextCheckDueUtc.Should().BeNull();
    }

    [Fact]
    public void ClearDomainEvents_RemovesAllPendingEvents()
    {
        var check = CheckWithResult(SensorIntegrityCheckResult.Fail);
        check.DomainEvents.Should().NotBeEmpty();

        check.ClearDomainEvents();

        check.DomainEvents.Should().BeEmpty();
    }
}
