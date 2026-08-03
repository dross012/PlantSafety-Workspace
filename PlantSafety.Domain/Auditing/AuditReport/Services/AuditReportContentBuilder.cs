// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Auditing.AuditReport
//  Layer:          Domain
//  File:           AuditReportContentBuilder.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Domain service that deterministically composes the five canonical
//                  AuditReportSection entities (Identity, Timeline, Approvals, Exceptions,
//                  Signatures) from pre-gathered, already-serialized section content. Contains
//                  no infrastructure access -- the Application layer gathers the raw content
//                  (via AuditTimeline queries, approval/signature read models, etc.) and this
//                  service only performs deterministic section assembly and ordering (Handbook
//                  Phase 5 §6.2). Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Common.Services;
using PlantSafety.Domain.Auditing.AuditReport.Entities;
using PlantSafety.Domain.Auditing.AuditReport.Enums;
using PlantSafety.Domain.Auditing.AuditReport.Rules;
using PlantSafety.Domain.Auditing.AuditReport.ValueObjects.Ids;

namespace PlantSafety.Domain.Auditing.AuditReport.Services;

/// <summary>Domain service composing the canonical AuditReportSection set for a report.</summary>
public sealed class AuditReportContentBuilder : IDomainService
{
    /// <summary>
    /// Builds one <see cref="AuditReportSection"/> per entry in <paramref name="sectionContentJson"/>
    /// and validates the result against <see cref="AuditReportGenerationRules.RequiredSections"/>.
    /// </summary>
    /// <param name="reportId">Owning report identity.</param>
    /// <param name="sectionContentJson">Pre-serialized JSON content, keyed by section type.</param>
    /// <exception cref="InvalidOperationException">Thrown when a required section is missing.</exception>
    public IReadOnlyList<AuditReportSection> BuildSections(AuditReportId reportId, IReadOnlyDictionary<AuditReportSectionType, string> sectionContentJson)
    {
        if (!AuditReportGenerationRules.SatisfiesRequiredSections(sectionContentJson.Keys))
        {
            var missing = AuditReportGenerationRules.RequiredSections.Except(sectionContentJson.Keys).Select(s => s.Code);
            throw new InvalidOperationException($"AuditReport content is missing required section(s): {string.Join(", ", missing)}.");
        }

        return sectionContentJson
            .OrderBy(kvp => kvp.Key.PrintOrder)
            .Select(kvp => AuditReportSection.Create(reportId, kvp.Key, kvp.Key.DisplayName, kvp.Value))
            .ToList();
    }
}
