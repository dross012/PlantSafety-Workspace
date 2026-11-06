// ============================================================================
//  Project:        Shared.Authorization.Infrastructure
//  Module:         Telemetry
//  File:           ContractTelemetryExtensions.cs
//  Created:        2026-08-10
//  Description:    Phase 12 deliverable. Extension methods that turn a
//                  ContractMetadata into a structured log payload with
//                  deterministic (alphabetical) key ordering, and record
//                  its emission through the existing ILogEventWriter --
//                  reused by UnifiedEventTelemetry and SchemaGovernanceEvaluator
//                  rather than each call site building its own Dictionary.
// ============================================================================
using Shared.Authorization.Abstractions.Contracts;
using Shared.Deterministic.Abstractions.Clock;
using Shared.Telemetry.Abstractions.Events;

namespace Shared.Authorization.Infrastructure.Telemetry;

/// <summary>Extension methods for emitting ContractMetadata through Shared.Telemetry's existing primitives.</summary>
public static class ContractTelemetryExtensions
{
    /// <summary>Converts <paramref name="metadata"/> into a structured log payload, keys in alphabetical order.</summary>
    public static IReadOnlyDictionary<string, object?> ToLogPayload(this ContractMetadata metadata)
    {
        ArgumentNullException.ThrowIfNull(metadata);

        return new Dictionary<string, object?>
        {
            ["CallerRole"] = metadata.CallerRole,
            ["CorrelationId"] = metadata.CorrelationId,
            ["MfaStatus"] = metadata.MfaStatus.ToString(),
            ["SchemaVersion"] = ContractMetadata.SchemaVersion,
            ["TenantId"] = metadata.TenantId,
            ["TimestampUtc"] = metadata.TimestampUtc,
        };
    }

    /// <summary>Writes a CONTRACT_METADATA_EMITTED log event carrying <paramref name="metadata"/>'s structured payload.</summary>
    public static void RecordEmitted(this ContractMetadata metadata, IClock clock, ILogEventWriter logEventWriter)
    {
        ArgumentNullException.ThrowIfNull(clock);
        ArgumentNullException.ThrowIfNull(logEventWriter);

        logEventWriter.Write(new LogEvent(
            EventId: "CONTRACT_METADATA_EMITTED",
            Category: LogCategory.Infrastructure,
            Severity: LogSeverity.Info,
            Message: $"ContractMetadata emitted for tenant \"{metadata.TenantId}\", correlation \"{metadata.CorrelationId}\".",
            Payload: metadata.ToLogPayload(),
            CorrelationId: metadata.CorrelationId,
            Module: "Contract",
            OccurredAtUtc: clock.UtcNow));
    }
}
