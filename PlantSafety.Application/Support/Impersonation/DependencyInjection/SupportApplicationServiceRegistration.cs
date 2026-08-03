// AUTHOR: PlantSafety Platform Team (Phase 7 §6.3 Support Tooling)
// CREATED: 2026-08-03
// FILE DESCRIPTION: Application layer DI registration entry point for the Support.Impersonation
//   module. MediatR handlers/validators are already covered by ApplicationServiceRegistration's
//   assembly-wide AddMediatR/AddValidatorsFromAssembly scan (this module ships no additional
//   pipeline behaviors or module-scoped singletons of its own -- it depends only on the
//   already-registered ICacheService, ITenantReadRepository, and IPermissionUsageLogRepository),
//   so this method exists as the canonical, discoverable extension point future Support
//   sub-features should register into, matching the sibling {Module}ApplicationServiceRegistration
//   convention used throughout this codebase (e.g. AddBillingApplicationServices).
// SchemaVersion: 1

using Microsoft.Extensions.DependencyInjection;

namespace PlantSafety.Application.Support.Impersonation.DependencyInjection;

/// <summary>DI registration entry point for the Support.Impersonation module's Application layer.</summary>
public static class SupportApplicationServiceRegistration
{
    /// <summary>
    /// Registers Support.Impersonation module-specific Application-layer services. MediatR/
    /// FluentValidation discovery of this module's commands/queries/validators is already handled
    /// by <c>ApplicationServiceRegistration.AddApplicationServices()</c>'s assembly-wide scan.
    /// </summary>
    public static IServiceCollection AddSupportApplicationServices(this IServiceCollection services) => services;
}
