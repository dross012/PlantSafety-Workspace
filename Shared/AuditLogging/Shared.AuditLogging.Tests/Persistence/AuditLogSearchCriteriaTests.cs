// ============================================================================
//  Project:        Shared.AuditLogging
//  Module:         Tests.Persistence
//  Layer:          Tests (Unit)
//  File:           AuditLogSearchCriteriaTests.cs
//  Created:        2026-07-26
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    Unit tests for the Abstractions models' record shape/defaults.
// ============================================================================
using Shared.AuditLogging.Abstractions.Models;

using Xunit;

namespace Shared.AuditLogging.Tests.Persistence;

public sealed class AuditLogSearchCriteriaTests
{
    [Fact]
    public void DefaultCriteria_HasPageNumberOneAndPageSizeFifty()
    {
        var criteria = new AuditLogSearchCriteria();

        Assert.Equal(1, criteria.PageNumber);
        Assert.Equal(50, criteria.PageSize);
        Assert.Null(criteria.TableName);
        Assert.Null(criteria.SearchText);
    }

    [Fact]
    public void AuditLogEntry_RoundTripsAllElevenFields()
    {
        var changeTime = new DateTime(2026, 7, 26, 12, 0, 0, DateTimeKind.Utc);

        var entry = new AuditLogEntry(
            "AiGovernance",
            "PlantSafety.CoreDB",
            "INSERT",
            "svc_plantsafety",
            changeTime,
            false,
            "AiGovernanceId=11111111-1111-1111-1111-111111111111",
            0,
            "sp_AiGovernance_Create",
            null,
            null);

        Assert.Equal("AiGovernance", entry.TableName);
        Assert.Equal("PlantSafety.CoreDB", entry.SourceDatabase);
        Assert.Equal("INSERT", entry.Operation);
        Assert.Equal("svc_plantsafety", entry.ChangedBy);
        Assert.Equal(changeTime, entry.ChangeTime);
        Assert.False(entry.ViewerAccessed);
        Assert.Equal(0, entry.Severity);
        Assert.Equal("sp_AiGovernance_Create", entry.Module);
        Assert.Null(entry.BeforeSnapshot);
        Assert.Null(entry.AfterSnapshot);
    }

    [Fact]
    public void AuditLogSearchResult_ExposesPagingMetadata()
    {
        var result = new AuditLogSearchResult(Array.Empty<AuditLogEntry>(), PageNumber: 2, PageSize: 25, TotalCount: 100);

        Assert.Empty(result.Items);
        Assert.Equal(2, result.PageNumber);
        Assert.Equal(25, result.PageSize);
        Assert.Equal(100, result.TotalCount);
    }
}
