// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CrossDomain.Machines
//  Layer:          Application
//  File:           MachineToCanonicalAssetMapper.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Maps the real Machines read model (MachinesItemDto -- the platform's actual
//                  equipment/machine identity source; there is no separate rich Machine domain
//                  aggregate) to the enterprise canonical model
//                  (PlantSafety.SharedKernel.Contracts.Enterprise.CanonicalModel.CanonicalAssetV1
//                  /V2/V3) for enterprise integration consumers (ERP/MES/CMMS adapters, event
//                  mesh). Lives in this module's Application layer (not SharedKernel) because
//                  SharedKernel must never reference module Domain/Application types. Additive-only
//                  evolution.
// ============================================================================
using PlantSafety.Application.CrossDomain.Machines.Models;
using PlantSafety.SharedKernel.Contracts.Enterprise.CanonicalModel;
using PlantSafety.SharedKernel.Primitives.ValueTypes;

namespace PlantSafety.Application.CrossDomain.Machines.Mappings;

/// <summary>Maps a real <see cref="MachinesItemDto"/> record to the enterprise canonical asset shapes.</summary>
public static class MachineToCanonicalAssetMapper
{
    /// <summary>
    /// Maps <paramref name="machine"/> to its V1 (stable) canonical projection. Documented
    /// assumption: <see cref="MachinesItemDto"/> (the real Machines read model) tracks no distinct
    /// record-creation timestamp, only <see cref="MachinesItemDto.LastServicedAtUtc"/> -- both
    /// <see cref="CanonicalAssetV1.CreatedAtUtc"/> and <see cref="CanonicalAssetV1.UpdatedAtUtc"/>
    /// are populated from it rather than fabricating a separate value.
    /// </summary>
    public static CanonicalAssetV1 ToCanonicalV1(this MachinesItemDto machine)
    {
        ArgumentNullException.ThrowIfNull(machine);

        return new CanonicalAssetV1
        {
            AssetId = machine.MachineId,
            TenantId = machine.TenantId,
            SiteId = machine.SiteId,
            AssetName = machine.MachineName,
            AssetType = machine.MachineType,
            Status = machine.Status,
            CreatedAtUtc = UtcTimestamp.From(machine.LastServicedAtUtc),
            UpdatedAtUtc = UtcTimestamp.From(machine.LastServicedAtUtc),
        };
    }

    /// <summary>Maps <paramref name="machine"/> to its V2 (extended) canonical projection.</summary>
    public static CanonicalAssetV2 ToCanonicalV2(this MachinesItemDto machine)
    {
        ArgumentNullException.ThrowIfNull(machine);

        return new CanonicalAssetV2
        {
            AssetId = machine.MachineId,
            TenantId = machine.TenantId,
            SiteId = machine.SiteId,
            AssetName = machine.MachineName,
            AssetType = machine.MachineType,
            Status = machine.Status,
            CreatedAtUtc = UtcTimestamp.From(machine.LastServicedAtUtc),
            UpdatedAtUtc = UtcTimestamp.From(machine.LastServicedAtUtc),
            SerialNumber = machine.SerialNumber,
            LastServicedAtUtc = UtcTimestamp.From(machine.LastServicedAtUtc),
            LastServicedBy = machine.LastServicedBy,
        };
    }

    /// <summary>
    /// Maps <paramref name="machine"/> to its V3 (predictive) canonical projection. Predictive
    /// fields are populated only when a genuine
    /// <see cref="PlantSafety.Domain.Intelligence.PredictiveSafety.Predictors.FailureLikelihoodPredictor"/>
    /// result exists for this asset; callers with no genuine prediction should pass nulls rather
    /// than fabricate values.
    /// </summary>
    public static CanonicalAssetV3 ToCanonicalV3(
        this MachinesItemDto machine,
        double? predictedFailureLikelihood = null,
        bool? shouldRaiseFailureAlert = null,
        string? predictionModelVersion = null,
        UtcTimestamp? predictionGeneratedAtUtc = null)
    {
        ArgumentNullException.ThrowIfNull(machine);

        return new CanonicalAssetV3
        {
            AssetId = machine.MachineId,
            TenantId = machine.TenantId,
            SiteId = machine.SiteId,
            AssetName = machine.MachineName,
            AssetType = machine.MachineType,
            Status = machine.Status,
            CreatedAtUtc = UtcTimestamp.From(machine.LastServicedAtUtc),
            UpdatedAtUtc = UtcTimestamp.From(machine.LastServicedAtUtc),
            SerialNumber = machine.SerialNumber,
            LastServicedAtUtc = UtcTimestamp.From(machine.LastServicedAtUtc),
            LastServicedBy = machine.LastServicedBy,
            PredictedFailureLikelihood = predictedFailureLikelihood,
            ShouldRaiseFailureAlert = shouldRaiseFailureAlert,
            PredictionModelVersion = predictionModelVersion,
            PredictionGeneratedAtUtc = predictionGeneratedAtUtc,
        };
    }
}
