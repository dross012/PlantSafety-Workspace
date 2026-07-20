// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-20
// FILE DESCRIPTION: Singleton state container for the Operations.SchemaGuard module.
//   Holds schema-drift health and the last-fetched migration instructions for the
//   SchemaGuardDashboardPage.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;

using PlantSafety.UI.Modules.SchemaGuard.Models;

namespace PlantSafety.UI.Modules.SchemaGuard.State;

/// <summary>Singleton state container for the Operations.SchemaGuard module.</summary>
public sealed class SchemaGuardState
{
    private IReadOnlyList<SchemaDatabaseHealthDto> _health = [];
    private string? _instructionsDatabaseKey;
    private IReadOnlyList<MigrationInstructionDto> _instructions = [];
    private bool _isLoading;
    private bool _isLoadingInstructions;
    private string? _error;

    /// <summary>Gets the schema-drift status per database.</summary>
    public IReadOnlyList<SchemaDatabaseHealthDto> Health => _health;

    /// <summary>Gets the database key the currently displayed instructions were fetched for, if any.</summary>
    public string? InstructionsDatabaseKey => _instructionsDatabaseKey;

    /// <summary>Gets the migration instructions for <see cref="InstructionsDatabaseKey"/>.</summary>
    public IReadOnlyList<MigrationInstructionDto> Instructions => _instructions;

    /// <summary>Gets a value indicating whether the dashboard data is loading.</summary>
    public bool IsLoading => _isLoading;

    /// <summary>Gets a value indicating whether migration instructions are loading.</summary>
    public bool IsLoadingInstructions => _isLoadingInstructions;

    /// <summary>Gets a sanitized load error, or null.</summary>
    public string? Error => _error;

    /// <summary>Raised whenever the state changes.</summary>
    public event Action? OnChange;

    /// <summary>Marks the dashboard as loading.</summary>
    public void BeginLoad() { _isLoading = true; _error = null; NotifyStateChanged(); }

    /// <summary>Applies freshly loaded schema health data.</summary>
    public void SetHealth(IReadOnlyList<SchemaDatabaseHealthDto> health)
    {
        _health = health;
        _isLoading = false;
        _error = null;
        NotifyStateChanged();
    }

    /// <summary>Records a dashboard load failure with a sanitized message.</summary>
    public void SetError(string sanitizedError) { _error = sanitizedError; _isLoading = false; NotifyStateChanged(); }

    /// <summary>Marks migration instructions as loading for the given database.</summary>
    public void BeginLoadInstructions(string databaseKey) { _isLoadingInstructions = true; _instructionsDatabaseKey = databaseKey; NotifyStateChanged(); }

    /// <summary>Applies freshly loaded migration instructions.</summary>
    public void SetInstructions(IReadOnlyList<MigrationInstructionDto> instructions)
    {
        _instructions = instructions;
        _isLoadingInstructions = false;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
