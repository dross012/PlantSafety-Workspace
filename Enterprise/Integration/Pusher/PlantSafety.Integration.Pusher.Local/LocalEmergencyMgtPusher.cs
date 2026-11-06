// ============================================================================
//  Project:        PlantSafety.Integration.Pusher.Local
//  File:           LocalEmergencyMgtPusher.cs
//  Description:    Direct, synchronous pusher: reads EmergencyMgt's EmergencyManagementDB
//                  directly and calls EmergencyMgtIntegration's own usp_*_Upsert/usp_*_Record
//                  procedures on PlantSafety.CoreDB. No message bus, no event envelope, no
//                  routing-key dispatcher -- see IIntegrationPusher's own header for why.
//                  Severity/Status TINYINT-code-to-label mapping duplicates
//                  EmergencyManagement.fnEmergencySeverity/fnEmergencyStatus's own logic
//                  locally (cross-database function calls are not possible without a linked
//                  server, which this architecture forbids -- see 00-create-schema.sql),
//                  mirroring the same "duplicate small deterministic logic across a boundary"
//                  precedent the OLS forecasting engine itself already established (EmergencyMgt
//                  docs/deviations.md DEV-009).
// ============================================================================
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using PlantSafety.Integration.Pusher.Abstractions;

namespace PlantSafety.Integration.Pusher.Local;

