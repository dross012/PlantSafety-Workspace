// AUTHOR: PlantSafety Platform Team (Phase 7 §3.3 Billing Engine)
// CREATED: 2026-08-03
// FILE DESCRIPTION: Payment gateway abstraction (Phase 7 §3.3 "Billing Engine"). This is a
//   "bring your own payment gateway" extension seam, deliberately defined without referencing any
//   concrete payment provider SDK -- no Stripe.net, Braintree, or similar package is referenced
//   anywhere in the PlantSafety solution, and none is added here (mirrors the
//   ISamlAuthenticationProvider / UnconfiguredSamlAuthenticationProvider pattern from Phase 6
//   §2.1). The default registered implementation
//   (Infrastructure.Billing.PaymentProcessing.UnconfiguredPaymentProcessor) fails fast with an
//   actionable error naming exactly what a real integration needs to implement, rather than
//   silently reporting a fake successful charge against nothing.
// SchemaVersion: 1
// Additive-only evolution: new members may be added; no members may be removed or renamed.

using PlantSafety.SharedKernel.Primitives.ValueTypes;

namespace PlantSafety.Application.Abstractions.Payments;

/// <summary>Outcome of a charge attempt.</summary>
/// <param name="Succeeded">Whether the charge was accepted by the gateway.</param>
/// <param name="TransactionId">The gateway's transaction identifier, if <paramref name="Succeeded"/>.</param>
/// <param name="FailureReason">A sanitized, user-safe failure description, if not <paramref name="Succeeded"/>.</param>
public sealed record PaymentChargeResult(bool Succeeded, string? TransactionId, string? FailureReason);

/// <summary>Outcome of a refund attempt.</summary>
/// <param name="Succeeded">Whether the refund was accepted by the gateway.</param>
/// <param name="RefundTransactionId">The gateway's refund transaction identifier, if <paramref name="Succeeded"/>.</param>
/// <param name="FailureReason">A sanitized, user-safe failure description, if not <paramref name="Succeeded"/>.</param>
public sealed record PaymentRefundResult(bool Succeeded, string? RefundTransactionId, string? FailureReason);

/// <summary>Current status of a tenant's on-file payment method.</summary>
public enum PaymentMethodStatus
{
    /// <summary>No payment method is on file for this tenant.</summary>
    NotConfigured = 0,

    /// <summary>A payment method is on file and chargeable.</summary>
    Active = 1,

    /// <summary>A payment method is on file but has expired (e.g. expired card).</summary>
    Expired = 2,

    /// <summary>The most recent charge attempt against the on-file payment method was declined.</summary>
    Declined = 3,
}

/// <summary>
/// Abstraction for a payment gateway integration (Charge/Refund/GetPaymentMethodStatus). Defines
/// the minimal surface a real gateway implementation needs to expose to plug into PlantSafety's
/// billing pipeline, without this solution taking a compile-time dependency on any specific
/// payment SDK.
/// </summary>
/// <remarks>
/// No payment gateway SDK is referenced by this solution today (verified: no Stripe.net,
/// Braintree, Square, Adyen, or similar package reference exists anywhere in the tree). This
/// interface is the intentional "bring your own payment gateway" extension seam: a future
/// integration adds the chosen gateway's NuGet package to <c>PlantSafety.Infrastructure</c> only,
/// implements this interface there (typically mapping <see cref="ChargeAsync"/> to the gateway's
/// PaymentIntent/Charge/Transaction creation API, <see cref="RefundAsync"/> to its refund API, and
/// <see cref="GetPaymentMethodStatusAsync"/> to its customer/payment-method lookup API, plus real
/// idempotency-key handling and webhook-driven async status updates that this synchronous
/// interface does not itself model), and replaces the DI registration of
/// <c>UnconfiguredPaymentProcessor</c> -- no other layer changes.
/// </remarks>
public interface IPaymentProcessor
{
    /// <summary>
    /// Returns <see langword="true"/> when a real payment gateway is configured for the current
    /// environment. Always <see langword="false"/> for the default no-op implementation.
    /// </summary>
    bool IsConfigured { get; }

    /// <summary>
    /// Attempts to charge the tenant's on-file payment method for <paramref name="amount"/>,
    /// referencing <paramref name="invoiceId"/> and idempotency key <paramref name="idempotencyKey"/>
    /// (so a retried call never double-charges).
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown by the default implementation when no payment gateway is configured -- see
    /// <see cref="IsConfigured"/>.
    /// </exception>
    Task<PaymentChargeResult> ChargeAsync(
        Guid tenantId, Guid invoiceId, Money amount, string idempotencyKey, CancellationToken cancellationToken = default);

    /// <summary>Attempts to refund a previously successful charge, identified by <paramref name="originalTransactionId"/>, for <paramref name="amount"/> (which may be a partial refund).</summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown by the default implementation when no payment gateway is configured -- see
    /// <see cref="IsConfigured"/>.
    /// </exception>
    Task<PaymentRefundResult> RefundAsync(
        Guid tenantId, string originalTransactionId, Money amount, CancellationToken cancellationToken = default);

    /// <summary>Returns the current status of the tenant's on-file payment method.</summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown by the default implementation when no payment gateway is configured -- see
    /// <see cref="IsConfigured"/>.
    /// </exception>
    Task<PaymentMethodStatus> GetPaymentMethodStatusAsync(Guid tenantId, CancellationToken cancellationToken = default);
}
