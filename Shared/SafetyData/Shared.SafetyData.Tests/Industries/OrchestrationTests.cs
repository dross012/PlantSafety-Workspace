// ============================================================================
//  Project:        Shared.SafetyData
//  Module:         Tests.Industries
//  Layer:          Tests (Unit)
//  File:           OrchestrationTests.cs
//  Created:        2026-08-08
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    Unit tests for the interim runtime event orchestration
//                  layer: OrchestrationEventCategoryCatalog, TenantFacilityRegistry
//                  (including its Path-A validation/Unregister/GetAll
//                  additions), and EventOrchestrationService.
// ============================================================================
using Shared.SafetyData.Abstractions.Models.Industries.ComplianceRisk;
using Shared.SafetyData.Abstractions.Models.Industries.Orchestration;
using Shared.SafetyData.Infrastructure.Industries;

using Xunit;

namespace Shared.SafetyData.Tests.Industries;

public sealed class OrchestrationTests
{
    [Theory]
    [InlineData("Accidents.AccidentCreated", OrchestrationEventCategory.Created)]
    [InlineData("Risk.RiskCreated", OrchestrationEventCategory.Created)]
    [InlineData("Accidents.AccidentArchived", OrchestrationEventCategory.Archived)]
    [InlineData("Risk.RiskArchived", OrchestrationEventCategory.Archived)]
    [InlineData("Accidents.AccidentInvestigatorAssigned", OrchestrationEventCategory.Other)]
    public void OrchestrationEventCategoryCatalog_ForEventType_ClassifiesBySuffix(string eventType, OrchestrationEventCategory expected)
    {
        Assert.Equal(expected, OrchestrationEventCategoryCatalog.ForEventType(eventType));
    }

    [Fact]
    public void TenantFacilityRegistry_Register_ThenTryResolve_RoundTrips()
    {
        var registry = new TenantFacilityRegistry();
        var registration = new TenantFacilityRegistration("tenant-1", Guid.NewGuid(), "Plant 1", "Medical Devices", "Loc");

        registry.Register(registration);

        Assert.True(registry.TryResolve("tenant-1", out TenantFacilityRegistration? resolved));
        Assert.Equal(registration, resolved);
    }

    [Fact]
    public void TenantFacilityRegistry_TryResolve_UnknownTenant_ReturnsFalse()
    {
        var registry = new TenantFacilityRegistry();

        Assert.False(registry.TryResolve("unknown", out TenantFacilityRegistration? resolved));
        Assert.Null(resolved);
    }

    [Fact]
    public void TenantFacilityRegistry_Register_EmptyTenantId_Throws()
    {
        var registry = new TenantFacilityRegistry();

        Assert.Throws<ArgumentException>(() =>
            registry.Register(new TenantFacilityRegistration("", Guid.NewGuid(), "X", "Medical Devices", "Loc")));
    }

    [Fact]
    public void TenantFacilityRegistry_Register_EmptyFacilityId_Throws()
    {
        var registry = new TenantFacilityRegistry();

        Assert.Throws<ArgumentException>(() =>
            registry.Register(new TenantFacilityRegistration("tenant-1", Guid.Empty, "X", "Medical Devices", "Loc")));
    }

    [Fact]
    public void TenantFacilityRegistry_Register_EmptyName_Throws()
    {
        var registry = new TenantFacilityRegistry();

        Assert.Throws<ArgumentException>(() =>
            registry.Register(new TenantFacilityRegistration("tenant-1", Guid.NewGuid(), " ", "Medical Devices", "Loc")));
    }

    [Fact]
    public void TenantFacilityRegistry_Register_EmptyIndustryName_Throws()
    {
        var registry = new TenantFacilityRegistry();

        Assert.Throws<ArgumentException>(() =>
            registry.Register(new TenantFacilityRegistration("tenant-1", Guid.NewGuid(), "X", "", "Loc")));
    }

    [Fact]
    public void TenantFacilityRegistry_Unregister_RemovesRegistration()
    {
        var registry = new TenantFacilityRegistry();
        registry.Register(new TenantFacilityRegistration("tenant-1", Guid.NewGuid(), "X", "Medical Devices", "Loc"));

        Assert.True(registry.Unregister("tenant-1"));
        Assert.False(registry.TryResolve("tenant-1", out _));
    }

    [Fact]
    public void TenantFacilityRegistry_Unregister_UnknownTenant_ReturnsFalse()
    {
        var registry = new TenantFacilityRegistry();

        Assert.False(registry.Unregister("unknown"));
    }

