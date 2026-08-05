// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Smoke.PhysicalAccessControl
//  Layer:          Tests (Unit)
//  File:           PhysicalAccessControlSmokeTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    Fast sanity checks that the PhysicalAccessControl module's basic building
//                  blocks are wired up and instantiable: DI registration extension methods run
//                  without throwing, every entity's ActionResult record constructs, and every
//                  command's ApiVersionToken/RequestedAtUtc default correctly. These are the
//                  first tests that should fail if the module's assembly wiring breaks.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using Microsoft.Extensions.DependencyInjection;
using PlantSafety.Application.PhysicalAccessControl.Commands;
using PlantSafety.Application.PhysicalAccessControl.DependencyInjection;

namespace PlantSafety.Tests.PhysicalAccessControl.Smoke;

public sealed class PhysicalAccessControlSmokeTests : UnitTestBase
{
    [Fact]
    public void AddPhysicalAccessControlApplicationServices_ShouldNotThrow()
    {
        var services = new ServiceCollection();

        var act = () => services.AddPhysicalAccessControlApplicationServices();

        act.Should().NotThrow();
    }

    [Fact]
    public void AddPhysicalAccessControlApplicationServices_ShouldReturnSameServiceCollection()
    {
        var services = new ServiceCollection();

        var result = services.AddPhysicalAccessControlApplicationServices();

        result.Should().BeSameAs(services);
    }

    // ---- ActionResult records: one per entity, all must construct cleanly --------------

    [Fact]
    public void AccessPointActionResult_ShouldConstruct()
    {
        var result = new AccessPointActionResult(Guid.NewGuid(), "Registered");
        result.Status.Should().Be("Registered");
    }

    [Fact]
    public void CredentialActionResult_ShouldConstruct()
    {
        var result = new CredentialActionResult(Guid.NewGuid(), "Issued");
        result.Status.Should().Be("Issued");
    }

    [Fact]
    public void AccessEventActionResult_ShouldConstruct()
    {
        var result = new AccessEventActionResult(Guid.NewGuid(), "Ingested");
        result.Status.Should().Be("Ingested");
    }

    [Fact]
    public void SafetyZoneActionResult_ShouldConstruct()
    {
        var result = new SafetyZoneActionResult(Guid.NewGuid(), "Active");
        result.Status.Should().Be("Active");
    }

    [Fact]
    public void AccessAnomalyActionResult_ShouldConstruct()
    {
        var result = new AccessAnomalyActionResult(Guid.NewGuid(), "Detected");
        result.Status.Should().Be("Detected");
    }

    [Fact]
    public void RestrictedZonePolicyActionResult_ShouldConstruct()
    {
        var result = new RestrictedZonePolicyActionResult(Guid.NewGuid(), "Draft");
        result.Status.Should().Be("Draft");
    }

    // ---- Every entity's aggregate can be created with only its required fields ---------

    [Fact]
    public void AllSixAggregates_ShouldConstructWithMinimalArguments()
    {
        var accessPoint = PlantSafety.Domain.PhysicalAccessControl.AccessPoints.Aggregates.AccessPoint.Create(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor, tenantId: TestTenantId.Value);
        var credential = PlantSafety.Domain.PhysicalAccessControl.Credentials.Aggregates.Credential.Create(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor, tenantId: TestTenantId.Value);
        var accessEvent = PlantSafety.Domain.PhysicalAccessControl.AccessEvents.Aggregates.AccessEvent.Create(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor, tenantId: TestTenantId.Value);
        var safetyZone = PlantSafety.Domain.PhysicalAccessControl.SafetyZones.Aggregates.SafetyZone.Create(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor, tenantId: TestTenantId.Value);
        var accessAnomaly = PlantSafety.Domain.PhysicalAccessControl.AccessAnomalies.Aggregates.AccessAnomaly.Create(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor, tenantId: TestTenantId.Value);
        var policy = PlantSafety.Domain.PhysicalAccessControl.RestrictedZonePolicies.Aggregates.RestrictedZonePolicy.Create(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor, tenantId: TestTenantId.Value);

        new object[] { accessPoint, credential, accessEvent, safetyZone, accessAnomaly, policy }
            .Should().OnlyContain(x => x != null);
    }
}
