// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Billing.PaymentProcessing
//  Layer:          Infrastructure
//  File:           UnconfiguredPaymentProcessor.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 7 §3.3 Billing Engine)
//  SchemaVersion:  2026.1
//  Description:    Default, unconfigured IPaymentProcessor. Every operation throws
//                  InvalidOperationException naming exactly what a real gateway integration would
//                  need to implement, because payment processing is a security/financial-sensitive
//                  flow: silently fabricating a fake successful charge against nothing would be far
//                  worse than a clear, immediate failure. Mirrors
//                  Infrastructure.Identity.UnconfiguredSamlAuthenticationProvider exactly (Phase 6
//                  §2.1 SSO pattern). Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Payments;

namespace PlantSafety.Infrastructure.Billing.PaymentProcessing;

/// <summary>
/// Default, unconfigured <see cref="IPaymentProcessor"/>. Every operation throws
/// <see cref="InvalidOperationException"/> with a message pointing at the extension seam.
/// </summary>
/// <remarks>
/// To add real payment processing: reference a payment gateway SDK in
/// <c>PlantSafety.Infrastructure</c> (e.g. Stripe.net, Braintree, Square, Adyen -- none is
/// currently referenced by this solution), implement <see cref="IPaymentProcessor"/> against it
/// (mapping <see cref="ChargeAsync"/> to the gateway's PaymentIntent/Charge creation API --
/// including real idempotency-key support so retried calls never double-charge --
/// <see cref="RefundAsync"/> to its refund API, and <see cref="GetPaymentMethodStatusAsync"/> to
/// its customer/payment-method lookup API, plus a webhook endpoint translating the gateway's
/// async payment-status callbacks into <c>ChargeInvoiceCommand</c>/domain-event follow-up, which
/// this synchronous interface does not itself model), and replace this class's DI registration in
/// <c>BillingInfrastructureServiceRegistration.AddBillingInfrastructure()</c>. No other layer needs
/// to change -- <see cref="IPaymentProcessor"/> is the entire contract the rest of the platform's
/// billing pipeline depends on.
/// </remarks>
public sealed class UnconfiguredPaymentProcessor : IPaymentProcessor
{
    private const string ExtensionSeamMessage =
        "No payment gateway is configured for this deployment. No payment gateway SDK (Stripe.net, " +
        "Braintree, Square, Adyen, etc.) is referenced by PlantSafety.Infrastructure. To enable real " +
        "billing/payment processing, implement " +
        "PlantSafety.Application.Abstractions.Payments.IPaymentProcessor against a payment gateway " +
        "of your choice and replace the UnconfiguredPaymentProcessor DI registration in " +
        "BillingInfrastructureServiceRegistration.AddBillingInfrastructure().";

    /// <inheritdoc />
    public bool IsConfigured => false;

    /// <inheritdoc />
    public Task<PaymentChargeResult> ChargeAsync(
        Guid tenantId, Guid invoiceId, PlantSafety.SharedKernel.Primitives.ValueTypes.Money amount, string idempotencyKey, CancellationToken cancellationToken = default) =>
        throw new InvalidOperationException(ExtensionSeamMessage);

    /// <inheritdoc />
    public Task<PaymentRefundResult> RefundAsync(
        Guid tenantId, string originalTransactionId, PlantSafety.SharedKernel.Primitives.ValueTypes.Money amount, CancellationToken cancellationToken = default) =>
        throw new InvalidOperationException(ExtensionSeamMessage);

    /// <inheritdoc />
    public Task<PaymentMethodStatus> GetPaymentMethodStatusAsync(Guid tenantId, CancellationToken cancellationToken = default) =>
        throw new InvalidOperationException(ExtensionSeamMessage);
}
