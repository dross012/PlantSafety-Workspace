// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Standards.Compliance
//  Layer:          Application
//  File:           StandardsComplianceTenantSiteSpecifications.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Concrete tenant/site-scoped specification subclasses for the seven
//                  MachineSafety Domain aggregates consumed by the Compliance.Standards.Compliance
//                  command handlers (CraneHoistSystems, MachineGuardsBarriers, LotoSystems,
//                  EmergencyStopDevices, TwoHandControls, SafetyValvesRelays,
//                  PresenceSensingDevices). Each aggregate's own abstract
//                  Specifications\<Aggregate>Specification base class exposes only
//                  ToExpression()/ToPredicate() -- no pre-existing concrete "by tenant/site"
//                  specification was found in the Domain or Application layers for any of these
//                  seven aggregates (verified by search before writing this file), so this file
//                  supplies the one concrete filter every EvaluateX command handler in this module
//                  needs: match TenantId, and match SiteId when a site scope was supplied (a null
//                  SiteId filter matches every site, mirroring the "SiteId is optional filter"
//                  convention used elsewhere in this codebase). Declared once here so the six
//                  command handlers don't each duplicate near-identical specification
//                  subclasses. Additive-only evolution.
// ============================================================================
using System.Linq.Expressions;
using DomainCraneHoistSystems = PlantSafety.Domain.MachineSafety.CraneHoistSystems.Aggregates.CraneHoistSystems;
using DomainMachineGuardsBarriers = PlantSafety.Domain.MachineSafety.MachineGuardsBarriers.Aggregates.MachineGuardsBarriers;
using DomainLotoSystems = PlantSafety.Domain.MachineSafety.LotoSystems.Aggregates.LotoSystems;
using DomainEmergencyStopDevices = PlantSafety.Domain.MachineSafety.EmergencyStopDevices.Aggregates.EmergencyStopDevices;
using DomainTwoHandControls = PlantSafety.Domain.MachineSafety.TwoHandControls.Aggregates.TwoHandControls;
using DomainSafetyValvesRelays = PlantSafety.Domain.MachineSafety.SafetyValvesRelays.Aggregates.SafetyValvesRelays;
using DomainPresenceSensingDevices = PlantSafety.Domain.MachineSafety.PresenceSensingDevices.Aggregates.PresenceSensingDevices;
using PlantSafety.Domain.MachineSafety.CraneHoistSystems.Specifications;
using PlantSafety.Domain.MachineSafety.MachineGuardsBarriers.Specifications;
using PlantSafety.Domain.MachineSafety.LotoSystems.Specifications;
using PlantSafety.Domain.MachineSafety.EmergencyStopDevices.Specifications;
using PlantSafety.Domain.MachineSafety.TwoHandControls.Specifications;
using PlantSafety.Domain.MachineSafety.SafetyValvesRelays.Specifications;
using PlantSafety.Domain.MachineSafety.PresenceSensingDevices.Specifications;

namespace PlantSafety.Application.Compliance.Standards.Compliance.Specifications;

/// <summary>Matches <see cref="DomainCraneHoistSystems"/> aggregates for a tenant, optionally scoped to a single site.</summary>
public sealed class CraneHoistSystemsTenantSiteSpecification(Guid tenantId, Guid? siteId) : CraneHoistSystemsSpecification
{
    /// <inheritdoc/>
    public override Expression<Func<DomainCraneHoistSystems, bool>> ToExpression() =>
        x => x.TenantId == tenantId && (siteId == null || x.SiteId == siteId);
}

/// <summary>Matches <see cref="DomainMachineGuardsBarriers"/> aggregates for a tenant, optionally scoped to a single site.</summary>
public sealed class MachineGuardsBarriersTenantSiteSpecification(Guid tenantId, Guid? siteId) : MachineGuardsBarriersSpecification
{
    /// <inheritdoc/>
    public override Expression<Func<DomainMachineGuardsBarriers, bool>> ToExpression() =>
        x => x.TenantId == tenantId && (siteId == null || x.SiteId == siteId);
}

/// <summary>Matches <see cref="DomainLotoSystems"/> aggregates for a tenant, optionally scoped to a single site.</summary>
public sealed class LotoSystemsTenantSiteSpecification(Guid tenantId, Guid? siteId) : LotoSystemsSpecification
{
    /// <inheritdoc/>
    public override Expression<Func<DomainLotoSystems, bool>> ToExpression() =>
        x => x.TenantId == tenantId && (siteId == null || x.SiteId == siteId);
}

/// <summary>Matches <see cref="DomainEmergencyStopDevices"/> aggregates for a tenant, optionally scoped to a single site.</summary>
public sealed class EmergencyStopDevicesTenantSiteSpecification(Guid tenantId, Guid? siteId) : EmergencyStopDevicesSpecification
{
    /// <inheritdoc/>
    public override Expression<Func<DomainEmergencyStopDevices, bool>> ToExpression() =>
        x => x.TenantId == tenantId && (siteId == null || x.SiteId == siteId);
}

/// <summary>Matches <see cref="DomainTwoHandControls"/> aggregates for a tenant, optionally scoped to a single site.</summary>
public sealed class TwoHandControlsTenantSiteSpecification(Guid tenantId, Guid? siteId) : TwoHandControlsSpecification
{
    /// <inheritdoc/>
    public override Expression<Func<DomainTwoHandControls, bool>> ToExpression() =>
        x => x.TenantId == tenantId && (siteId == null || x.SiteId == siteId);
}

/// <summary>Matches <see cref="DomainSafetyValvesRelays"/> aggregates for a tenant, optionally scoped to a single site.</summary>
public sealed class SafetyValvesRelaysTenantSiteSpecification(Guid tenantId, Guid? siteId) : SafetyValvesRelaysSpecification
{
    /// <inheritdoc/>
    public override Expression<Func<DomainSafetyValvesRelays, bool>> ToExpression() =>
        x => x.TenantId == tenantId && (siteId == null || x.SiteId == siteId);
}

/// <summary>Matches <see cref="DomainPresenceSensingDevices"/> aggregates for a tenant, optionally scoped to a single site.</summary>
public sealed class PresenceSensingDevicesTenantSiteSpecification(Guid tenantId, Guid? siteId) : PresenceSensingDevicesSpecification
{
    /// <inheritdoc/>
    public override Expression<Func<DomainPresenceSensingDevices, bool>> ToExpression() =>
        x => x.TenantId == tenantId && (siteId == null || x.SiteId == siteId);
}
