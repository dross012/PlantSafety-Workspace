// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.SharedKernel.Contracts.Enterprise
//  Layer:          Tests (Unit)
//  File:           CanonicalModelSampleData.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Deterministic sample instances of every enterprise canonical DTO version, shared
//                  by the snapshot and schema-diff test suites so both exercise the exact same
//                  fixed data. Isolated, deterministic -- no I/O, no network.
// ============================================================================
using PlantSafety.SharedKernel.Contracts.Enterprise.CanonicalModel;

namespace PlantSafety.Tests.SharedKernel.Contracts.Enterprise;

/// <summary>Deterministic sample instances of every canonical DTO version, for snapshot/schema-diff tests.</summary>
public static class CanonicalModelSampleData
{
    /// <summary>Frozen UTC instant shared by every sample instance below.</summary>
    public static readonly UtcTimestamp FixedTimestamp = UtcTimestamp.From(new DateTime(2026, 8, 3, 12, 0, 0, DateTimeKind.Utc));

    private static readonly Guid FixedGuidA = new("00000000-0000-0000-0000-0000000000a1");
    private static readonly Guid FixedGuidB = new("00000000-0000-0000-0000-0000000000b2");
    private static readonly Guid FixedGuidC = new("00000000-0000-0000-0000-0000000000c3");

    // ---- CanonicalAsset ---------------------------------------------------------

    public static CanonicalAssetV1 AssetV1() => new()
    {
        AssetId = FixedGuidA,
        TenantId = FixedGuidB,
        SiteId = FixedGuidC,
        AssetName = "Conveyor B",
        AssetType = "Conveyor",
        Status = "Operational",
        CreatedAtUtc = FixedTimestamp,
        UpdatedAtUtc = FixedTimestamp,
    };

    public static CanonicalAssetV2 AssetV2() => new()
    {
        AssetId = FixedGuidA,
        TenantId = FixedGuidB,
        SiteId = FixedGuidC,
        AssetName = "Conveyor B",
        AssetType = "Conveyor",
        Status = "Operational",
        CreatedAtUtc = FixedTimestamp,
        UpdatedAtUtc = FixedTimestamp,
        SerialNumber = "SN-12345",
        LastServicedAtUtc = FixedTimestamp,
        LastServicedBy = FixedGuidC,
    };

    public static CanonicalAssetV3 AssetV3() => new()
    {
        AssetId = FixedGuidA,
        TenantId = FixedGuidB,
        SiteId = FixedGuidC,
        AssetName = "Conveyor B",
        AssetType = "Conveyor",
        Status = "Operational",
        CreatedAtUtc = FixedTimestamp,
        UpdatedAtUtc = FixedTimestamp,
        SerialNumber = "SN-12345",
        LastServicedAtUtc = FixedTimestamp,
        LastServicedBy = FixedGuidC,
        PredictedFailureLikelihood = 0.42,
        ShouldRaiseFailureAlert = false,
        PredictionModelVersion = "2026.1-heuristic",
        PredictionGeneratedAtUtc = FixedTimestamp,
    };

    // ---- CanonicalWorkOrder ------------------------------------------------------

    public static CanonicalWorkOrderV1 WorkOrderV1() => new()
    {
        WorkOrderId = FixedGuidA,
        TenantId = FixedGuidB,
        SiteId = FixedGuidC,
        WorkOrderCode = "WO-001",
        AssetId = FixedGuidA,
        AssetCode = "M-001",
        Priority = "High",
        Description = "Replace bearing",
        Status = "Open",
        DueAtUtc = FixedTimestamp,
        CreatedAtUtc = FixedTimestamp,
        UpdatedAtUtc = FixedTimestamp,
    };

    public static CanonicalWorkOrderV2 WorkOrderV2() => new()
    {
        WorkOrderId = FixedGuidA,
        TenantId = FixedGuidB,
        SiteId = FixedGuidC,
        WorkOrderCode = "WO-001",
        AssetId = FixedGuidA,
        AssetCode = "M-001",
        Priority = "High",
        Description = "Replace bearing",
        Status = "Assigned",
        DueAtUtc = FixedTimestamp,
        CreatedAtUtc = FixedTimestamp,
        UpdatedAtUtc = FixedTimestamp,
        TechnicianId = FixedGuidB,
        TechnicianName = "J. Rivera",
        AssignedAtUtc = FixedTimestamp,
    };