    [Fact]
    public void TenantFacilityRegistry_GetAll_ReturnsEveryRegistration()
    {
        var registry = new TenantFacilityRegistry();
        registry.Register(new TenantFacilityRegistration("tenant-1", Guid.NewGuid(), "A", "Medical Devices", "Loc A"));
        registry.Register(new TenantFacilityRegistration("tenant-2", Guid.NewGuid(), "B", "Chemical Manufacturing", "Loc B"));

        Assert.Equal(2, registry.GetAll().Count);
    }

    [Fact]
    public void TenantFacilityRegistry_Register_SameTenantTwice_ReplacesRegistration()
    {
        var registry = new TenantFacilityRegistry();
        var facilityIdA = Guid.NewGuid();
        var facilityIdB = Guid.NewGuid();

        registry.Register(new TenantFacilityRegistration("tenant-1", facilityIdA, "A", "Medical Devices", "Loc A"));
        registry.Register(new TenantFacilityRegistration("tenant-1", facilityIdB, "B", "Chemical Manufacturing", "Loc B"));

        Assert.True(registry.TryResolve("tenant-1", out TenantFacilityRegistration? resolved));
        Assert.Equal(facilityIdB, resolved!.FacilityId);
        Assert.Single(registry.GetAll());
    }

    [Fact]
    public void EventOrchestrationService_Handle_CreatedEvent_SetsCreatedCategory()
    {
        var registry = new TenantFacilityRegistry();
        registry.Register(new TenantFacilityRegistration("tenant-1", Guid.NewGuid(), "X", "Medical Devices", "Loc"));
        var facilityProfileService = new FacilityProfileService();
        var complianceRiskService = new ComplianceRiskService();
        var orchestrationService = new EventOrchestrationService(registry, facilityProfileService, complianceRiskService);

        FacilityEventOrchestrationResult? result = orchestrationService.Handle("tenant-1", "PlantSafety", "Accidents.AccidentCreated", DateTime.UtcNow);

        Assert.NotNull(result);
        Assert.Equal(OrchestrationEventCategory.Created, result!.Category);
    }

    [Fact]
    public void EventOrchestrationService_Handle_ArchivedEvent_SetsArchivedCategory()
    {
        var registry = new TenantFacilityRegistry();
        registry.Register(new TenantFacilityRegistration("tenant-1", Guid.NewGuid(), "X", "Medical Devices", "Loc"));
        var facilityProfileService = new FacilityProfileService();
        var complianceRiskService = new ComplianceRiskService();
        var orchestrationService = new EventOrchestrationService(registry, facilityProfileService, complianceRiskService);

        FacilityEventOrchestrationResult? result = orchestrationService.Handle("tenant-1", "PlantSafety", "Accidents.AccidentArchived", DateTime.UtcNow);

        Assert.Equal(OrchestrationEventCategory.Archived, result!.Category);
    }

    [Fact]
    public void EventOrchestrationService_Handle_NothingCompleted_RequiresImmediateAttention()
    {
        var registry = new TenantFacilityRegistry();
        registry.Register(new TenantFacilityRegistration("tenant-1", Guid.NewGuid(), "X", "Chemical Manufacturing", "Loc"));
        var facilityProfileService = new FacilityProfileService();
        var complianceRiskService = new ComplianceRiskService();
        var orchestrationService = new EventOrchestrationService(registry, facilityProfileService, complianceRiskService);

        FacilityEventOrchestrationResult? result = orchestrationService.Handle("tenant-1", "PlantSafety", "Accidents.AccidentCreated", DateTime.UtcNow);

        // Chemical Manufacturing is a 9-flag industry (max in the taxonomy) with nothing
        // completed: ComplianceTier is NonCompliant, so RequiresImmediateAttention must be true.
        Assert.True(result!.RequiresImmediateAttention);
        Assert.Equal(ComplianceTier.NonCompliant, result.ComplianceScore.ComplianceTier);
    }

    [Fact]
    public void EventOrchestrationService_Handle_UnregisteredTenant_ReturnsNull()
    {
        var registry = new TenantFacilityRegistry();
        var facilityProfileService = new FacilityProfileService();
        var complianceRiskService = new ComplianceRiskService();
        var orchestrationService = new EventOrchestrationService(registry, facilityProfileService, complianceRiskService);

        Assert.Null(orchestrationService.Handle("unregistered", "PlantSafety", "Accidents.AccidentCreated", DateTime.UtcNow));
    }
}
