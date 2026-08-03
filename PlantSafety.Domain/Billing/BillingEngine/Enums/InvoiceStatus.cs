// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Billing.BillingEngine
//  Layer:          Domain
//  File:           InvoiceStatus.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 7 §3.3 Billing Engine)
//  SchemaVersion:  2026.1
//  Description:    Smart-enum-style value object for an Invoice's lifecycle status. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Billing.BillingEngine.Enums;

/// <summary>Smart-enum-style value object identifying an Invoice's lifecycle status.</summary>
public sealed class InvoiceStatus : IEquatable<InvoiceStatus>
{
    /// <summary>Generated but not yet finalized/sent to the tenant.</summary>
    public static readonly InvoiceStatus Draft = new("Draft");

    /// <summary>Finalized and issued to the tenant; awaiting payment.</summary>
    public static readonly InvoiceStatus Issued = new("Issued");

    /// <summary>Successfully charged/paid in full.</summary>
    public static readonly InvoiceStatus Paid = new("Paid");

    /// <summary>Issued but not paid within the payment terms window.</summary>
    public static readonly InvoiceStatus Overdue = new("Overdue");

    /// <summary>A charge attempt failed and no further automatic retry succeeded.</summary>
    public static readonly InvoiceStatus PaymentFailed = new("PaymentFailed");

    /// <summary>Voided/cancelled before payment; no charge is ever collected.</summary>
    public static readonly InvoiceStatus Void = new("Void");

    /// <summary>All defined InvoiceStatus instances.</summary>
    public static readonly IReadOnlyList<InvoiceStatus> All = [Draft, Issued, Paid, Overdue, PaymentFailed, Void];

    /// <summary>The default status for a newly generated invoice.</summary>
    public static InvoiceStatus Default => Draft;

    /// <summary>Stable machine-readable code.</summary>
    public string Code { get; }

    private InvoiceStatus(string code) => Code = code;

    /// <summary>Resolves an InvoiceStatus instance from its <see cref="Code"/>. Throws if unknown.</summary>
    public static InvoiceStatus From(string code)
        => TryParse(code, out var value) ? value : throw new ArgumentException($"Unknown InvoiceStatus code '{code}'.", nameof(code));

    /// <summary>Attempts to resolve an InvoiceStatus instance from its <see cref="Code"/>.</summary>
    public static bool TryParse(string? code, out InvoiceStatus value)
    {
        value = All.FirstOrDefault(x => string.Equals(x.Code, code, StringComparison.OrdinalIgnoreCase))!;
        return value is not null;
    }

    /// <inheritdoc/>
    public bool Equals(InvoiceStatus? other) => other is not null && Code == other.Code;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as InvoiceStatus);

    /// <inheritdoc/>
    public override int GetHashCode() => Code.GetHashCode(StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public override string ToString() => Code;
}