    public static CanonicalWorkOrderV3 WorkOrderV3() => new()
    {
        WorkOrderId = FixedGuidA,
        TenantId = FixedGuidB,
        SiteId = FixedGuidC,
        WorkOrderCode = "WO-001",
        AssetId = FixedGuidA,
        AssetCode = "M-001",
        Priority = "High",
        Description = "Replace bearing",
        Status = "Assigned",
        DueAtUtc = FixedTimestamp,
        CreatedAtUtc = FixedTimestamp,
        UpdatedAtUtc = FixedTimestamp,
        TechnicianId = FixedGuidB,
        TechnicianName = "J. Rivera",
        AssignedAtUtc = FixedTimestamp,
        PredictedCompletionAtUtc = FixedTimestamp,
        PredictedRiskScore = 0.35,
        PredictionModelVersion = "2026.1-heuristic",
        PredictionGeneratedAtUtc = FixedTimestamp,
    };

    // ---- CanonicalProductionOrder -------------------------------------------------

    public static CanonicalProductionOrderV1 ProductionOrderV1() => new()
    {
        ProductionOrderId = FixedGuidA,
        TenantId = FixedGuidB,
        SiteId = FixedGuidC,
        OrderCode = "PR-001",
        ProductionLineId = FixedGuidB,
        ProductionLineName = "Line 1",
        AssetId = FixedGuidA,
        Status = "Running",
        StartedAtUtc = FixedTimestamp,
        CreatedAtUtc = FixedTimestamp,
        UpdatedAtUtc = FixedTimestamp,
    };

    public static CanonicalProductionOrderV2 ProductionOrderV2() => new()
    {
        ProductionOrderId = FixedGuidA,
        TenantId = FixedGuidB,
        SiteId = FixedGuidC,
        OrderCode = "PR-001",
        ProductionLineId = FixedGuidB,
        ProductionLineName = "Line 1",
        AssetId = FixedGuidA,
        Status = "Running",
        StartedAtUtc = FixedTimestamp,
        CreatedAtUtc = FixedTimestamp,
        UpdatedAtUtc = FixedTimestamp,
        OperatorName = "A. Chen",
        IdealCycleTimeSeconds = 12.5,
        GoodUnitCount = 480,
        ScrapUnitCount = 6,
        DowntimeMinutesTotal = 14.0,
    };

    public static CanonicalProductionOrderV3 ProductionOrderV3() => new()
    {
        ProductionOrderId = FixedGuidA,
        TenantId = FixedGuidB,
        SiteId = FixedGuidC,
        OrderCode = "PR-001",
        ProductionLineId = FixedGuidB,
        ProductionLineName = "Line 1",
        AssetId = FixedGuidA,
        Status = "Running",
        StartedAtUtc = FixedTimestamp,
        CreatedAtUtc = FixedTimestamp,
        UpdatedAtUtc = FixedTimestamp,
        OperatorName = "A. Chen",
        IdealCycleTimeSeconds = 12.5,
        GoodUnitCount = 480,
        ScrapUnitCount = 6,
        DowntimeMinutesTotal = 14.0,
        PredictedCompletionAtUtc = FixedTimestamp,
        PredictedCompletionConfidence = 0.6,
        PredictionModelVersion = "2026.1-heuristic",
        PredictionGeneratedAtUtc = FixedTimestamp,
    };

    // ---- CanonicalRiskEvent -------------------------------------------------------

    public static CanonicalRiskEventV1 RiskEventV1() => new()
    {
        RiskEventId = FixedGuidA,
        TenantId = FixedGuidB,
        SiteId = FixedGuidC,
        RiskTitle = "Pinch point on Conveyor B",
        Category = "High",
        Location = "Building 2, Zone A",
        HazardSource = "Mechanical",
        Status = "Identified",
        CreatedAtUtc = FixedTimestamp,
    };

    public static CanonicalRiskEventV2 RiskEventV2() => new()
    {
        RiskEventId = FixedGuidA,
        TenantId = FixedGuidB,
        SiteId = FixedGuidC,
        RiskTitle = "Pinch point on Conveyor B",
        Category = "High",
        Location = "Building 2, Zone A",
        HazardSource = "Mechanical",
        Status = "Scored",
        CreatedAtUtc = FixedTimestamp,
        Priority = "P2",
        CurrentScoreSeverity = 4,
        CurrentScoreLikelihood = 3,
        ControlMeasureCount = 1,
        ReviewCadenceCode = "Monthly",
    };

