// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         RiskManagement
//  Layer:          Infrastructure
//  File:           RiskManagementCategoryDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Single Dapper repository implementing all 11 RiskManagement category
//                  read-model Application-layer repository interfaces. Each method calls the
//                  single shared [RiskMgmt].[usp_Risks_GetByCategory] stored procedure with its
//                  own literal @Category value (matching the Infrastructure-layer
//                  HazardDomainCategory classification tag -- see RiskDapperRepository remarks),
//                  avoiding 11 near-duplicate query shapes.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.RiskManagement.ChemicalEnvironmentalSafety.Models;
using PlantSafety.Application.RiskManagement.ChemicalEnvironmentalSafety.Repositories;
using PlantSafety.Application.RiskManagement.ContinuousMonitoringReview.Models;
using PlantSafety.Application.RiskManagement.ContinuousMonitoringReview.Repositories;
using PlantSafety.Application.RiskManagement.ControlMeasuresHierarchy.Models;
using PlantSafety.Application.RiskManagement.ControlMeasuresHierarchy.Repositories;
using PlantSafety.Application.RiskManagement.EmergencyPreparedness.Models;
using PlantSafety.Application.RiskManagement.EmergencyPreparedness.Repositories;
using PlantSafety.Application.RiskManagement.ErgonomicsMaterialHandling.Models;
using PlantSafety.Application.RiskManagement.ErgonomicsMaterialHandling.Repositories;
using PlantSafety.Application.RiskManagement.HazardIdentificationMeasurement.Models;
using PlantSafety.Application.RiskManagement.HazardIdentificationMeasurement.Repositories;
using PlantSafety.Application.RiskManagement.LotoConfinedSpaces.Models;
using PlantSafety.Application.RiskManagement.LotoConfinedSpaces.Repositories;
using PlantSafety.Application.RiskManagement.MachineOperationalGuarding.Models;
using PlantSafety.Application.RiskManagement.MachineOperationalGuarding.Repositories;
using PlantSafety.Application.RiskManagement.PreStartupSafetyReview.Models;
using PlantSafety.Application.RiskManagement.PreStartupSafetyReview.Repositories;
using PlantSafety.Application.RiskManagement.ProcessSafetyInformation.Models;
using PlantSafety.Application.RiskManagement.ProcessSafetyInformation.Repositories;
using PlantSafety.Application.RiskManagement.TrainingContractorManagement.Models;
using PlantSafety.Application.RiskManagement.TrainingContractorManagement.Repositories;

namespace PlantSafety.Infrastructure.RiskManagement.Persistence;

