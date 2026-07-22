// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyPolicies
//  Layer:          Infrastructure
//  File:           SafetyPoliciesCategoryDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Single Dapper repository implementing all 17 SafetyPolicies category
//                  read-model Application-layer repository interfaces (one per PolicyCategory
//                  taxonomy entry). Each method calls the single shared
//                  [SafetyPolicies].[usp_SafetyPolicies_GetByCategory] stored procedure with its
//                  own literal @Category value, avoiding 17 near-duplicate query shapes.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.SafetyPolicies.ConfinedSpaceEntry.Models;
using PlantSafety.Application.SafetyPolicies.ConfinedSpaceEntry.Repositories;
using PlantSafety.Application.SafetyPolicies.ContractorSafety.Models;
using PlantSafety.Application.SafetyPolicies.ContractorSafety.Repositories;
using PlantSafety.Application.SafetyPolicies.ElectricalSafety.Models;
using PlantSafety.Application.SafetyPolicies.ElectricalSafety.Repositories;
using PlantSafety.Application.SafetyPolicies.EmergencyResponseEvacuation.Models;
using PlantSafety.Application.SafetyPolicies.EmergencyResponseEvacuation.Repositories;
using PlantSafety.Application.SafetyPolicies.EnvironmentalChemicalSafety.Models;
using PlantSafety.Application.SafetyPolicies.EnvironmentalChemicalSafety.Repositories;
using PlantSafety.Application.SafetyPolicies.EquipmentMachineOperation.Models;
using PlantSafety.Application.SafetyPolicies.EquipmentMachineOperation.Repositories;
using PlantSafety.Application.SafetyPolicies.HazardCommunicationAssessment.Models;
using PlantSafety.Application.SafetyPolicies.HazardCommunicationAssessment.Repositories;
using PlantSafety.Application.SafetyPolicies.HotWork.Models;
using PlantSafety.Application.SafetyPolicies.HotWork.Repositories;
using PlantSafety.Application.SafetyPolicies.HousekeepingErgonomics.Models;
using PlantSafety.Application.SafetyPolicies.HousekeepingErgonomics.Repositories;
using PlantSafety.Application.SafetyPolicies.IncidentInvestigationCorrectiveAction.Models;
using PlantSafety.Application.SafetyPolicies.IncidentInvestigationCorrectiveAction.Repositories;
using PlantSafety.Application.SafetyPolicies.MachineMaintenanceInspection.Models;
using PlantSafety.Application.SafetyPolicies.MachineMaintenanceInspection.Repositories;
using PlantSafety.Application.SafetyPolicies.MaterialHandlingStorage.Models;
using PlantSafety.Application.SafetyPolicies.MaterialHandlingStorage.Repositories;
using PlantSafety.Application.SafetyPolicies.PersonalProtectiveEquipment.Models;
using PlantSafety.Application.SafetyPolicies.PersonalProtectiveEquipment.Repositories;
using PlantSafety.Application.SafetyPolicies.SafetyCultureBehavioral.Models;
using PlantSafety.Application.SafetyPolicies.SafetyCultureBehavioral.Repositories;
using PlantSafety.Application.SafetyPolicies.SafetyTrainingReporting.Models;
using PlantSafety.Application.SafetyPolicies.SafetyTrainingReporting.Repositories;
using PlantSafety.Application.SafetyPolicies.SecurityAccessControl.Models;
using PlantSafety.Application.SafetyPolicies.SecurityAccessControl.Repositories;
using PlantSafety.Application.SafetyPolicies.WorkingAtHeights.Models;
using PlantSafety.Application.SafetyPolicies.WorkingAtHeights.Repositories;

namespace PlantSafety.Infrastructure.SafetyPolicies.Persistence;