    public static CanonicalRiskEventV3 RiskEventV3() => new()
    {
        RiskEventId = FixedGuidA,
        TenantId = FixedGuidB,
        SiteId = FixedGuidC,
        RiskTitle = "Pinch point on Conveyor B",
        Category = "High",
        Location = "Building 2, Zone A",
        HazardSource = "Mechanical",
        Status = "Scored",
        CreatedAtUtc = FixedTimestamp,
        Priority = "P2",
        CurrentScoreSeverity = 4,
        CurrentScoreLikelihood = 3,
        ControlMeasureCount = 1,
        ReviewCadenceCode = "Monthly",
        NextReviewDueUtc = new DateTime(2026, 9, 3, 12, 0, 0, DateTimeKind.Utc),
        IsReviewOverdue = false,
    };

    // ---- CanonicalImprovementAction -----------------------------------------------

    public static CanonicalImprovementActionV1 ImprovementActionV1() => new()
    {
        ImprovementActionId = FixedGuidA,
        TenantId = FixedGuidB,
        SourceEventId = FixedGuidC,
        SourceEventType = "Incident",
        Title = "Install machine guard",
        Description = "Install a fixed guard on Conveyor B pinch point.",
        ActionType = "Corrective",
        Priority = "High",
        Status = "Open",
        AssignedToWorkerId = FixedGuidB,
        DueAtUtc = FixedTimestamp,
        CreatedAtUtc = FixedTimestamp,
    };

    public static CanonicalImprovementActionV2 ImprovementActionV2() => new()
    {
        ImprovementActionId = FixedGuidA,
        TenantId = FixedGuidB,
        SourceEventId = FixedGuidC,
        SourceEventType = "Incident",
        Title = "Install machine guard",
        Description = "Install a fixed guard on Conveyor B pinch point.",
        ActionType = "Corrective",
        Priority = "High",
        Status = "InProgress",
        AssignedToWorkerId = FixedGuidB,
        DueAtUtc = FixedTimestamp,
        CreatedAtUtc = FixedTimestamp,
    };

    public static CanonicalImprovementActionV3 ImprovementActionV3() => new()
    {
        ImprovementActionId = FixedGuidA,
        TenantId = FixedGuidB,
        SourceEventId = FixedGuidC,
        SourceEventType = "Incident",
        Title = "Install machine guard",
        Description = "Install a fixed guard on Conveyor B pinch point.",
        ActionType = "Corrective",
        Priority = "High",
        Status = "InProgress",
        AssignedToWorkerId = FixedGuidB,
        DueAtUtc = FixedTimestamp,
        CreatedAtUtc = FixedTimestamp,
        IsPredictedOverdue = false,
    };

    // ---- CanonicalSchedulingBlock --------------------------------------------------

    public static CanonicalSchedulingBlockV1 SchedulingBlockV1() => new()
    {
        SchedulingBlockId = FixedGuidA,
        TenantId = FixedGuidB,
        SiteId = FixedGuidC,
        OptimizationRunId = FixedGuidB,
        TaskId = FixedGuidC,
        ResourceId = FixedGuidA,
        Status = "Completed",
        StartUtc = FixedTimestamp,
        EndUtc = FixedTimestamp,
        CreatedAtUtc = FixedTimestamp,
    };

    public static CanonicalSchedulingBlockV2 SchedulingBlockV2() => new()
    {
        SchedulingBlockId = FixedGuidA,
        TenantId = FixedGuidB,
        SiteId = FixedGuidC,
        OptimizationRunId = FixedGuidB,
        TaskId = FixedGuidC,
        ResourceId = FixedGuidA,
        Status = "Completed",
        StartUtc = FixedTimestamp,
        EndUtc = FixedTimestamp,
        CreatedAtUtc = FixedTimestamp,
        EffectivePriorityWeight = 0.75,
        Objective = "MinimizeDowntime",
        SelectedStrategyName = "GeneticAlgorithm",
        RunScore = 0.88,
    };

    public static CanonicalSchedulingBlockV3 SchedulingBlockV3() => new()
    {
        SchedulingBlockId = FixedGuidA,
        TenantId = FixedGuidB,
        SiteId = FixedGuidC,
        OptimizationRunId = FixedGuidB,
        TaskId = FixedGuidC,
        ResourceId = FixedGuidA,
        Status = "Completed",
        StartUtc = FixedTimestamp,
        EndUtc = FixedTimestamp,
        CreatedAtUtc = FixedTimestamp,
        EffectivePriorityWeight = 0.75,
        Objective = "MinimizeDowntime",
        SelectedStrategyName = "GeneticAlgorithm",
        RunScore = 0.88,
        PredictedConflictLikelihood = 0.15,
        PredictedDuration = TimeSpan.FromMinutes(90),
        RecommendedWindowScore = 0.91,
        PredictionModelVersion = "2026.1-heuristic",
        PredictionGeneratedAtUtc = FixedTimestamp,
    };
}
