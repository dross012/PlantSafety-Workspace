// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Module registration coordinator — documents which modules are registered
//   and in what order. This is an informational type; actual registrations live in
//   ApplicationServiceRegistration and the individual module DI extension files.
// SchemaVersion: 1

namespace PlantSafety.Application.DependencyInjection.Modules;

/// <summary>
/// Documents the set of Application-layer modules registered in this assembly.
/// Used by health checks, startup validation, and observability tooling to enumerate active modules.
/// </summary>
public static class ModuleRegistrationCoordinator
{
    /// <summary>
    /// Ordered list of all registered Application module names.
    /// Must stay in sync with the registrations in <c>ApplicationServiceRegistration</c>.
    /// </summary>
    public static readonly IReadOnlyList<string> RegisteredModules =
    [
        "SafetyEvents",
        "Analytics",
        "Compliance",
        "Financials",
        "PlanningProjects",
        "Operational",
        "ContinuousImprovement",
        "EnterpriseFabric",
    ];
}
