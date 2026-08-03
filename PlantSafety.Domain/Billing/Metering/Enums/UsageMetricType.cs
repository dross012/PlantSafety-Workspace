// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Billing.Metering
//  Layer:          Domain
//  File:           UsageMetricType.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 7 §3.1 Usage Metering)
//  SchemaVersion:  2026.1
//  Description:    Smart-enum-style value object for the six usage dimensions metered per tenant
//                  per billing period. Shared between Billing.Metering (recorded values) and
//                  Billing.Subscription (plan limits / overage rates keyed by the same metric).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Billing.Metering.Enums;

/// <summary>Smart-enum-style value object identifying a metered usage dimension.</summary>
public sealed class UsageMetricType : IEquatable<UsageMetricType>
{
    /// <summary>Count of API calls (HTTP requests) served for the tenant, sourced from PlatformHealthDashboard's RequestMetricEvent capture pipeline.</summary>
    public static readonly UsageMetricType ApiCalls = new("ApiCalls", "API Calls", isWholeUnit: true);

    /// <summary>Count of scheduled/optimization task runs executed for the tenant, sourced from PredictiveScheduling's OptimizationRun history.</summary>
    public static readonly UsageMetricType ScheduledTasks = new("ScheduledTasks", "Scheduled Tasks", isWholeUnit: true);

    /// <summary>Blob storage consumed by the tenant, in bytes, sourced from IBlobStorageService's tenant container.</summary>
    public static readonly UsageMetricType StorageBytes = new("StorageBytes", "Storage (Bytes)", isWholeUnit: false);

    /// <summary>Count of integration/domain events observed for the tenant (dead-lettered + successfully processed), sourced from the Outbox/DeadLetterEvents pipeline.</summary>
    public static readonly UsageMetricType EventVolume = new("EventVolume", "Event Volume", isWholeUnit: true);

    /// <summary>Count of active user accounts provisioned for the tenant, sourced from CrossDomain/EnterpriseFabric Users.</summary>
    public static readonly UsageMetricType UserCount = new("UserCount", "User Count", isWholeUnit: true);

    /// <summary>Count of distinct feature-flag-gated modules actively enabled for the tenant, sourced from the tenant's own TenantConfiguration.EnabledModules.</summary>
    public static readonly UsageMetricType ModuleUsage = new("ModuleUsage", "Module Usage", isWholeUnit: true);

    /// <summary>All defined UsageMetricType instances.</summary>
    public static readonly IReadOnlyList<UsageMetricType> All = [ApiCalls, ScheduledTasks, StorageBytes, EventVolume, UserCount, ModuleUsage];

    /// <summary>Stable machine-readable code.</summary>
    public string Code { get; }

    /// <summary>Human-readable display name.</summary>
    public string DisplayName { get; }

    /// <summary>When <see langword="true"/>, this metric is counted in whole units (e.g. calls, tasks); when <see langword="false"/>, it is a continuous quantity (e.g. bytes).</summary>
    public bool IsWholeUnit { get; }

    private UsageMetricType(string code, string displayName, bool isWholeUnit)
    {
        Code = code;
        DisplayName = displayName;
        IsWholeUnit = isWholeUnit;
    }

    /// <summary>Resolves a UsageMetricType instance from its <see cref="Code"/>. Throws if unknown.</summary>
    public static UsageMetricType From(string code)
        => TryParse(code, out var value) ? value : throw new ArgumentException($"Unknown UsageMetricType code '{code}'.", nameof(code));

    /// <summary>Attempts to resolve a UsageMetricType instance from its <see cref="Code"/>.</summary>
    public static bool TryParse(string? code, out UsageMetricType value)
    {
        value = All.FirstOrDefault(x => string.Equals(x.Code, code, StringComparison.OrdinalIgnoreCase))!;
        return value is not null;
    }

    /// <inheritdoc/>
    public bool Equals(UsageMetricType? other) => other is not null && Code == other.Code;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as UsageMetricType);

    /// <inheritdoc/>
    public override int GetHashCode() => Code.GetHashCode(StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public override string ToString() => Code;
}
