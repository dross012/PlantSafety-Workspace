// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-08-03
// FILE DESCRIPTION: Singleton state container for the Admin Portal's Billing tab (subscription
//   plan, usage metering summary, invoice list/detail).
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;

using PlantSafety.UI.Modules.Admin.Models;

namespace PlantSafety.UI.Modules.Admin.State;

/// <summary>Singleton state container for the Admin Portal's Billing tab.</summary>
public sealed class BillingState
{
    private SubscriptionPlanDto? _subscriptionPlan;
    private TenantUsageMeteringSummaryDto? _usageMetering;
    private IReadOnlyList<InvoiceDto> _invoices = [];
    private bool _isLoading;
    private string? _error;
    private string? _actionMessage;

    /// <summary>Gets the tenant's currently active subscription plan, or null when not loaded.</summary>
    public SubscriptionPlanDto? SubscriptionPlan => _subscriptionPlan;

    /// <summary>Gets the tenant's usage-metering summary for the queried period, or null when not loaded.</summary>
    public TenantUsageMeteringSummaryDto? UsageMetering => _usageMetering;

    /// <summary>Gets every loaded invoice for the tenant, most recent period first.</summary>
    public IReadOnlyList<InvoiceDto> Invoices => _invoices;

    /// <summary>Gets a value indicating whether the Billing tab is loading.</summary>
    public bool IsLoading => _isLoading;

    /// <summary>Gets a sanitized load error, or null.</summary>
    public string? Error => _error;

    /// <summary>Gets the most recent invoice action outcome message.</summary>
    public string? ActionMessage => _actionMessage;

    /// <summary>Raised whenever the state changes.</summary>
    public event Action? OnChange;

    /// <summary>Marks the Billing tab as loading.</summary>
    public void BeginLoad() { _isLoading = true; _error = null; NotifyStateChanged(); }

    /// <summary>Applies freshly loaded billing data.</summary>
    public void SetData(SubscriptionPlanDto? plan, TenantUsageMeteringSummaryDto? usage, IReadOnlyList<InvoiceDto> invoices)
    {
        _subscriptionPlan = plan;
        _usageMetering = usage;
        _invoices = invoices;
        _isLoading = false;
        _error = null;
        NotifyStateChanged();
    }

    /// <summary>Records a billing data load failure.</summary>
    public void SetError(string sanitizedError) { _error = sanitizedError; _isLoading = false; NotifyStateChanged(); }

    /// <summary>Applies the result of a generate/charge/refund invoice action, updating the list in-place.</summary>
    public void ApplyInvoiceUpdate(InvoiceDto updated, string message)
    {
        var found = false;
        var next = new List<InvoiceDto>(_invoices.Count);
        foreach (var invoice in _invoices)
        {
            if (invoice.InvoiceId == updated.InvoiceId) { next.Add(updated); found = true; }
            else next.Add(invoice);
        }
        if (!found) next.Insert(0, updated);

        _invoices = next;
        _actionMessage = message;
        NotifyStateChanged();
    }

    /// <summary>Records an invoice action outcome message directly (e.g. after a refund that returns no invoice payload).</summary>
    public void SetActionMessage(string message) { _actionMessage = message; NotifyStateChanged(); }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
