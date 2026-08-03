// AUTHOR: PlantSafety Platform Team (Phase 7 §3 Billing, Metering & Subscription Management)
// CREATED: 2026-08-03
// FILE DESCRIPTION: Application layer DI registration entry point for the Billing module
//   (Subscription, Metering, BillingEngine). MediatR handlers/validators are already covered by
//   ApplicationServiceRegistration's assembly-wide AddMediatR/AddValidatorsFromAssembly scan (this
//   module ships no additional pipeline behaviors or module-scoped singletons of its own), so this
//   method exists as the canonical, discoverable extension point future Billing sub-features
//   should register into, matching the sibling {Module}ApplicationServiceRegistration convention
//   used throughout this codebase (e.g. AddTenantApplicationServices).
// SchemaVersion: 1

using Microsoft.Extensions.DependencyInjection;

namespace PlantSafety.Application.Billing.DependencyInjection;

/// <summary>DI registration entry point for the Billing module's Application layer.</summary>
public static class BillingApplicationServiceRegistration
{
    /// <summary>
    /// Registers Billing module-specific Application-layer services. MediatR/FluentValidation
    /// discovery of this module's commands/queries/validators is already handled by
    /// <c>ApplicationServiceRegistration.AddApplicationServices()</c>'s assembly-wide scan.
    /// </summary>
    public static IServiceCollection AddBillingApplicationServices(this IServiceCollection services) => services;
}