/// <summary>Pushes EmergencyMgt's EmergencyEvent/CorrectiveAction/ReadinessScore/VersionManifest data into PlantSafety's EmergencyMgtIntegration schema.</summary>
public sealed class LocalEmergencyMgtPusher(
    string emergencyMgtConnectionString,
    string plantSafetyConnectionString,
    ILogger<LocalEmergencyMgtPusher> logger) : IIntegrationPusher
{
    private static readonly Dictionary<byte, string> SeverityLabels = new()
    {
        [1] = "Low", [2] = "Medium", [3] = "High", [4] = "Critical",
    };

    private static readonly Dictionary<byte, string> StatusLabels = new()
    {
        [1] = "Declared", [2] = "ActiveResponse", [3] = "Stabilizing", [4] = "Resolved", [5] = "Closed", [6] = "Archived",
    };

    /// <inheritdoc/>
    public async Task<IIntegrationPushResult> PushAsync(CancellationToken cancellationToken)
    {
        var startedUtc = DateTime.UtcNow;
        var details = new Dictionary<string, string>();
        int succeeded = 0, failed = 0;

        await using var source = new SqlConnection(emergencyMgtConnectionString);
        await using var target = new SqlConnection(plantSafetyConnectionString);
        await source.OpenAsync(cancellationToken);
        await target.OpenAsync(cancellationToken);

        (succeeded, failed) = await PushSummaryAndLifecycleAsync(source, target, succeeded, failed, details, cancellationToken);
        (succeeded, failed) = await PushCorrectiveActionsAsync(source, target, succeeded, failed, details, cancellationToken);
        (succeeded, failed) = await PushReadinessAsync(source, target, succeeded, failed, details, cancellationToken);
        (succeeded, failed) = await PushVersionManifestSignalAsync(source, target, succeeded, failed, details, cancellationToken);

        var completedUtc = DateTime.UtcNow;
        var status = failed == 0
            ? IntegrationPushStatus.Success
            : succeeded == 0 ? IntegrationPushStatus.Failed : IntegrationPushStatus.PartialFailure;

        logger.LogInformation("Push complete: {Status}, {Succeeded} succeeded, {Failed} failed, {Elapsed}ms", status, succeeded, failed, (completedUtc - startedUtc).TotalMilliseconds);

        return new IntegrationPushResult
        {
            Status = status,
            SucceededCount = succeeded,
            FailedCount = failed,
            FeedDetails = details,
            StartedUtc = startedUtc,
            CompletedUtc = completedUtc,
        };
    }

    private async Task<(int succeeded, int failed)> PushSummaryAndLifecycleAsync(
        SqlConnection source, SqlConnection target, int succeeded, int failed, Dictionary<string, string> details, CancellationToken ct)
    {
        try
        {
            var rows = await source.QueryAsync(new CommandDefinition(
                @"SELECT e.EmergencyEventId, t.Name AS EmergencyTypeName, e.Severity, e.Location, e.InitiatedBy,
                         e.StartedUtc, e.EndedUtc, e.Status, e.ModifiedUtc, e.CreatedUtc
                  FROM EmergencyManagement.EmergencyEvent e
                  JOIN EmergencyManagement.EmergencyType t ON t.EmergencyTypeId = e.EmergencyTypeId
                  WHERE e.IsDeleted = 0",
                cancellationToken: ct));

            int pushed = 0;
            foreach (var row in rows)
            {
                byte severity = (byte)row.Severity;
                byte status = (byte)row.Status;
                bool isTerminal = status is 5 or 6;

                await target.ExecuteAsync(new CommandDefinition(
                    "[EmergencyMgtIntegration].[usp_EmergencySummaryInbound_Upsert]",
                    new
                    {
                        SourceEmergencyEventId = (Guid)row.EmergencyEventId,
                        EmergencyTypeName = (string)row.EmergencyTypeName,
                        Severity = SeverityLabels[severity],
                        StatusCode = StatusLabels[status],
                        IsStatusTerminal = isTerminal,
                        Location = (string)row.Location,
                        InitiatedBy = (string)row.InitiatedBy,
                        StartedUtc = (DateTime)row.StartedUtc,
                        EndedUtc = (DateTime?)row.EndedUtc,
                    },
                    commandType: System.Data.CommandType.StoredProcedure, cancellationToken: ct));

                DateTime transitionUtc = row.ModifiedUtc ?? row.CreatedUtc;
                await target.ExecuteAsync(new CommandDefinition(
                    "[EmergencyMgtIntegration].[usp_EmergencyLifecycleInbound_Record]",
                    new
                    {
                        SourceEmergencyEventId = (Guid)row.EmergencyEventId,
                        FromStatus = (string?)null,
                        ToStatus = StatusLabels[status],
                        TransitionUtc = transitionUtc,
                    },
                    commandType: System.Data.CommandType.StoredProcedure, cancellationToken: ct));

                pushed++;
            }

            succeeded += pushed;
            details["EmergencySummary+Lifecycle"] = $"{pushed} event(s) pushed. Lifecycle rows reflect current status only (EmergencyEventHistory is not populated by any EmergencyMgt CRUD proc today -- see EmergencyMgt docs/deviations.md DEV-001 -- so no true transition history exists to push).";
        }
        catch (Exception ex)
        {
            failed++;
            details["EmergencySummary+Lifecycle"] = $"FAILED: {ex.Message}";
            logger.LogError(ex, "Failed to push EmergencySummary/Lifecycle.");
        }

        return (succeeded, failed);
    }

    private async Task<(int succeeded, int failed)> PushCorrectiveActionsAsync(
        SqlConnection source, SqlConnection target, int succeeded, int failed, Dictionary<string, string> details, CancellationToken ct)
    {
        try
        {
            var rows = await source.QueryAsync(new CommandDefinition(
                @"SELECT EmergencyCorrectiveActionId, EmergencyEventId, Description, AssignedTo, DueUtc, Status
                  FROM EmergencyManagement.EmergencyCorrectiveAction
                  WHERE IsDeleted = 0",
                cancellationToken: ct));

            int pushed = 0;
            foreach (var row in rows)
            {
                await target.ExecuteAsync(new CommandDefinition(
                    "[EmergencyMgtIntegration].[usp_EmergencyCorrectiveActionInbound_Upsert]",
                    new
                    {
                        SourceCorrectiveActionId = (Guid)row.EmergencyCorrectiveActionId,
                        SourceEmergencyEventId = (Guid?)row.EmergencyEventId,
                        Description = (string)row.Description,
                        AssignedTo = (string)row.AssignedTo,
                        DueUtc = (DateTime?)row.DueUtc,
                        StatusCode = (string)row.Status,
                    },
                    commandType: System.Data.CommandType.StoredProcedure, cancellationToken: ct));
                pushed++;
            }

            succeeded += pushed;
            details["EmergencyCorrectiveActions"] = $"{pushed} corrective action(s) pushed.";
        }
        catch (Exception ex)
        {
            failed++;
            details["EmergencyCorrectiveActions"] = $"FAILED: {ex.Message}";
            logger.LogError(ex, "Failed to push EmergencyCorrectiveActions.");
        }

        return (succeeded, failed);
    }

    private async Task<(int succeeded, int failed)> PushReadinessAsync(
        SqlConnection source, SqlConnection target, int succeeded, int failed, Dictionary<string, string> details, CancellationToken ct)
    {
        try
        {
            var rows = await source.QueryAsync(new CommandDefinition(
                @"SELECT EmergencyReadinessScoreId, OverallScore, FactorScoresJson, ComputedUtc
                  FROM EmergencyManagement.EmergencyReadinessScore
                  WHERE IsDeleted = 0",
                cancellationToken: ct));

            int pushed = 0;
            foreach (var row in rows)
            {
                await target.ExecuteAsync(new CommandDefinition(
                    "[EmergencyMgtIntegration].[usp_EmergencyReadinessInbound_Upsert]",
                    new
                    {
                        SourceReadinessScoreId = (Guid)row.EmergencyReadinessScoreId,
                        OverallScore = (decimal)row.OverallScore,
                        FactorScoresJson = (string)row.FactorScoresJson,
                        ComputedUtc = (DateTime)row.ComputedUtc,
                    },
                    commandType: System.Data.CommandType.StoredProcedure, cancellationToken: ct));
                pushed++;
            }

            succeeded += pushed;
            details["EmergencyReadiness"] = $"{pushed} readiness score(s) pushed.";
        }
        catch (Exception ex)
        {
            failed++;
            details["EmergencyReadiness"] = $"FAILED: {ex.Message}";
            logger.LogError(ex, "Failed to push EmergencyReadiness.");
        }

        return (succeeded, failed);
    }

    private async Task<(int succeeded, int failed)> PushVersionManifestSignalAsync(
        SqlConnection source, SqlConnection target, int succeeded, int failed, Dictionary<string, string> details, CancellationToken ct)
    {
        try
        {
            var row = await source.QuerySingleOrDefaultAsync(new CommandDefinition(
                @"SELECT TOP 1 VersionManifestId, SchemaVersionLabel, ManifestHash, CreatedUtc
                  FROM Governance.VersionManifest
                  ORDER BY CreatedUtc DESC",
                cancellationToken: ct));

            if (row is null)
            {
                details["VersionManifestSignal"] = "No VersionManifest row exists yet in EmergencyMgt's own Governance.VersionManifest; nothing to push.";
                return (succeeded, failed);
            }

            string payloadJson = System.Text.Json.JsonSerializer.Serialize(new
            {
                versionManifestId = (Guid)row.VersionManifestId,
                schemaVersionLabel = (string)row.SchemaVersionLabel,
                manifestHash = ((string)row.ManifestHash).Trim(),
                createdUtc = (DateTime)row.CreatedUtc,
            });

            await target.ExecuteAsync(new CommandDefinition(
                "[EmergencyMgtIntegration].[usp_EmergencyGovernanceLog_Record]",
                new { SignalType = "VersionManifest", PayloadJson = payloadJson },
                commandType: System.Data.CommandType.StoredProcedure, cancellationToken: ct));

            succeeded++;
            details["VersionManifestSignal"] = $"Pushed EmergencyMgt's latest VersionManifest ({(string)row.SchemaVersionLabel}).";
        }
        catch (Exception ex)
        {
            failed++;
            details["VersionManifestSignal"] = $"FAILED: {ex.Message}";
            logger.LogError(ex, "Failed to push VersionManifest signal.");
        }

        return (succeeded, failed);
    }
}
