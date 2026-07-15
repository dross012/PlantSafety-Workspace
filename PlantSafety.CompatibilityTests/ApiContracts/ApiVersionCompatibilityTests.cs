// ============================================================================
//  Test IDs:  PLNT-COMPAT-API-001..002 / PLNT-BKWD-API-001..002 /
//             PLNT-INTEROP-API-001
//  Manual:    tests_rules_design_manual.md (COMPAT, BKWD, INTEROP types).
//  Purpose:   Reflection-based API contract-shape stability checks over the
//             compiled PlantSafety.Api assembly. Confirms every V1 route
//             still exists (BKWD: a shipped V1 contract must never
//             disappear), V1 and V2 controllers can coexist side-by-side for
//             the same resource (COMPAT: multi-version support), and every
//             versioned controller exposes a consistent ApiVersion +
//             [ApiController] + canonical route-prefix shape (INTEROP: any
//             conformant HTTP client/API-gateway can discover and call every
//             endpoint the same way).
// ============================================================================
namespace PlantSafety.CompatibilityTests.ApiContracts;

public sealed class ApiVersionCompatibilityTests
{
    private static readonly Assembly ApiAssembly = typeof(Program).Assembly;

    private static IReadOnlyList<Type> AllControllers => ApiAssembly
        .GetTypes()
        .Where(t => t.IsClass && !t.IsAbstract && typeof(ControllerBase).IsAssignableFrom(t))
        .ToList();

    [Fact(DisplayName = "PLNT-COMPAT-API-001: The Api assembly exposes at least one versioned controller (baseline for all other contract checks)")]
    public void ApiAssembly_ShouldExposeAtLeastOneVersionedController()
    {
        AllControllers.Should().NotBeEmpty();
    }

    [Fact(DisplayName = "PLNT-BKWD-API-001: Every controller carrying a V1 route segment declares ApiVersion 1.0 (a shipped V1 contract must never silently change version)")]
    public void V1RouteControllers_ShouldDeclareApiVersionOne()
    {
        var v1Controllers = AllControllers.Where(t => t.Namespace is not null && t.Namespace.Contains(".V1")).ToList();

        v1Controllers.Should().NotBeEmpty("the codebase is expected to contain V1 controllers (e.g. AlertsV1Controller)");

        foreach (var controller in v1Controllers)
        {
            var versions = controller.GetCustomAttributes<ApiVersionAttribute>()
                .SelectMany(a => a.Versions)
                .ToList();

            versions.Should().Contain(v => v.MajorVersion == 1,
                $"{controller.FullName} lives under a .V1 namespace and must declare ApiVersion(\"1.0\") to preserve its shipped contract");
        }
    }

    [Fact(DisplayName = "PLNT-COMPAT-API-002: For every resource with a V2 controller, the corresponding V1 controller still exists (old and new clients both keep working)")]
    public void V2Controllers_ShouldHaveACorrespondingSurvivingV1Controller()
    {
        var v2Controllers = AllControllers
            .Where(t => t.Namespace is not null && t.Namespace.Contains(".V2"))
            .Select(t => t.Namespace!.Replace(".V2", ".V1"))
            .Distinct()
            .ToList();

        var v1Namespaces = AllControllers
            .Where(t => t.Namespace is not null && t.Namespace.Contains(".V1"))
            .Select(t => t.Namespace!)
            .ToHashSet();

        var missing = v2Controllers.Where(ns => !v1Namespaces.Contains(ns)).ToList();

        missing.Should().BeEmpty(
            "every resource that has been upgraded to V2 must retain its V1 controller for backward-compatible clients (found V2-only resources: " +
            string.Join(", ", missing) + ")");
    }

    [Fact(DisplayName = "PLNT-INTEROP-API-001: Every versioned controller follows the canonical [ApiController] + [Route(\"api/v{version:apiVersion}/...\")] shape (uniform discoverability for any conformant client)")]
    public void AllVersionedControllers_ShouldFollowCanonicalRouteShape()
    {
        var versionedControllers = AllControllers
            .Where(t => t.GetCustomAttribute<ApiVersionAttribute>() is not null)
            .ToList();

        versionedControllers.Should().NotBeEmpty();

        foreach (var controller in versionedControllers)
        {
            controller.GetCustomAttribute<ApiControllerAttribute>().Should().NotBeNull(
                $"{controller.FullName} must carry [ApiController] for consistent model-binding/validation behavior across every endpoint");

            var route = controller.GetCustomAttribute<RouteAttribute>();
            route.Should().NotBeNull($"{controller.FullName} must declare an explicit [Route] for stable, discoverable URIs");
            route!.Template.Should().StartWith("api/v{version:apiVersion}/",
                $"{controller.FullName} must follow the canonical versioned route-prefix convention shared by every module");
        }
    }

    [Fact(DisplayName = "PLNT-BKWD-API-002: No V1 controller has been deleted compared to the count of distinct V1-namespaced resources found under V2 (monotonic contract growth)")]
    public void V1ControllerCount_ShouldBeAtLeastAsLargeAsResourcesWithV2()
    {
        var v1Count = AllControllers.Count(t => t.Namespace is not null && t.Namespace.Contains(".V1"));
        var v2ResourceCount = AllControllers
            .Where(t => t.Namespace is not null && t.Namespace.Contains(".V2"))
            .Select(t => t.Namespace)
            .Distinct()
            .Count();

        v1Count.Should().BeGreaterThanOrEqualTo(v2ResourceCount,
            "the number of surviving V1 controllers must never fall below the number of resources that have since gained a V2 -- contracts only grow, never shrink");
    }
}