/// <summary>
/// Dapper-backed repository implementing all 11 RiskManagement category read-model repository
/// interfaces over the shared <c>[RiskMgmt].[usp_Risks_GetByCategory]</c> procedure.
/// </summary>
public sealed class RiskManagementCategoryDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<RiskManagementCategoryDapperRepository> logger) :
    IChemicalEnvironmentalSafetyRepository,
    IContinuousMonitoringReviewRepository,
    IControlMeasuresHierarchyRepository,
    IEmergencyPreparednessRepository,
    IErgonomicsMaterialHandlingRepository,
    IHazardIdentificationMeasurementRepository,
    ILotoConfinedSpacesRepository,
    IMachineOperationalGuardingRepository,
    IPreStartupSafetyReviewRepository,
    IProcessSafetyInformationRepository,
    ITrainingContractorManagementRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<ChemicalEnvironmentalSafetyItemDto>> GetChemicalEnvironmentalSafetyAsync(
        Guid tenantId, Guid? siteId, DateTime from, DateTime to, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetChemicalEnvironmentalSafety for RiskManagement tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .AddString("Category", "ChemicalEnvironmentalSafety", 100)
            .Build();

        var rows = await queryExecutor.QueryAsync<CategoryRow>(
            "[RiskMgmt].[usp_Risks_GetByCategory]", parameters, cancellationToken);

        return rows.Select(r => new ChemicalEnvironmentalSafetyItemDto
        {
            RiskId = r.RiskId,
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
    public async Task<IReadOnlyList<ContinuousMonitoringReviewItemDto>> GetContinuousMonitoringReviewAsync(
        Guid tenantId, Guid? siteId, DateTime from, DateTime to, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetContinuousMonitoringReview for RiskManagement tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .AddString("Category", "ContinuousMonitoringReview", 100)
            .Build();

        var rows = await queryExecutor.QueryAsync<CategoryRow>(
            "[RiskMgmt].[usp_Risks_GetByCategory]", parameters, cancellationToken);

        return rows.Select(r => new ContinuousMonitoringReviewItemDto
        {
            RiskId = r.RiskId,
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
    public async Task<IReadOnlyList<ControlMeasuresHierarchyItemDto>> GetControlMeasuresHierarchyAsync(
        Guid tenantId, Guid? siteId, DateTime from, DateTime to, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetControlMeasuresHierarchy for RiskManagement tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .AddString("Category", "ControlMeasuresHierarchy", 100)
            .Build();

        var rows = await queryExecutor.QueryAsync<CategoryRow>(
            "[RiskMgmt].[usp_Risks_GetByCategory]", parameters, cancellationToken);

        return rows.Select(r => new ControlMeasuresHierarchyItemDto
        {
            RiskId = r.RiskId,
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
    public async Task<IReadOnlyList<EmergencyPreparednessItemDto>> GetEmergencyPreparednessAsync(
        Guid tenantId, Guid? siteId, DateTime from, DateTime to, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetEmergencyPreparedness for RiskManagement tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .AddString("Category", "EmergencyPreparedness", 100)
            .Build();

        var rows = await queryExecutor.QueryAsync<CategoryRow>(
            "[RiskMgmt].[usp_Risks_GetByCategory]", parameters, cancellationToken);

        return rows.Select(r => new EmergencyPreparednessItemDto
        {
            RiskId = r.RiskId,
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
    public async Task<IReadOnlyList<ErgonomicsMaterialHandlingItemDto>> GetErgonomicsMaterialHandlingAsync(
        Guid tenantId, Guid? siteId, DateTime from, DateTime to, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetErgonomicsMaterialHandling for RiskManagement tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .AddString("Category", "ErgonomicsMaterialHandling", 100)
            .Build();

        var rows = await queryExecutor.QueryAsync<CategoryRow>(
            "[RiskMgmt].[usp_Risks_GetByCategory]", parameters, cancellationToken);

        return rows.Select(r => new ErgonomicsMaterialHandlingItemDto
        {
            RiskId = r.RiskId,
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
    public async Task<IReadOnlyList<HazardIdentificationMeasurementItemDto>> GetHazardIdentificationMeasurementAsync(
        Guid tenantId, Guid? siteId, DateTime from, DateTime to, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetHazardIdentificationMeasurement for RiskManagement tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .AddString("Category", "HazardIdentificationMeasurement", 100)
            .Build();

        var rows = await queryExecutor.QueryAsync<CategoryRow>(
            "[RiskMgmt].[usp_Risks_GetByCategory]", parameters, cancellationToken);

        return rows.Select(r => new HazardIdentificationMeasurementItemDto
        {
            RiskId = r.RiskId,
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
    public async Task<IReadOnlyList<LotoConfinedSpacesItemDto>> GetLotoConfinedSpacesAsync(
        Guid tenantId, Guid? siteId, DateTime from, DateTime to, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetLotoConfinedSpaces for RiskManagement tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .AddString("Category", "LotoConfinedSpaces", 100)
            .Build();

        var rows = await queryExecutor.QueryAsync<CategoryRow>(
            "[RiskMgmt].[usp_Risks_GetByCategory]", parameters, cancellationToken);

        return rows.Select(r => new LotoConfinedSpacesItemDto
        {
            RiskId = r.RiskId,
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
    public async Task<IReadOnlyList<MachineOperationalGuardingItemDto>> GetMachineOperationalGuardingAsync(
        Guid tenantId, Guid? siteId, DateTime from, DateTime to, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetMachineOperationalGuarding for RiskManagement tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .AddString("Category", "MachineOperationalGuarding", 100)
            .Build();

        var rows = await queryExecutor.QueryAsync<CategoryRow>(
            "[RiskMgmt].[usp_Risks_GetByCategory]", parameters, cancellationToken);

        return rows.Select(r => new MachineOperationalGuardingItemDto
        {
            RiskId = r.RiskId,
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
    public async Task<IReadOnlyList<PreStartupSafetyReviewItemDto>> GetPreStartupSafetyReviewAsync(
        Guid tenantId, Guid? siteId, DateTime from, DateTime to, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetPreStartupSafetyReview for RiskManagement tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .AddString("Category", "PreStartupSafetyReview", 100)
            .Build();

        var rows = await queryExecutor.QueryAsync<CategoryRow>(
            "[RiskMgmt].[usp_Risks_GetByCategory]", parameters, cancellationToken);

        return rows.Select(r => new PreStartupSafetyReviewItemDto
        {
            RiskId = r.RiskId,
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
    public async Task<IReadOnlyList<ProcessSafetyInformationItemDto>> GetProcessSafetyInformationAsync(
        Guid tenantId, Guid? siteId, DateTime from, DateTime to, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetProcessSafetyInformation for RiskManagement tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .AddString("Category", "ProcessSafetyInformation", 100)
            .Build();

        var rows = await queryExecutor.QueryAsync<CategoryRow>(
            "[RiskMgmt].[usp_Risks_GetByCategory]", parameters, cancellationToken);

        return rows.Select(r => new ProcessSafetyInformationItemDto
        {
            RiskId = r.RiskId,
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
    public async Task<IReadOnlyList<TrainingContractorManagementItemDto>> GetTrainingContractorManagementAsync(
        Guid tenantId, Guid? siteId, DateTime from, DateTime to, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetTrainingContractorManagement for RiskManagement tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .AddString("Category", "TrainingContractorManagement", 100)
            .Build();

        var rows = await queryExecutor.QueryAsync<CategoryRow>(
            "[RiskMgmt].[usp_Risks_GetByCategory]", parameters, cancellationToken);

        return rows.Select(r => new TrainingContractorManagementItemDto
        {
            RiskId = r.RiskId,
            TenantId = r.TenantId,
            SiteId = r.SiteId,
            Title = r.Title,
            Category = r.Category,
            Status = r.Status,
            EffectiveAtUtc = r.EffectiveAtUtc,
            OwnerId = r.OwnerId,
        }).ToList();
    }

    /// <summary>Flat row shape returned by <c>usp_Risks_GetByCategory</c>.</summary>
    private sealed record CategoryRow
    {
        public required Guid RiskId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string Title { get; init; }
        public required string Category { get; init; }
        public required string Status { get; init; }
        public required DateTime EffectiveAtUtc { get; init; }
        public required Guid OwnerId { get; init; }
    }
}
