// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Singleton state container for the Human Factors
//   dashboard. Human Factors has no dedicated backend of its own (only a
//   Domain aggregate exists — see PlantSafety.Domain.HumanFactors) so this
//   dashboard aggregates the three sibling read-models that DO have working
//   backends (Fatigue Management, Ergonomics, Industrial Hygiene) into a
//   single cross-module risk summary, and surfaces Worker WellBeing as a
//   graceful "not yet available" navigation tile.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;

namespace PlantSafety.UI.Modules.HumanFactors.State;

/// <summary>Aggregated risk counts for one Human Factors sub-domain (Fatigue, Ergonomics, or Industrial Hygiene).</summary>
public sealed record HumanFactorsSectionSummary
{
    /// <summary>Gets the count of High-risk/exposure records.</summary>
    public int HighCount { get; init; }

    /// <summary>Gets the count of Medium-risk/exposure records.</summary>
    public int MediumCount { get; init; }

    /// <summary>Gets the count of Low-risk/exposure records.</summary>
    public int LowCount { get; init; }

    /// <summary>Gets a sanitized load error for this section, or null when the section loaded successfully.</summary>
    public string? Error { get; init; }

    /// <summary>Gets an empty, not-yet-loaded summary.</summary>
    public static HumanFactorsSectionSummary Empty { get; } = new();
}

/// <summary>Singleton state container for the Human Factors dashboard.</summary>
public sealed class HumanFactorsState
{
    private bool _isLoading;

    /// <summary>Gets a value indicating whether the aggregated summary is loading.</summary>
    public bool IsLoading => _isLoading;

    /// <summary>Gets the aggregated Fatigue Management risk summary.</summary>
    public HumanFactorsSectionSummary Fatigue { get; private set; } = HumanFactorsSectionSummary.Empty;

    /// <summary>Gets the aggregated Ergonomics risk summary.</summary>
    public HumanFactorsSectionSummary Ergonomics { get; private set; } = HumanFactorsSectionSummary.Empty;

    /// <summary>Gets the aggregated Industrial Hygiene exposure summary.</summary>
    public HumanFactorsSectionSummary IndustrialHygiene { get; private set; } = HumanFactorsSectionSummary.Empty;

    /// <summary>Gets the combined High-risk count across all loaded sub-domains.</summary>
    public int TotalHighRiskCount => Fatigue.HighCount + Ergonomics.HighCount + IndustrialHygiene.HighCount;

    /// <summary>Raised whenever the state changes.</summary>
    public event Action? OnChange;

    /// <summary>Marks the aggregated summary as loading.</summary>
    public void BeginLoad() { _isLoading = true; NotifyStateChanged(); }

    /// <summary>Applies the loaded Fatigue Management summary.</summary>
    public void SetFatigue(HumanFactorsSectionSummary summary) { Fatigue = summary; NotifyStateChanged(); }

    /// <summary>Applies the loaded Ergonomics summary.</summary>
    public void SetErgonomics(HumanFactorsSectionSummary summary) { Ergonomics = summary; NotifyStateChanged(); }

    /// <summary>Applies the loaded Industrial Hygiene summary.</summary>
    public void SetIndustrialHygiene(HumanFactorsSectionSummary summary) { IndustrialHygiene = summary; NotifyStateChanged(); }

    /// <summary>Marks the aggregated summary as no longer loading.</summary>
    public void EndLoad() { _isLoading = false; NotifyStateChanged(); }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
