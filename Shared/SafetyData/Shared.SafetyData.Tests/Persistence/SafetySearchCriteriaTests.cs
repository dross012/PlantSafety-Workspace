// ============================================================================
//  Project:        Shared.SafetyData
//  Module:         Tests.Persistence
//  Layer:          Tests (Unit)
//  File:           SafetySearchCriteriaTests.cs
//  Created:        2026-07-27
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    Unit tests for the Abstractions models' record shape/defaults.
//                  Mirrors Shared.AuditLogging.Tests' AuditLogSearchCriteriaTests.
// ============================================================================
using Shared.SafetyData.Abstractions.Models;

using Xunit;

namespace Shared.SafetyData.Tests.Persistence;

public sealed class SafetySearchCriteriaTests
{
    [Fact]
    public void DefaultCriteria_HasPageNumberOneAndPageSizeFifty()
    {
        var tenantId = Guid.NewGuid();
        var criteria = new SafetySearchCriteria(tenantId);

        Assert.Equal(tenantId, criteria.TenantId);
        Assert.Equal(1, criteria.PageNumber);
        Assert.Equal(50, criteria.PageSize);
        Assert.Null(criteria.SiteId);
        Assert.Null(criteria.FromUtc);
        Assert.Null(criteria.ToUtc);
    }

    [Fact]
    public void HazardSummaryEntry_RoundTripsAllSixFields()
    {
        var identifiedAt = new DateTime(2026, 7, 27, 12, 0, 0, DateTimeKind.Utc);
        var controlMeasures = new List<string> { "Isolate energy source", "Install machine guard" };

        var entry = new HazardSummaryEntry(
            Guid.NewGuid().ToString(),
            identifiedAt,
            "Mechanical",
            "High",
            "Unguarded pinch point on conveyor",
            controlMeasures);

        Assert.Equal(identifiedAt, entry.IdentifiedAtUtc);
        Assert.Equal("Mechanical", entry.Category);
        Assert.Equal("High", entry.RiskLevel);
        Assert.Equal("Unguarded pinch point on conveyor", entry.Description);
        Assert.Equal(2, entry.ControlMeasures.Count);
    }

    [Fact]
    public void CorrectiveActionEntry_RoundTripsAllSixFields()
    {
        var raisedAt = new DateTime(2026, 7, 27, 12, 0, 0, DateTimeKind.Utc);
        var dueAt = raisedAt.AddDays(14);

        var entry = new CorrectiveActionEntry(
            Guid.NewGuid().ToString(),
            raisedAt,
            "Engineering",
            "Open",
            "Install additional guarding on line 3",
            dueAt);

        Assert.Equal(raisedAt, entry.RaisedAtUtc);
        Assert.Equal("Engineering", entry.Category);
        Assert.Equal("Open", entry.Status);
        Assert.Equal(dueAt, entry.DueDateUtc);
    }

    [Fact]
    public void SafetyComplianceEntry_RoundTripsAllSevenFields()
    {
        var asOf = new DateTime(2026, 7, 27, 12, 0, 0, DateTimeKind.Utc);

        var entry = new SafetyComplianceEntry("Hazards", 100, 30, 70, 0, 70.00m, asOf);

        Assert.Equal("Hazards", entry.Category);
        Assert.Equal(100, entry.TotalCount);
        Assert.Equal(30, entry.OpenCount);
        Assert.Equal(70, entry.ClosedCount);
        Assert.Equal(0, entry.OverdueCount);
        Assert.Equal(70.00m, entry.CompliancePercentage);
        Assert.Equal(asOf, entry.AsOfUtc);
    }

    [Fact]
    public void SafetySearchResult_ExposesPagingMetadata()
    {
        var result = new SafetySearchResult<HazardSummaryEntry>(Array.Empty<HazardSummaryEntry>(), PageNumber: 2, PageSize: 25, TotalCount: 100);

        Assert.Empty(result.Items);
        Assert.Equal(2, result.PageNumber);
        Assert.Equal(25, result.PageSize);
        Assert.Equal(100, result.TotalCount);
    }
}
