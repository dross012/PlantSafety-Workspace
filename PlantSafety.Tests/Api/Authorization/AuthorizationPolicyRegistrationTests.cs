// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Api.Authorization
//  Layer:          Tests (Unit)
//  File:           AuthorizationPolicyRegistrationTests.cs
//  Created:        2026-07-27
//  Author:         Security & Compliance Agent
//  Description:    Regression guard for the defect this pass fixed: every
//                  [Authorize(Policy = "...")] string referenced anywhere in
//                  PlantSafety.Api MUST resolve to an actually-registered
//                  AuthorizationPolicy. Before this fix, AddPlantSafetyPolicies()
//                  was declared but never invoked, so every one of the ~300+
//                  actions using a per-module policy name would fail at
//                  runtime with InvalidOperationException the first time it
//                  was hit. This test builds the real policy registration
//                  pipeline (AuthorizationPolicyExtensions.AddPlantSafetyPolicies)
//                  and asserts every referenced policy name resolves.
// ============================================================================

using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using PlantSafety.Api.Extensions;

namespace PlantSafety.Tests.Api.Authorization;

public sealed class AuthorizationPolicyRegistrationTests
{
    /// <summary>
    /// Base policies registered directly in ServiceCollectionExtensions.AddPlantSafetyAuthorization
    /// rather than via the auto-discovering AddPlantSafetyPolicies() — verified separately
    /// because that method also configures JWT Bearer authentication (external Authority),
    /// which is out of scope for a policy-registration unit test.
    /// </summary>
    private static readonly string[] BaseAuthPolicies = ["RequireAuthenticatedUser", "RequireDashboardRead"];

    [Fact]
    public async Task EveryControllerReferencedPolicy_IsRegisteredByAddPlantSafetyPolicies()
    {
        var services = new ServiceCollection();
        services.AddAuthorization(options => options.AddPlantSafetyPolicies());
        await using var provider = services.BuildServiceProvider();
        var policyProvider = provider.GetRequiredService<IAuthorizationPolicyProvider>();

        var referencedPolicyNames = DiscoverReferencedPolicyNames()
            .Except(BaseAuthPolicies, StringComparer.Ordinal)
            .ToList();

        referencedPolicyNames.Should().NotBeEmpty(
            "the controller assembly should contain many [Authorize(Policy = ...)] usages to exercise this guard");

        var unregistered = new List<string>();
        foreach (var policyName in referencedPolicyNames)
        {
            var policy = await policyProvider.GetPolicyAsync(policyName);
            if (policy is null)
                unregistered.Add(policyName);
        }

        unregistered.Should().BeEmpty(
            "every [Authorize(Policy = ...)] referenced by a controller must be registered in " +
            "AuthorizationPolicyExtensions.AddPlantSafetyPolicies — an unregistered policy causes a hard " +
            "runtime failure (InvalidOperationException) on first request, which is exactly the defect this " +
            "regression test exists to catch.");
    }

    [Fact]
    public void SafetyTierPolicies_AreAllRegistered()
    {
        var services = new ServiceCollection();
        services.AddAuthorization(options => options.AddPlantSafetyPolicies());
        using var provider = services.BuildServiceProvider();
        var policyProvider = provider.GetRequiredService<IAuthorizationPolicyProvider>();

        foreach (var tierPolicy in new[]
                 {
                     Shared.Authorization.Abstractions.Policies.SafetyTierPolicies.Read,
                     Shared.Authorization.Abstractions.Policies.SafetyTierPolicies.Write,
                     Shared.Authorization.Abstractions.Policies.SafetyTierPolicies.Approve,
                     Shared.Authorization.Abstractions.Policies.SafetyTierPolicies.Admin,
                 })
        {
            policyProvider.GetPolicyAsync(tierPolicy).Result.Should().NotBeNull(
                $"canonical tier policy '{tierPolicy}' must always be registered");
        }
    }

    private static HashSet<string> DiscoverReferencedPolicyNames()
    {
        var names = new HashSet<string>(StringComparer.Ordinal);
        var apiAssembly = typeof(AuthorizationPolicyExtensions).Assembly;

        var controllerTypes = apiAssembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && typeof(ControllerBase).IsAssignableFrom(t));

        foreach (var controllerType in controllerTypes)
        {
            foreach (var attr in controllerType.GetCustomAttributes<AuthorizeAttribute>(inherit: true))
            {
                if (!string.IsNullOrEmpty(attr.Policy))
                    names.Add(attr.Policy!);
            }

            foreach (var method in controllerType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                foreach (var attr in method.GetCustomAttributes<AuthorizeAttribute>(inherit: true))
                {
                    if (!string.IsNullOrEmpty(attr.Policy))
                        names.Add(attr.Policy!);
                }
            }
        }

        return names;
    }
}
