// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-08-05
// FILE DESCRIPTION: Singleton state container for the Create Work Order
//   page (CreateWorkOrderCommand write surface). Mirrors the
//   FinancialsState.CreateBudget* pattern.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;

namespace PlantSafety.UI.Modules.Maintenance.State;

/// <summary>Singleton state container for the Create Work Order page.</summary>
public sealed class CreateWorkOrderState
{
    private bool _isCreating;
    private string? _createError;
    private Guid? _createdWorkOrderId;

    /// <summary>Gets a value indicating whether a Create Work Order submission is in flight.</summary>
    public bool IsCreating => _isCreating;

    /// <summary>Gets a sanitized Create Work Order submission error, or null.</summary>
    public string? CreateError => _createError;

    /// <summary>Gets the identifier of the most recently created work order, or null.</summary>
    public Guid? CreatedWorkOrderId => _createdWorkOrderId;

    /// <summary>Raised whenever the state changes.</summary>
    public event Action? OnChange;

    /// <summary>Marks a Create Work Order submission as in flight.</summary>
    public void BeginCreate() { _isCreating = true; _createError = null; _createdWorkOrderId = null; NotifyStateChanged(); }

    /// <summary>Records a successful Create Work Order submission.</summary>
    public void SetCreateSuccess(Guid workOrderId) { _createdWorkOrderId = workOrderId; _isCreating = false; _createError = null; NotifyStateChanged(); }

    /// <summary>Records a Create Work Order submission failure with a sanitized message.</summary>
    public void SetCreateError(string sanitizedError) { _createError = sanitizedError; _isCreating = false; NotifyStateChanged(); }

    /// <summary>Resets Create Work Order submission state (e.g. when navigating to the form).</summary>
    public void Reset() { _isCreating = false; _createError = null; _createdWorkOrderId = null; NotifyStateChanged(); }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
