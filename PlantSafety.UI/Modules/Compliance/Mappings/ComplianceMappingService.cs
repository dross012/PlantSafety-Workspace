// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: UI-layer mapping service projecting the three Compliance
//   read-model DTOs (RequirementItemDto/ObligationItemDto/TrackingItemDto)
//   into the unified ComplianceRowViewModel/ComplianceDetailViewModel shapes.
//   Named and shaped after the platform's standard Mapster DTO -> ViewModel
//   convention (Adapt-style pure static projections, no side effects);
//   implemented as hand-written static methods rather than the Mapster
//   package because PlantSafety.UI.csproj does not reference Mapster (see
//   Modules/PlanningProjects/Mappings/PlanningProjectsMappingService.cs for
//   the precedent establishing this convention).
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;
using System.Linq;

using PlantSafety.UI.Modules.Compliance.Models;

namespace PlantSafety.UI.Modules.Compliance.Mappings;

/// <summary>Pure, stateless DTO -&gt; ViewModel projections for the Compliance dashboard module.</summary>
public static class ComplianceMappingService
{
    /// <summary>Projects a Regulatory Requirement item into a unified dashboard row.</summary>
    public static ComplianceRowViewModel ToRowViewModel(this RequirementItemDto dto) => new()
    {
        Category = ComplianceCategory.Requirement,
        ItemId = dto.RequirementId,
        Title = dto.RequirementTitle,
        Classifier = dto.Jurisdiction,
        Status = dto.Status,
        StatusIndicatorState = ToStatusIndicatorState(dto.Status),
        RelevantDateUtc = dto.EffectiveAtUtc,
        RelevantDateLabel = "Effective",
        OwnerId = dto.OwnerId,
    };

    /// <summary>Projects an Obligation item into a unified dashboard row.</summary>
    public static ComplianceRowViewModel ToRowViewModel(this ObligationItemDto dto) => new()
    {
        Category = ComplianceCategory.Obligation,
        ItemId = dto.ObligationId,
        Title = dto.ObligationTitle,
        Classifier = dto.RegulatoryBody,
        Status = dto.Status,
        StatusIndicatorState = ToStatusIndicatorState(dto.Status),
        RelevantDateUtc = dto.DueAtUtc,
        RelevantDateLabel = "Due",
        OwnerId = dto.OwnerId,
    };

    /// <summary>Projects a Tracking item into a unified dashboard row.</summary>
    public static ComplianceRowViewModel ToRowViewModel(this TrackingItemDto dto) => new()
    {
        Category = ComplianceCategory.Tracking,
        ItemId = dto.TrackingItemId,
        Title = dto.ItemDescription,
        Classifier = dto.Category,
        Status = dto.Status,
        StatusIndicatorState = ToStatusIndicatorState(dto.Status),
        RelevantDateUtc = dto.LastUpdatedAtUtc,
        RelevantDateLabel = "Last Updated",
        OwnerId = dto.OwnerId,
    };

    /// <summary>Projects a list of Requirement items into unified rows.</summary>
    public static IReadOnlyList<ComplianceRowViewModel> ToRowViewModels(this IEnumerable<RequirementItemDto> items) =>
        items.Select(ToRowViewModel).ToList();

    /// <summary>Projects a list of Obligation items into unified rows.</summary>
    public static IReadOnlyList<ComplianceRowViewModel> ToRowViewModels(this IEnumerable<ObligationItemDto> items) =>
        items.Select(ToRowViewModel).ToList();

    /// <summary>Projects a list of Tracking items into unified rows.</summary>
    public static IReadOnlyList<ComplianceRowViewModel> ToRowViewModels(this IEnumerable<TrackingItemDto> items) =>
        items.Select(ToRowViewModel).ToList();

    /// <summary>Projects a Regulatory Requirement item into a full detail view model.</summary>
    public static ComplianceDetailViewModel ToDetailViewModel(this RequirementItemDto dto) => new()
    {
        Category = ComplianceCategory.Requirement,
        ItemId = dto.RequirementId,
        Title = dto.RequirementTitle,
        Classifier = dto.Jurisdiction,
        ClassifierLabel = "Jurisdiction",
        Status = dto.Status,
        StatusIndicatorState = ToStatusIndicatorState(dto.Status),
        RelevantDateUtc = dto.EffectiveAtUtc,
        RelevantDateLabel = "Effective",
        OwnerId = dto.OwnerId,
        TenantId = dto.TenantId,
        SiteId = dto.SiteId,
    };

    /// <summary>Projects an Obligation item into a full detail view model.</summary>
    public static ComplianceDetailViewModel ToDetailViewModel(this ObligationItemDto dto) => new()
    {
        Category = ComplianceCategory.Obligation,
        ItemId = dto.ObligationId,
        Title = dto.ObligationTitle,
        Classifier = dto.RegulatoryBody,
        ClassifierLabel = "Regulatory Body",
        Status = dto.Status,
        StatusIndicatorState = ToStatusIndicatorState(dto.Status),
        RelevantDateUtc = dto.DueAtUtc,
        RelevantDateLabel = "Due",
        OwnerId = dto.OwnerId,
        TenantId = dto.TenantId,
        SiteId = dto.SiteId,
    };

    /// <summary>Projects a Tracking item into a full detail view model.</summary>
    public static ComplianceDetailViewModel ToDetailViewModel(this TrackingItemDto dto) => new()
    {
        Category = ComplianceCategory.Tracking,
        ItemId = dto.TrackingItemId,
        Title = dto.ItemDescription,
        Classifier = dto.Category,
        ClassifierLabel = "Category",
        Status = dto.Status,
        StatusIndicatorState = ToStatusIndicatorState(dto.Status),
        RelevantDateUtc = dto.LastUpdatedAtUtc,
        RelevantDateLabel = "Last Updated",
        OwnerId = dto.OwnerId,
        TenantId = dto.TenantId,
        SiteId = dto.SiteId,
    };

    /// <summary>Maps a raw Compliance status code to the closest StatusIndicatorComponent operational state.</summary>
    /// <param name="status">The raw status code (case-insensitive; covers the Requirement/Obligation/Tracking vocabularies).</param>
    public static string ToStatusIndicatorState(string status) => status.Trim().ToLowerInvariant() switch
    {
        "applicable" or "fulfilled" or "ontrack" or "closed" => "Healthy",
        "underreview" or "open" or "atrisk" or "pending" => "Warning",
        "overdue" or "critical" => "Critical",
        "superseded" or "cancelled" or "archived" => "Offline",
        _ => "Offline",
    };
}
