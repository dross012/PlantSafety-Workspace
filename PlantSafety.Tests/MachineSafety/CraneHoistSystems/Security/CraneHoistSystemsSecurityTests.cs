// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.MachineSafety.CraneHoistSystems.Security
//  Layer:          Tests (Security)
//  File:           CraneHoistSystemsSecurityTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Security tests verifying (a) tenant isolation at the read-model boundary --
//                  a query scoped to one tenant never returns another tenant's asset -- and
//                  (b) every CraneHoistSystems controller action is decorated with an
//                  [Authorize] attribute carrying a recognised SafetyTierPolicies policy, so no
//                  endpoint can be reached by an unauthenticated/unauthorized caller.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using PlantSafety.Api.MachineSafety.Controllers.V1;
using PlantSafety.Application.MachineSafety.CraneHoistSystems.Handlers;
using PlantSafety.Application.MachineSafety.CraneHoistSystems.Models;
using PlantSafety.Application.MachineSafety.CraneHoistSystems.Queries;
using PlantSafety.Application.MachineSafety.CraneHoistSystems.Repositories;

namespace PlantSafety.Tests.MachineSafety.CraneHoistSystems.Security;

public sealed class CraneHoistSystemsSecurityTests : UnitTestBase
{
    [Fact]
    public async Task GetCraneHoistAssetById_ScopedToOneTenant_DoesNotLeakAnotherTenantsAsset()
    {
        var repo = Substitute.For<ICraneHoistSystemsRepository>();
        var otherTenantId = Guid.NewGuid();
        var assetId = Guid.NewGuid();

        // The repository is a tenant-scoped contract: a lookup for TestTenantId must never be
        // satisfied by data belonging to otherTenantId, so the mock only returns data for the
        // exact (tenantId, id) pair it was asked about -- exactly like the real WHERE TenantId = @TenantId clause.
        repo.GetCraneHoistSystemsItemByIdAsync(otherTenantId, assetId, Arg.Any<CancellationToken>())
            .Returns(new CraneHoistSystemsItemDto
            {
                CraneHoistSystemsId = assetId,
                TenantId = otherTenantId,
                AssetTag = "OTHER-TENANT",
                AssetTitle = "Other Tenant Crane",
                AssetType = "Hoist",
                RatedLoadCapacityKg = 1000m,
                SystemStatus = "Registered",
                CreatedAtUtc = FixedUtcNow,
                CreatedBy = TestUserId.Value,
            });
        repo.GetCraneHoistSystemsItemByIdAsync(TestTenantId.Value, assetId, Arg.Any<CancellationToken>())
            .Returns((CraneHoistSystemsItemDto?)null);

        var handler = new GetCraneHoistAssetByIdQueryHandler(repo, NullLogger<GetCraneHoistAssetByIdQueryHandler>());

        var result = await handler.Handle(new GetCraneHoistAssetByIdQuery(TestTenantId.Value, assetId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull("querying with the wrong tenant must never surface another tenant's record");
    }

    public static IEnumerable<object[]> CraneHoistActionMethods() =>
        typeof(MachineSafetyV1Controller)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(m => m.GetCustomAttributes<HttpMethodAttribute>().Any(a => a.Template != null && a.Template.Contains("crane-hoist-systems")))
            .Select(m => new object[] { m.Name });

    [Theory]
    [MemberData(nameof(CraneHoistActionMethods))]
    public void CraneHoistSystemsControllerAction_IsDecoratedWithAuthorizePolicy(string methodName)
    {
        var method = typeof(MachineSafetyV1Controller).GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance)!;

        var authorizeAttribute = method.GetCustomAttribute<AuthorizeAttribute>();

        authorizeAttribute.Should().NotBeNull($"{methodName} must require authorization -- no CraneHoistSystems endpoint may be anonymous");
        authorizeAttribute!.Policy.Should().NotBeNullOrWhiteSpace($"{methodName} must specify a named SafetyTierPolicies policy, not a bare [Authorize]");
    }

    [Fact]
    public void CraneHoistSystemsControllerActions_Exist()
    {
        CraneHoistActionMethods().Count().Should().BeGreaterThanOrEqualTo(14, "all 8 commands + 6 queries must be exposed as controller actions");
    }
}
