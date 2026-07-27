// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Api.Authorization
//  Layer:          Tests (Unit)
//  File:           ControllerAuthorizationTierTests.cs
//  Created:        2026-07-27
//  Author:         Security & Compliance Agent
//  Description:    Reflection-driven controller-level authorization tests covering
//                  every controller in PlantSafety.Api (not a hand-picked sample —
//                  the whole surface, automatically re-scoped as controllers are
//                  added/removed). Verifies:
//                    1. Every routed action enforces SOME authorization (either
//                       [Authorize] resolving to a role-restricting policy, or is on
//                       the explicit, reviewed [AllowAnonymous] allow-list).
//                    2. Within a controller, read (GET) actions never require a
//                       *stricter* Safety tier than that controller's write
//                       (POST/PUT/PATCH/DELETE) actions — i.e. read and write are
//                       correctly separated, matching the "viewers can read, only
//                       editors/approvers/admins can write" rule from
//                       docs/architecture/authorization-tiers.md.
// ============================================================================

using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using PlantSafety.Api.Extensions;
using Shared.Authorization.Abstractions.Roles;

namespace PlantSafety.Tests.Api.Authorization;

public sealed class ControllerAuthorizationTierTests
{
    /// <summary>
    /// Controllers reviewed and confirmed to legitimately allow anonymous access:
    /// PlatformV1/V2Controller (version/health info, no Safety data) and
    /// WebhookReceiverV1Controller (authenticates external callers via a webhook
    /// signature, not a JWT bearer token — see WebhookAuthorizationPolicies.cs).
    /// Any new [AllowAnonymous] controller not on this list fails the test, forcing
    /// a deliberate, reviewed addition rather than silent anonymous-endpoint drift.
    /// </summary>
    private static readonly HashSet<string> KnownAnonymousControllers = new(StringComparer.Ordinal)
    {
        "PlatformV1Controller",
        "PlatformV2Controller",
        "WebhookReceiverV1Controller",
    };

    private static IAuthorizationPolicyProvider BuildPolicyProvider()
    {
        var services = new ServiceCollection();
        services.AddAuthorization(options =>
        {
            // Mirrors ServiceCollectionExtensions.AddPlantSafetyAuthorization's two
            // hand-registered base policies (kept out of AddPlantSafetyPolicies()
            // because that method's job is auto-discovery, not base policies).
            options.AddPolicy("RequireAuthenticatedUser", p => p.RequireAuthenticatedUser());
            options.AddPolicy("RequireDashboardRead", p => p.RequireAuthenticatedUser().RequireRole(
                SafetyRoles.PlatformAdmin, "PlantManager", "SafetyOfficer", "EnvironmentalEngineer",
                "Supervisor", "ComplianceAuditor", "DataScientist", "EsgReportingOfficer", "ReadOnlyAuditor"));
            options.AddPlantSafetyPolicies();
        });
        var provider = services.BuildServiceProvider();
        return provider.GetRequiredService<IAuthorizationPolicyProvider>();
    }

    /// <summary>Viewer=1 .. PlatformAdmin=4. The lowest-numbered role present in a policy's
    /// allowed-role set is that policy's effective tier level (allowed sets are cumulative,
    /// e.g. ViewerAndAbove = {Viewer,Editor,Approver,PlatformAdmin}).</summary>
    private static int? TierLevel(IReadOnlySet<string> allowedRoles)
    {
        if (allowedRoles.Contains(SafetyRoles.Viewer)) return 1;
        if (allowedRoles.Contains(SafetyRoles.Editor)) return 2;
        if (allowedRoles.Contains(SafetyRoles.Approver)) return 3;
        if (allowedRoles.Contains(SafetyRoles.PlatformAdmin)) return 4;
        return null;
    }

    [Fact]
    public void AllControllers_EnforceAuthorizationOrAreOnTheReviewedAnonymousAllowList()
    {
        var policyProvider = BuildPolicyProvider();
        var apiAssembly = typeof(AuthorizationPolicyExtensions).Assembly;

        var controllerTypes = apiAssembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && typeof(ControllerBase).IsAssignableFrom(t))
            .ToList();

        controllerTypes.Should().HaveCountGreaterThan(300,
            "sanity check that reflection is discovering the full ~360-controller surface");

        var failures = new List<string>();

