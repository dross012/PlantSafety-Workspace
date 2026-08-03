// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.UnifiedModel
//  Layer:          Domain
//  File:           IFactRecord.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Marker/contract abstraction shared by every fact table projection in the
//                  cross-module unified analytics data model. Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Primitives.ValueTypes;

namespace PlantSafety.Domain.Analytics.UnifiedModel.Abstractions;

/// <summary>
/// Contract implemented by every fact-table read-model projection in the unified analytics
/// data model (Rule: Phase 5 §4.1). A fact record represents a single measured business event
/// at a fixed grain, foreign-keyed to one or more dimension tables via surrogate <see cref="Guid"/>
/// dimension keys, plus the degenerate operational keys needed to trace back to the owning
/// source-of-truth aggregate.
/// </summary>
public interface IFactRecord
{
    /// <summary>Tenant that owns this fact row.</summary>
    Guid TenantId { get; }

    /// <summary>Surrogate key of the associated <c>DimSite</c> row, if site-scoped.</summary>
    Guid? SiteKey { get; }

    /// <summary>Surrogate key of the associated <c>DimDate</c> row for the fact's business date.</summary>
    Guid DateKey { get; }

    /// <summary>UTC instant this fact row was captured/derived by the ETL/promotion pipeline.</summary>
    UtcTimestamp CapturedAtUtc { get; }

    /// <summary>Identifier of the source aggregate this fact row was projected from.</summary>
    Guid SourceAggregateId { get; }

    /// <summary>Fully-qualified name of the source bounded context/module (e.g. "RiskManagement").</summary>
    string SourceModule { get; }
}
