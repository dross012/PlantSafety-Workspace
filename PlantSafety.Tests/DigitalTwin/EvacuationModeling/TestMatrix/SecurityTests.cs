// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.DigitalTwin.EvacuationModeling.TestMatrix
//  Layer:          Tests (Unit) -- Security
//  File:           SecurityTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Security test type: asserts every action on EvacuationModelingV1Controller
//                  requires authentication and carries an explicit authorization policy (no
//                  anonymous/unguarded endpoints), and that write actions use SafetyTierPolicies.
//                  Write while read/list/get actions use SafetyTierPolicies.Read -- no command
//                  action is ever reachable under the weaker Read tier. Isolated, deterministic,
//                  no I/O.
// ============================================================================
using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Authorization.Abstractions.Policies;
using ControllerType = PlantSafety.Api.DigitalTwin.EvacuationModeling.Controllers.V1.EvacuationModelingV1Controller;

namespace PlantSafety.Tests.DigitalTwin.EvacuationModeling.TestMatrix;

public sealed class SecurityTests : UnitTestBase
{
    [Fact]
    public void Controller_RequiresAuthenticatedUser_AtClassLevel()
    {
        var attr = typeof(ControllerType).GetCustomAttribute<AuthorizeAttribute>();

        attr.Should().NotBeNull("the controller must require authentication for every action by default");
        attr!.Policy.Should().Be("RequireAuthenticatedUser");
    }

    public static IEnumerable<object[]> ActionMethods() =>
        typeof(ControllerType)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(m => typeof(Task<IActionResult>).IsAssignableFrom(m.ReturnType))
            .Select(m => new object[] { m.Name });

    [Theory]
    [MemberData(nameof(ActionMethods))]
    public void EveryAction_CarriesAnExplicitAuthorizationPolicy(string methodName)
    {
        var method = typeof(ControllerType).GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance)!;
        var attr = method.GetCustomAttribute<AuthorizeAttribute>();

        attr.Should().NotBeNull($"{methodName} must carry an explicit [Authorize(Policy=...)] tier attribute -- no anonymous write/read is permitted");
        attr!.Policy.Should().NotBeNullOrWhiteSpace();
    }

    [Theory]
    [InlineData(nameof(ControllerType.DefineEvacuationRouteAsync))]
    [InlineData(nameof(ControllerType.RetireEvacuationRouteAsync))]
    [InlineData(nameof(ControllerType.RecordRouteObstructionAsync))]
    [InlineData(nameof(ControllerType.ClearRouteObstructionAsync))]
    [InlineData(nameof(ControllerType.DefineEvacuationZoneAsync))]
    [InlineData(nameof(ControllerType.RecordOccupancySnapshotAsync))]
    [InlineData(nameof(ControllerType.RunEgressSimulationAsync))]
    [InlineData(nameof(ControllerType.LinkSimulationToDrillAsync))]
    public void WriteActions_RequireWriteTier(string methodName)
    {
        var method = typeof(ControllerType).GetMethod(methodName)!;
        var attr = method.GetCustomAttribute<AuthorizeAttribute>();

        attr!.Policy.Should().Be(SafetyTierPolicies.Write);
    }

    [Theory]
    [InlineData(nameof(ControllerType.GetEvacuationRouteByIdAsync))]
    [InlineData(nameof(ControllerType.ListEvacuationRoutesByZoneAsync))]
    [InlineData(nameof(ControllerType.ListActiveObstructionsAsync))]
    [InlineData(nameof(ControllerType.GetOccupancyByZoneAsync))]
    [InlineData(nameof(ControllerType.GetLatestEgressSimulationAsync))]
    [InlineData(nameof(ControllerType.GetBottleneckHistoryAsync))]
    public void ReadActions_RequireReadTier(string methodName)
    {
        var method = typeof(ControllerType).GetMethod(methodName)!;
        var attr = method.GetCustomAttribute<AuthorizeAttribute>();

        attr!.Policy.Should().Be(SafetyTierPolicies.Read);
    }

    [Fact]
    public void ControllerRoute_IsScopedUnderDigitalTwinEvacuationModeling()
    {
        var routeAttr = typeof(ControllerType).GetCustomAttribute<RouteAttribute>();

        routeAttr.Should().NotBeNull();
        routeAttr!.Template.Should().Be("api/v{version:apiVersion}/digitaltwin/evacuation-modeling",
            "the route template is part of the module's stable API surface and must not silently drift");
    }

    [Fact]
    public void AggregateIdentifiers_AreAlwaysBoundFromTheRoute_NeverAcceptedFromTheRequestBody()
    {
        // Security guard: request DTOs for actions that mutate an existing aggregate must never
        // themselves carry the aggregate's own identifier -- it must be bound from the route
        // (e.g. {evacuationRouteId:guid}), preventing a caller from mutating an aggregate other
        // than the one named in the URL.
        var recordObstructionRequestType = typeof(PlantSafety.Api.DigitalTwin.EvacuationModeling.Contracts.Requests.RecordRouteObstructionRequest);
        recordObstructionRequestType.GetProperty("EvacuationRouteId").Should().BeNull(
            "RecordRouteObstructionRequest must not carry EvacuationRouteId -- it is bound from the route segment");

        var recordOccupancyRequestType = typeof(PlantSafety.Api.DigitalTwin.EvacuationModeling.Contracts.Requests.RecordOccupancySnapshotRequest);
        recordOccupancyRequestType.GetProperty("EvacuationZoneId").Should().BeNull(
            "RecordOccupancySnapshotRequest must not carry EvacuationZoneId -- it is bound from the route segment");
    }
}