/// <summary>
/// Dapper-backed repository implementing all 17 SafetyPolicies category read-model repository
/// interfaces over the shared <c>[SafetyPolicies].[usp_SafetyPolicies_GetByCategory]</c> procedure.
/// </summary>
public sealed class SafetyPoliciesCategoryDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<SafetyPoliciesCategoryDapperRepository> logger) :
    IConfinedSpaceEntryRepository,
    IContractorSafetyRepository,
    IElectricalSafetyRepository,
    IEmergencyResponseEvacuationRepository,
    IEnvironmentalChemicalSafetyRepository,
    IEquipmentMachineOperationRepository,
    IHazardCommunicationAssessmentRepository,
    IHotWorkRepository,
    IHousekeepingErgonomicsRepository,
    IIncidentInvestigationCorrectiveActionRepository,
    IMachineMaintenanceInspectionRepository,
    IMaterialHandlingStorageRepository,
    IPersonalProtectiveEquipmentRepository,
    ISafetyCultureBehavioralRepository,
    ISafetyTrainingReportingRepository,
    ISecurityAccessControlRepository,
    IWorkingAtHeightsRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<ConfinedSpaceEntryItemDto>> GetConfinedSpaceEntryAsync(
        Guid tenantId, Guid? siteId, DateTime from, DateTime to, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetConfinedSpaceEntry for SafetyPolicies tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .AddString("Category", "ConfinedSpaceEntry", 100)
            .Build();

        var rows = await queryExecutor.QueryAsync<CategoryRow>(
            "[SafetyPolicies].[usp_SafetyPolicies_GetByCategory]", parameters, cancellationToken);

        return rows.Select(r => new ConfinedSpaceEntryItemDto
        {
            SafetyPolicyId = r.SafetyPolicyId,
            TenantId = r.TenantId,
            SiteId = r.SiteId,
            Title = r.Title,
            Category = r.Category,
            Status = r.Status,
            EffectiveAtUtc = r.EffectiveAtUtc,
            OwnerId = r.OwnerId,
        }).ToList();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ContractorSafetyItemDto>> GetContractorSafetyAsync(
        Guid tenantId, Guid? siteId, DateTime from, DateTime to, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetContractorSafety for SafetyPolicies tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .AddString("Category", "ContractorSafety", 100)
            .Build();

        var rows = await queryExecutor.QueryAsync<CategoryRow>(
            "[SafetyPolicies].[usp_SafetyPolicies_GetByCategory]", parameters, cancellationToken);

        return rows.Select(r => new ContractorSafetyItemDto
        {
            SafetyPolicyId = r.SafetyPolicyId,
            TenantId = r.TenantId,
            SiteId = r.SiteId,
            Title = r.Title,
            Category = r.Category,
            Status = r.Status,
            EffectiveAtUtc = r.EffectiveAtUtc,
            OwnerId = r.OwnerId,
        }).ToList();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ElectricalSafetyItemDto>> GetElectricalSafetyAsync(
        Guid tenantId, Guid? siteId, DateTime from, DateTime to, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetElectricalSafety for SafetyPolicies tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .AddString("Category", "ElectricalSafety", 100)
            .Build();

        var rows = await queryExecutor.QueryAsync<CategoryRow>(
            "[SafetyPolicies].[usp_SafetyPolicies_GetByCategory]", parameters, cancellationToken);

        return rows.Select(r => new ElectricalSafetyItemDto
        {
            SafetyPolicyId = r.SafetyPolicyId,
            TenantId = r.TenantId,
            SiteId = r.SiteId,
            Title = r.Title,
            Category = r.Category,
            Status = r.Status,
            EffectiveAtUtc = r.EffectiveAtUtc,
            OwnerId = r.OwnerId,
        }).ToList();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<EmergencyResponseEvacuationItemDto>> GetEmergencyResponseEvacuationAsync(
        Guid tenantId, Guid? siteId, DateTime from, DateTime to, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetEmergencyResponseEvacuation for SafetyPolicies tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .AddString("Category", "EmergencyResponseEvacuation", 100)
            .Build();

        var rows = await queryExecutor.QueryAsync<CategoryRow>(
            "[SafetyPolicies].[usp_SafetyPolicies_GetByCategory]", parameters, cancellationToken);

        return rows.Select(r => new EmergencyResponseEvacuationItemDto
        {
            SafetyPolicyId = r.SafetyPolicyId,
            TenantId = r.TenantId,
            SiteId = r.SiteId,
            Title = r.Title,
            Category = r.Category,
            Status = r.Status,
            EffectiveAtUtc = r.EffectiveAtUtc,
            OwnerId = r.OwnerId,
        }).ToList();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<EnvironmentalChemicalSafetyItemDto>> GetEnvironmentalChemicalSafetyAsync(
        Guid tenantId, Guid? siteId, DateTime from, DateTime to, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetEnvironmentalChemicalSafety for SafetyPolicies tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .AddString("Category", "EnvironmentalChemicalSafety", 100)
            .Build();

        var rows = await queryExecutor.QueryAsync<CategoryRow>(
            "[SafetyPolicies].[usp_SafetyPolicies_GetByCategory]", parameters, cancellationToken);

        return rows.Select(r => new EnvironmentalChemicalSafetyItemDto
        {
            SafetyPolicyId = r.SafetyPolicyId,
            TenantId = r.TenantId,
            SiteId = r.SiteId,
            Title = r.Title,
            Category = r.Category,
            Status = r.Status,
            EffectiveAtUtc = r.EffectiveAtUtc,
            OwnerId = r.OwnerId,
        }).ToList();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<EquipmentMachineOperationItemDto>> GetEquipmentMachineOperationAsync(
        Guid tenantId, Guid? siteId, DateTime from, DateTime to, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetEquipmentMachineOperation for SafetyPolicies tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .AddString("Category", "EquipmentMachineOperation", 100)
            .Build();

        var rows = await queryExecutor.QueryAsync<CategoryRow>(
            "[SafetyPolicies].[usp_SafetyPolicies_GetByCategory]", parameters, cancellationToken);

        return rows.Select(r => new EquipmentMachineOperationItemDto
        {
            SafetyPolicyId = r.SafetyPolicyId,
            TenantId = r.TenantId,
            SiteId = r.SiteId,
            Title = r.Title,
            Category = r.Category,
            Status = r.Status,
            EffectiveAtUtc = r.EffectiveAtUtc,
            OwnerId = r.OwnerId,
        }).ToList();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<HazardCommunicationAssessmentItemDto>> GetHazardCommunicationAssessmentAsync(
        Guid tenantId, Guid? siteId, DateTime from, DateTime to, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetHazardCommunicationAssessment for SafetyPolicies tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .AddString("Category", "HazardCommunicationAssessment", 100)
            .Build();

        var rows = await queryExecutor.QueryAsync<CategoryRow>(
            "[SafetyPolicies].[usp_SafetyPolicies_GetByCategory]", parameters, cancellationToken);

        return rows.Select(r => new HazardCommunicationAssessmentItemDto
        {
            SafetyPolicyId = r.SafetyPolicyId,
            TenantId = r.TenantId,
            SiteId = r.SiteId,
            Title = r.Title,
            Category = r.Category,
            Status = r.Status,
            EffectiveAtUtc = r.EffectiveAtUtc,
            OwnerId = r.OwnerId,
        }).ToList();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<HotWorkItemDto>> GetHotWorkAsync(
        Guid tenantId, Guid? siteId, DateTime from, DateTime to, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetHotWork for SafetyPolicies tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .AddString("Category", "HotWork", 100)
            .Build();

        var rows = await queryExecutor.QueryAsync<CategoryRow>(
            "[SafetyPolicies].[usp_SafetyPolicies_GetByCategory]", parameters, cancellationToken);

        return rows.Select(r => new HotWorkItemDto
        {
            SafetyPolicyId = r.SafetyPolicyId,
            TenantId = r.TenantId,
            SiteId = r.SiteId,
            Title = r.Title,
            Category = r.Category,
            Status = r.Status,
            EffectiveAtUtc = r.EffectiveAtUtc,
            OwnerId = r.OwnerId,
        }).ToList();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<HousekeepingErgonomicsItemDto>> GetHousekeepingErgonomicsAsync(
        Guid tenantId, Guid? siteId, DateTime from, DateTime to, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetHousekeepingErgonomics for SafetyPolicies tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .AddString("Category", "HousekeepingErgonomics", 100)
            .Build();

        var rows = await queryExecutor.QueryAsync<CategoryRow>(
            "[SafetyPolicies].[usp_SafetyPolicies_GetByCategory]", parameters, cancellationToken);

        return rows.Select(r => new HousekeepingErgonomicsItemDto
        {
            SafetyPolicyId = r.SafetyPolicyId,
            TenantId = r.TenantId,
            SiteId = r.SiteId,
            Title = r.Title,
            Category = r.Category,
            Status = r.Status,
            EffectiveAtUtc = r.EffectiveAtUtc,
            OwnerId = r.OwnerId,
        }).ToList();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<IncidentInvestigationCorrectiveActionItemDto>> GetIncidentInvestigationCorrectiveActionAsync(
        Guid tenantId, Guid? siteId, DateTime from, DateTime to, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetIncidentInvestigationCorrectiveAction for SafetyPolicies tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .AddString("Category", "IncidentInvestigationCorrectiveAction", 100)
            .Build();

        var rows = await queryExecutor.QueryAsync<CategoryRow>(
            "[SafetyPolicies].[usp_SafetyPolicies_GetByCategory]", parameters, cancellationToken);

        return rows.Select(r => new IncidentInvestigationCorrectiveActionItemDto
        {
            SafetyPolicyId = r.SafetyPolicyId,
            TenantId = r.TenantId,
            SiteId = r.SiteId,
            Title = r.Title,
            Category = r.Category,
            Status = r.Status,
            EffectiveAtUtc = r.EffectiveAtUtc,
            OwnerId = r.OwnerId,
        }).ToList();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<MachineMaintenanceInspectionItemDto>> GetMachineMaintenanceInspectionAsync(
        Guid tenantId, Guid? siteId, DateTime from, DateTime to, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetMachineMaintenanceInspection for SafetyPolicies tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .AddString("Category", "MachineMaintenanceInspection", 100)
            .Build();

        var rows = await queryExecutor.QueryAsync<CategoryRow>(
            "[SafetyPolicies].[usp_SafetyPolicies_GetByCategory]", parameters, cancellationToken);

        return rows.Select(r => new MachineMaintenanceInspectionItemDto
        {
            SafetyPolicyId = r.SafetyPolicyId,
            TenantId = r.TenantId,
            SiteId = r.SiteId,
            Title = r.Title,
            Category = r.Category,
            Status = r.Status,
            EffectiveAtUtc = r.EffectiveAtUtc,
            OwnerId = r.OwnerId,
        }).ToList();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<MaterialHandlingStorageItemDto>> GetMaterialHandlingStorageAsync(
        Guid tenantId, Guid? siteId, DateTime from, DateTime to, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetMaterialHandlingStorage for SafetyPolicies tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .AddString("Category", "MaterialHandlingStorage", 100)
            .Build();

        var rows = await queryExecutor.QueryAsync<CategoryRow>(
            "[SafetyPolicies].[usp_SafetyPolicies_GetByCategory]", parameters, cancellationToken);

        return rows.Select(r => new MaterialHandlingStorageItemDto
        {
            SafetyPolicyId = r.SafetyPolicyId,
            TenantId = r.TenantId,
            SiteId = r.SiteId,
            Title = r.Title,
            Category = r.Category,
            Status = r.Status,
            EffectiveAtUtc = r.EffectiveAtUtc,
            OwnerId = r.OwnerId,
        }).ToList();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<PersonalProtectiveEquipmentItemDto>> GetPersonalProtectiveEquipmentAsync(
        Guid tenantId, Guid? siteId, DateTime from, DateTime to, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetPersonalProtectiveEquipment for SafetyPolicies tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .AddString("Category", "PersonalProtectiveEquipment", 100)
            .Build();

        var rows = await queryExecutor.QueryAsync<CategoryRow>(
            "[SafetyPolicies].[usp_SafetyPolicies_GetByCategory]", parameters, cancellationToken);

        return rows.Select(r => new PersonalProtectiveEquipmentItemDto
        {
            SafetyPolicyId = r.SafetyPolicyId,
            TenantId = r.TenantId,
            SiteId = r.SiteId,
            Title = r.Title,
            Category = r.Category,
            Status = r.Status,
            EffectiveAtUtc = r.EffectiveAtUtc,
            OwnerId = r.OwnerId,
        }).ToList();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<SafetyCultureBehavioralItemDto>> GetSafetyCultureBehavioralAsync(
        Guid tenantId, Guid? siteId, DateTime from, DateTime to, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetSafetyCultureBehavioral for SafetyPolicies tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .AddString("Category", "SafetyCultureBehavioral", 100)
            .Build();

        var rows = await queryExecutor.QueryAsync<CategoryRow>(
            "[SafetyPolicies].[usp_SafetyPolicies_GetByCategory]", parameters, cancellationToken);

        return rows.Select(r => new SafetyCultureBehavioralItemDto
        {
            SafetyPolicyId = r.SafetyPolicyId,
            TenantId = r.TenantId,
            SiteId = r.SiteId,
            Title = r.Title,
            Category = r.Category,
            Status = r.Status,
            EffectiveAtUtc = r.EffectiveAtUtc,
            OwnerId = r.OwnerId,
        }).ToList();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<SafetyTrainingReportingItemDto>> GetSafetyTrainingReportingAsync(
        Guid tenantId, Guid? siteId, DateTime from, DateTime to, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetSafetyTrainingReporting for SafetyPolicies tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .AddString("Category", "SafetyTrainingReporting", 100)
            .Build();

        var rows = await queryExecutor.QueryAsync<CategoryRow>(
            "[SafetyPolicies].[usp_SafetyPolicies_GetByCategory]", parameters, cancellationToken);

        return rows.Select(r => new SafetyTrainingReportingItemDto
        {
            SafetyPolicyId = r.SafetyPolicyId,
            TenantId = r.TenantId,
            SiteId = r.SiteId,
            Title = r.Title,
            Category = r.Category,
            Status = r.Status,
            EffectiveAtUtc = r.EffectiveAtUtc,
            OwnerId = r.OwnerId,
        }).ToList();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<SecurityAccessControlItemDto>> GetSecurityAccessControlAsync(
        Guid tenantId, Guid? siteId, DateTime from, DateTime to, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetSecurityAccessControl for SafetyPolicies tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .AddString("Category", "SecurityAccessControl", 100)
            .Build();

        var rows = await queryExecutor.QueryAsync<CategoryRow>(
            "[SafetyPolicies].[usp_SafetyPolicies_GetByCategory]", parameters, cancellationToken);

        return rows.Select(r => new SecurityAccessControlItemDto
        {
            SafetyPolicyId = r.SafetyPolicyId,
            TenantId = r.TenantId,
            SiteId = r.SiteId,
            Title = r.Title,
            Category = r.Category,
            Status = r.Status,
            EffectiveAtUtc = r.EffectiveAtUtc,
            OwnerId = r.OwnerId,
        }).ToList();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<WorkingAtHeightsItemDto>> GetWorkingAtHeightsAsync(
        Guid tenantId, Guid? siteId, DateTime from, DateTime to, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetWorkingAtHeights for SafetyPolicies tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .AddString("Category", "WorkingAtHeights", 100)
            .Build();

        var rows = await queryExecutor.QueryAsync<CategoryRow>(
            "[SafetyPolicies].[usp_SafetyPolicies_GetByCategory]", parameters, cancellationToken);

        return rows.Select(r => new WorkingAtHeightsItemDto
        {
            SafetyPolicyId = r.SafetyPolicyId,
            TenantId = r.TenantId,
            SiteId = r.SiteId,
            Title = r.Title,
            Category = r.Category,
            Status = r.Status,
            EffectiveAtUtc = r.EffectiveAtUtc,
            OwnerId = r.OwnerId,
        }).ToList();
    }

    /// <summary>Flat row shape returned by <c>usp_SafetyPolicies_GetByCategory</c>.</summary>
    private sealed record CategoryRow
    {
        public required Guid SafetyPolicyId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string Title { get; init; }
        public required string Category { get; init; }
        public required string Status { get; init; }
        public required DateTime EffectiveAtUtc { get; init; }
        public required Guid OwnerId { get; init; }
    }
}
