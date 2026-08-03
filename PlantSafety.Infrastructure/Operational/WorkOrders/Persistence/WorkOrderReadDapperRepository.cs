// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.WorkOrders
//  Layer:          Infrastructure
//  File:           WorkOrderReadDapperRepository.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of the Application-layer IWorkOrderReadRepository
//                  (the module's full query surface: get-by-id, list, summary, machine status
//                  summary, tasks, parts usage, history, machine history, technician workload,
//                  calendar). Distinct from IWorkOrderRepository (the Domain write-side aggregate
//                  repository, implemented separately by WorkOrderDapperRepository). Every list-style
//                  query uses the single-result-set COUNT(*) OVER() window-function pattern (mirrors
//                  PermitDapperRepository) since DapperQueryExecutor exposes no multi-result-set
//                  method. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Operational.WorkOrders.Models;
using PlantSafety.Application.Operational.WorkOrders.Repositories;

namespace PlantSafety.Infrastructure.Operational.WorkOrders.Persistence;

/// <summary>Dapper-backed repository for the WorkOrder module's Application-layer query surface.</summary>
public sealed class WorkOrderReadDapperRepository(DapperQueryExecutor queryExecutor, ILogger<WorkOrderReadDapperRepository> logger)
    : IWorkOrderReadRepository
{
    /// <inheritdoc/>
    public async Task<WorkOrderItemDto?> GetByIdAsync(Guid tenantId, Guid workOrderId, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder()
            .AddGuid("WorkOrderId", workOrderId)
            .AddGuid("TenantId", tenantId)
            .Build();

        var row = await queryExecutor.QuerySingleOrDefaultAsync<WorkOrderItemRow>("[Operational].[usp_WorkOrder_GetById]", parameters, cancellationToken);
        if (row is null) return null;

        return new WorkOrderItemDto
        {
            WorkOrderId = row.WorkOrderId,
            TenantId = row.TenantId,
            WorkOrderCode = row.WorkOrderCode,
            MachineId = row.MachineId,
            MachineCode = row.MachineCode,
            TechnicianName = row.TechnicianName ?? string.Empty,
            Status = row.Status,
            Priority = row.Priority,
            CreatedAtUtc = AsUtcOffset(row.CreatedAtUtc),
            UpdatedAtUtc = AsUtcOffset(row.UpdatedAtUtc ?? row.CreatedAtUtc),
            DueAtUtc = AsUtcOffset(row.DueAtUtc),
            ETag = row.ETag ?? string.Empty,
        };
    }

    /// <inheritdoc/>
    public async Task<(IReadOnlyList<WorkOrderRowDto> Items, int TotalCount)> ListAsync(
        Guid tenantId, string? status, string? technician, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddString("Status", status, 50)
            .AddString("Technician", technician, 200)
            .AddInt("PageNumber", pageNumber)
            .AddInt("PageSize", pageSize)
            .Build();

        var rows = await queryExecutor.QueryAsync<WorkOrderListRow>("[Operational].[usp_WorkOrder_List]", parameters, cancellationToken);

        var items = rows.Select(r => new WorkOrderRowDto
        {
            WorkOrderId = r.WorkOrderId,
            WorkOrderCode = r.WorkOrderCode,
            MachineId = r.MachineId,
            MachineCode = r.MachineCode,
            TechnicianName = r.TechnicianName,
            Priority = r.Priority,
            Status = r.Status,
        }).ToList();

        return (items, rows.Count > 0 ? rows[0].TotalCount : 0);
    }

    /// <inheritdoc/>
    public async Task<WorkOrderSummaryDto> GetSummaryAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder().AddGuid("TenantId", tenantId).Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<WorkOrderSummaryRow>("[Operational].[usp_WorkOrder_Summary]", parameters, cancellationToken);

        return new WorkOrderSummaryDto
        {
            OpenCount = row?.OpenCount ?? 0,
            InProgressCount = row?.InProgressCount ?? 0,
            CompletedCount = row?.CompletedCount ?? 0,
        };
    }

    /// <inheritdoc/>
    public async Task<MachineStatusSummaryDto> GetMachineStatusSummaryAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder().AddGuid("TenantId", tenantId).Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<MachineStatusSummaryRow>("[Operational].[usp_WorkOrder_MachineStatusSummary]", parameters, cancellationToken);

        return new MachineStatusSummaryDto
        {
            HealthyCount = row?.HealthyCount ?? 0,
            DegradedCount = row?.DegradedCount ?? 0,
            OfflineCount = row?.OfflineCount ?? 0,
        };
    }

    /// <inheritdoc/>
    public async Task<(IReadOnlyList<TaskStepDto> Items, int TotalCount)> GetTasksAsync(Guid tenantId, Guid workOrderId, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder().AddGuid("TenantId", tenantId).AddGuid("WorkOrderId", workOrderId).Build();
        var rows = await queryExecutor.QueryAsync<TaskStepRow>("[Operational].[usp_WorkOrder_Tasks]", parameters, cancellationToken);

        var items = rows.Select(r => new TaskStepDto
        {
            StepNumber = r.StepNumber,
            Description = r.Description,
            DurationMinutes = r.DurationMinutes,
            Status = r.Status,
            TechnicianNotes = r.TechnicianNotes,
        }).ToList();

        return (items, rows.Count > 0 ? rows[0].TotalCount : 0);
    }

    /// <inheritdoc/>
    public async Task<(IReadOnlyList<PartUsageDto> Items, int TotalCount)> GetPartsUsageAsync(Guid tenantId, Guid workOrderId, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder().AddGuid("TenantId", tenantId).AddGuid("WorkOrderId", workOrderId).Build();
        var rows = await queryExecutor.QueryAsync<PartUsageRow>("[Operational].[usp_WorkOrder_PartsUsage]", parameters, cancellationToken);

        var items = rows.Select(r => new PartUsageDto
        {
            PartId = r.PartId,
            PartNumber = r.PartNumber,
            PartName = r.PartName,
            QuantityUsed = r.QuantityUsed,
            WasAvailable = r.WasAvailable,
        }).ToList();

        return (items, rows.Count > 0 ? rows[0].TotalCount : 0);
    }

    /// <inheritdoc/>
    public async Task<(IReadOnlyList<MaintenanceEventDto> Items, int TotalCount)> GetHistoryAsync(Guid tenantId, Guid workOrderId, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder().AddGuid("TenantId", tenantId).AddGuid("WorkOrderId", workOrderId).Build();
        var rows = await queryExecutor.QueryAsync<MaintenanceEventRow>("[Operational].[usp_WorkOrder_History]", parameters, cancellationToken);

        return (MapEvents(rows), rows.Count > 0 ? rows[0].TotalCount : 0);
    }

    /// <inheritdoc/>
    public async Task<(IReadOnlyList<MaintenanceEventDto> Items, int TotalCount)> GetMachineHistoryAsync(
        Guid tenantId, Guid machineId, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId).AddGuid("MachineId", machineId)
            .AddInt("PageNumber", pageNumber).AddInt("PageSize", pageSize)
            .Build();
        var rows = await queryExecutor.QueryAsync<MaintenanceEventRow>("[Operational].[usp_WorkOrder_MachineHistory]", parameters, cancellationToken);

        return (MapEvents(rows), rows.Count > 0 ? rows[0].TotalCount : 0);
    }

    /// <inheritdoc/>
    public async Task<(IReadOnlyList<TechnicianWorkloadDto> Items, int TotalCount)> GetTechnicianWorkloadAsync(
        Guid tenantId, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId).AddInt("PageNumber", pageNumber).AddInt("PageSize", pageSize)
            .Build();
        var rows = await queryExecutor.QueryAsync<TechnicianWorkloadRow>("[Operational].[usp_WorkOrder_TechnicianWorkload]", parameters, cancellationToken);

        var items = rows.Select(r => new TechnicianWorkloadDto
        {
            TechnicianId = r.TechnicianId,
            TechnicianName = r.TechnicianName ?? string.Empty,
            SkillTags = [],
            AssignedCount = r.AssignedCount,
            WorkloadPercent = r.WorkloadPercent,
            AvailabilityStatus = r.AvailabilityStatus,
        }).ToList();

        return (items, rows.Count > 0 ? rows[0].TotalCount : 0);
    }

    /// <inheritdoc/>
    public async Task<(IReadOnlyList<MaintenanceCalendarEventDto> Items, int TotalCount)> GetCalendarEventsAsync(
        Guid tenantId, DateTime rangeStartUtc, DateTime rangeEndUtc, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddDateTime("RangeStartUtc", rangeStartUtc)
            .AddDateTime("RangeEndUtc", rangeEndUtc)
            .Build();
        var rows = await queryExecutor.QueryAsync<MaintenanceCalendarEventRow>("[Operational].[usp_WorkOrder_Calendar]", parameters, cancellationToken);

        var items = rows.Select(r => new MaintenanceCalendarEventDto
        {
            EventId = r.EventId,
            WorkOrderCode = r.WorkOrderCode,
            EquipmentName = r.EquipmentName,
            TechnicianName = r.TechnicianName,
            StartUtc = AsUtcOffset(r.StartUtc),
            EndUtc = AsUtcOffset(r.EndUtc),
            EventType = r.EventType,
            Status = r.Status,
            Priority = r.Priority,
            HasConflict = false,
        }).ToList();

        return (items, items.Count);
    }

    private static List<MaintenanceEventDto> MapEvents(IReadOnlyList<MaintenanceEventRow> rows) =>
        rows.Select(r => new MaintenanceEventDto
        {
            EventId = r.EventId,
            OccurredAtUtc = AsUtcOffset(r.OccurredAtUtc),
            WorkOrderCode = r.WorkOrderCode,
            TechnicianName = r.TechnicianName,
            Summary = r.Summary,
            WorkOrderId = r.WorkOrderId,
        }).ToList();

    /// <summary>Converts a DB-sourced UTC <see cref="DateTime"/> (Kind=Unspecified) into a <see cref="DateTimeOffset"/>.</summary>
    private static DateTimeOffset AsUtcOffset(DateTime value) => new(DateTime.SpecifyKind(value, DateTimeKind.Utc));

    private sealed record WorkOrderItemRow
    {
        public string? ETag { get; init; }
        public required Guid WorkOrderId { get; init; }
        public required Guid TenantId { get; init; }
        public required string Status { get; init; }
        public required string WorkOrderCode { get; init; }
        public required Guid MachineId { get; init; }
        public required string MachineCode { get; init; }
        public string? TechnicianName { get; init; }
        public required string Priority { get; init; }
        public required DateTime DueAtUtc { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
        public DateTime? UpdatedAtUtc { get; init; }
    }

    private sealed record WorkOrderListRow
    {
        public required Guid WorkOrderId { get; init; }
        public required string WorkOrderCode { get; init; }
        public required Guid MachineId { get; init; }
        public required string MachineCode { get; init; }
        public required string TechnicianName { get; init; }
        public required string Priority { get; init; }
        public required string Status { get; init; }
        public required int TotalCount { get; init; }
    }

    private sealed record WorkOrderSummaryRow
    {
        public int OpenCount { get; init; }
        public int InProgressCount { get; init; }
        public int CompletedCount { get; init; }
    }

    private sealed record MachineStatusSummaryRow
    {
        public int HealthyCount { get; init; }
        public int DegradedCount { get; init; }
        public int OfflineCount { get; init; }
    }

    private sealed record TaskStepRow
    {
        public required int StepNumber { get; init; }
        public required string Description { get; init; }
        public required int DurationMinutes { get; init; }
        public required string Status { get; init; }
        public string? TechnicianNotes { get; init; }
        public required int TotalCount { get; init; }
    }

    private sealed record PartUsageRow
    {
        public required Guid PartId { get; init; }
        public required string PartNumber { get; init; }
        public required string PartName { get; init; }
        public required int QuantityUsed { get; init; }
        public required bool WasAvailable { get; init; }
        public required int TotalCount { get; init; }
    }

    private sealed record MaintenanceEventRow
    {
        public required Guid EventId { get; init; }
        public required DateTime OccurredAtUtc { get; init; }
        public required string WorkOrderCode { get; init; }
        public required string TechnicianName { get; init; }
        public required string Summary { get; init; }
        public required Guid WorkOrderId { get; init; }
        public required int TotalCount { get; init; }
    }

    private sealed record TechnicianWorkloadRow
    {
        public required Guid TechnicianId { get; init; }
        public string? TechnicianName { get; init; }
        public required int AssignedCount { get; init; }
        public required int WorkloadPercent { get; init; }
        public required string AvailabilityStatus { get; init; }
        public required int TotalCount { get; init; }
    }

    private sealed record MaintenanceCalendarEventRow
    {
        public required Guid EventId { get; init; }
        public string? WorkOrderCode { get; init; }
        public required string EquipmentName { get; init; }
        public string? TechnicianName { get; init; }
        public required DateTime StartUtc { get; init; }
        public required DateTime EndUtc { get; init; }
        public required string EventType { get; init; }
        public required string Status { get; init; }
        public required string Priority { get; init; }
    }
}
