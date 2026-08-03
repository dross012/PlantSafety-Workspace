// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.RulesEngine
//  Layer:          Infrastructure
//  File:           ComplianceEvaluationReadDapperRepository.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the Application-layer ComplianceEvaluation read model
//                  (IComplianceEvaluationReadRepository). Reuses the same stored procedures as
//                  the Domain-facing write repository (mirrors RiskReadDapperRepository's
//                  precedent) but projects flat DTOs directly rather than rehydrating the
//                  aggregate. Additive-only evolution.
// ============================================================================
using System.Text.Json;
using PlantSafety.Application.Compliance.RulesEngine.Models;
using PlantSafety.Application.Compliance.RulesEngine.Repositories;

namespace PlantSafety.Infrastructure.Compliance.RulesEngine.Persistence;

/// <summary>Dapper-backed repository for ComplianceEvaluation read-model queries.</summary>
public sealed class ComplianceEvaluationReadDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<ComplianceEvaluationReadDapperRepository> logger)
    : IComplianceEvaluationReadRepository
{
    /// <inheritdoc/>
    public async Task<ComplianceEvaluationDto?> GetByIdAsync(Guid tenantId, Guid complianceEvaluationId, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetById read-model for ComplianceEvaluation {ComplianceEvaluationId} Tenant {TenantId}", complianceEvaluationId, tenantId);

        var parameters = new StoredProcedureBuilder().AddGuid("ComplianceEvaluationId", complianceEvaluationId).AddGuid("TenantId", tenantId).Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<EvaluationRow>(
            "[ComplianceRulesEngine].[usp_ComplianceEvaluation_GetById]", parameters, cancellationToken);

        if (row is null) return null;

        var resultParameters = new StoredProcedureBuilder().AddGuid("ComplianceEvaluationId", complianceEvaluationId).Build();
        var resultRows = await queryExecutor.QueryAsync<RuleResultRow>(
            "[ComplianceRulesEngine].[usp_ComplianceRuleResult_GetByEvaluationId]", resultParameters, cancellationToken);

        return ToDto(row, resultRows);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ComplianceEvaluationDto>> GetHistoryForTargetAsync(Guid tenantId, string targetEntityType, Guid targetEntityId, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddString("TargetEntityType", targetEntityType, 200)
            .AddGuid("TargetEntityId", targetEntityId)
            .Build();

        var rows = await queryExecutor.QueryAsync<EvaluationRow>(
            "[ComplianceRulesEngine].[usp_ComplianceEvaluation_GetHistoryForTarget]", parameters, cancellationToken);

        var results = new List<ComplianceEvaluationDto>();
        foreach (var row in rows)
        {
            var resultParameters = new StoredProcedureBuilder().AddGuid("ComplianceEvaluationId", row.ComplianceEvaluationId).Build();
            var resultRows = await queryExecutor.QueryAsync<RuleResultRow>(
                "[ComplianceRulesEngine].[usp_ComplianceRuleResult_GetByEvaluationId]", resultParameters, cancellationToken);

            results.Add(ToDto(row, resultRows));
        }

        return results;
    }

    private static ComplianceEvaluationDto ToDto(EvaluationRow row, IEnumerable<RuleResultRow> resultRows)
        => new()
        {
            ComplianceEvaluationId = row.ComplianceEvaluationId,
            TenantId = row.TenantId,
            SiteId = row.SiteId,
            TargetEntityType = row.TargetEntityType,
            TargetEntityId = row.TargetEntityId,
            Status = row.Status,
            RuleResults = resultRows.Select(r => new ComplianceRuleResultDto
            {
                RuleCode = r.RuleCode,
                RuleName = r.RuleName,
                Category = r.Category,
                Passed = r.Passed,
                Violations = (JsonSerializer.Deserialize<List<ViolationJson>>(r.ViolationsJson) ?? [])
                    .Select(v => new ComplianceViolationDto
                    {
                        ViolationCode = v.ViolationCode,
                        Description = v.Description,
                        Severity = v.Severity,
                        RemediationGuidance = v.RemediationGuidance,
                    }).ToList(),
                EvaluatedAtUtc = r.EvaluatedAtUtc,
            }).ToList(),
            StartedAtUtc = row.StartedAtUtc,
            CompletedAtUtc = row.CompletedAtUtc,
            TriggeredBy = row.TriggeredBy,
        };

    private sealed record ViolationJson(string ViolationCode, string Description, string Severity, string? RemediationGuidance);

    private sealed record EvaluationRow
    {
        public required Guid ComplianceEvaluationId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string TargetEntityType { get; init; }
        public required Guid TargetEntityId { get; init; }
        public required string Status { get; init; }
        public required DateTime StartedAtUtc { get; init; }
        public DateTime? CompletedAtUtc { get; init; }
        public required Guid TriggeredBy { get; init; }
    }

    private sealed record RuleResultRow
    {
        public required string RuleCode { get; init; }
        public required string RuleName { get; init; }
        public required string Category { get; init; }
        public required bool Passed { get; init; }
        public required string ViolationsJson { get; init; }
        public required DateTime EvaluatedAtUtc { get; init; }
    }
}