        foreach (var controllerType in controllerTypes)
        {
            var classAuthorize = controllerType.GetCustomAttributes<AuthorizeAttribute>(inherit: true).ToList();
            var classAnonymous = controllerType.GetCustomAttribute<AllowAnonymousAttribute>(inherit: true) is not null;

            foreach (var (method, verb) in RoutedActions(controllerType))
            {
                var isAnonymous = classAnonymous || method.GetCustomAttribute<AllowAnonymousAttribute>() is not null;
                if (isAnonymous)
                {
                    if (!KnownAnonymousControllers.Contains(controllerType.Name))
                    {
                        failures.Add(
                            $"{controllerType.FullName}.{method.Name}: [AllowAnonymous] on a controller not in " +
                            $"the reviewed allow-list (ControllerAuthorizationTierTests.KnownAnonymousControllers).");
                    }
                    continue;
                }

                var methodAuthorize = method.GetCustomAttributes<AuthorizeAttribute>(inherit: true).ToList();
                var allAuthorize = classAuthorize.Concat(methodAuthorize).ToList();

                if (allAuthorize.Count == 0)
                {
                    failures.Add(
                        $"{controllerType.FullName}.{method.Name} ({verb}): no [Authorize] at class or method " +
                        $"level, and not on the [AllowAnonymous] allow-list — this endpoint has NO server-side " +
                        $"enforcement.");
                    continue;
                }

                var restrictsByRole = allAuthorize
                    .Where(a => !string.IsNullOrEmpty(a.Policy))
                    .Select(a => policyProvider.GetPolicyAsync(a.Policy!).GetAwaiter().GetResult())
                    .Any(p => p?.Requirements.OfType<RolesAuthorizationRequirement>().Any() == true);

                if (!restrictsByRole)
                {
                    failures.Add(
                        $"{controllerType.FullName}.{method.Name} ({verb}): only requires authentication " +
                        $"(e.g. RequireAuthenticatedUser) with no role-restricting policy — any authenticated " +
                        $"user, regardless of tier, can call this endpoint.");
                }
            }
        }

        failures.Should().BeEmpty(because: "every routed action must enforce role-based authorization; failures:" +
                                            Environment.NewLine + string.Join(Environment.NewLine, failures));
    }

    [Fact]
    public void PerController_ReadActionsNeverRequireAStricterTierThanWriteActions()
    {
        var policyProvider = BuildPolicyProvider();
        var apiAssembly = typeof(AuthorizationPolicyExtensions).Assembly;

        var controllerTypes = apiAssembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && typeof(ControllerBase).IsAssignableFrom(t));

        var failures = new List<string>();

        foreach (var controllerType in controllerTypes)
        {
            var classAuthorize = controllerType.GetCustomAttributes<AuthorizeAttribute>(inherit: true).ToList();
            var classAnonymous = controllerType.GetCustomAttribute<AllowAnonymousAttribute>(inherit: true) is not null;
            if (classAnonymous) continue;

            var readLevels = new List<int>();
            var writeLevels = new List<int>();

            foreach (var (method, verb) in RoutedActions(controllerType))
            {
                if (method.GetCustomAttribute<AllowAnonymousAttribute>() is not null) continue;

                var allAuthorize = classAuthorize.Concat(method.GetCustomAttributes<AuthorizeAttribute>(inherit: true)).ToList();

                // Effective tier = the strictest (highest-level) policy applied to this action,
                // since ASP.NET Core combines multiple [Authorize] attributes with AND semantics.
                int? effectiveLevel = null;
                foreach (var attr in allAuthorize)
                {
                    if (string.IsNullOrEmpty(attr.Policy)) continue;
                    var policy = policyProvider.GetPolicyAsync(attr.Policy!).GetAwaiter().GetResult();
                    var rolesReq = policy?.Requirements.OfType<RolesAuthorizationRequirement>().FirstOrDefault();
                    if (rolesReq is null) continue;

                    var level = TierLevel(new HashSet<string>(rolesReq.AllowedRoles, StringComparer.Ordinal));
                    if (level is null) continue;

                    effectiveLevel = effectiveLevel is null ? level : Math.Max(effectiveLevel.Value, level.Value);
                }

                if (effectiveLevel is null) continue; // covered by the other test in this fixture

                if (string.Equals(verb, "GET", StringComparison.OrdinalIgnoreCase))
                    readLevels.Add(effectiveLevel.Value);
                else
                    writeLevels.Add(effectiveLevel.Value);
            }

            if (readLevels.Count == 0 || writeLevels.Count == 0) continue;

            var maxRead = readLevels.Max();
            var minWrite = writeLevels.Min();

            if (maxRead > minWrite)
            {
                failures.Add(
                    $"{controllerType.FullName}: a GET action requires tier level {maxRead} while a " +
                    $"write action only requires tier level {minWrite} — reads must never require a stricter " +
                    $"tier than writes in the same controller (levels: Viewer=1, Editor=2, Approver=3, Admin=4).");
            }
        }

        failures.Should().BeEmpty(because: "read/write tier separation must be internally consistent per controller; failures:" +
                                            Environment.NewLine + string.Join(Environment.NewLine, failures));
    }

    private static IEnumerable<(MethodInfo Method, string Verb)> RoutedActions(Type controllerType)
    {
        foreach (var method in controllerType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
        {
            if (method.GetCustomAttribute<NonActionAttribute>() is not null) continue;

            var verbAttr = method.GetCustomAttributes()
                .OfType<IActionHttpMethodProvider>()
                .FirstOrDefault();

            if (verbAttr is null) continue;

            foreach (var verb in verbAttr.HttpMethods)
                yield return (method, verb);
        }
    }
}
