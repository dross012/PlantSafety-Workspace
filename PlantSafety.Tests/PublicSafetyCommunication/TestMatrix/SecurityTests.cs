// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.PublicSafetyCommunication.TestMatrix
//  Layer:          Tests (Unit) -- Security
//  File:           SecurityTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    [PLNT-SEC-PSC] Security test type (TRD-MAN-2026-001 Ch.3): asserts every action
//                  on PublicSafetyCommunicationV1Controller requires authentication and carries an
//                  explicit authorization policy (no anonymous/unguarded endpoints), that
//                  write/approve-tier actions use the correct SafetyTierPolicies, and that the
//                  immutable CommunicationLogEntry aggregate is not writable via any command other
//                  than LogCommunicationEntryCommand (no Update/Delete command exists for it --
//                  the PLNT-SEC-IMMUTABLE-001 guarantee). Isolated, deterministic, no I/O.
// ============================================================================
using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Authorization.Abstractions.Policies;
using ControllerType = PlantSafety.Api.PublicSafetyCommunication.Controllers.V1.PublicSafetyCommunicationV1Controller;

namespace PlantSafety.Tests.PublicSafetyCommunication.TestMatrix;

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

    [Fact]
    public void BroadcastPublicAlertAsync_RequiresApproveTier_BecauseItIsAPublicFacingBroadcast()
    {
        var method = typeof(ControllerType).GetMethod(nameof(ControllerType.BroadcastPublicAlertAsync))!;
        var attr = method.GetCustomAttribute<AuthorizeAttribute>();

        attr!.Policy.Should().Be(SafetyTierPolicies.Approve,
            "sending a public-facing alert broadcast must require the highest write tier, not the ordinary Write tier used by internal-only commands");
    }

    [Theory]
    [InlineData(nameof(ControllerType.RegisterPublicSafetyAgencyAsync))]
    [InlineData(nameof(ControllerType.DispatchIncidentNotificationAsync))]
    [InlineData(nameof(ControllerType.AcknowledgeIncidentNotificationAsync))]
    [InlineData(nameof(ControllerType.CloseIncidentNotificationAsync))]
    [InlineData(nameof(ControllerType.LogCommunicationEntryAsync))]
    public void WriteActions_RequireWriteTier(string methodName)
    {
        var method = typeof(ControllerType).GetMethod(methodName)!;
        var attr = method.GetCustomAttribute<AuthorizeAttribute>();

        attr!.Policy.Should().Be(SafetyTierPolicies.Write);
    }

    [Theory]
    [InlineData(nameof(ControllerType.ListPublicSafetyAgenciesAsync))]
    [InlineData(nameof(ControllerType.GetIncidentNotificationStatusAsync))]
    [InlineData(nameof(ControllerType.ListActiveBroadcastsAsync))]
    [InlineData(nameof(ControllerType.GetCommunicationLogByIncidentAsync))]
    public void ReadActions_RequireReadTier(string methodName)
    {
        var method = typeof(ControllerType).GetMethod(methodName)!;
        var attr = method.GetCustomAttribute<AuthorizeAttribute>();

        attr!.Policy.Should().Be(SafetyTierPolicies.Read);
    }

    [Fact]
    public void NoCommandExistsThatCanMutateOrDeleteACommunicationLogEntry()
    {
        // PLNT-SEC-IMMUTABLE-001 guarantee: the only command that touches CommunicationLogEntry is
        // LogCommunicationEntryCommand (a pure create/append). No Update/Delete/Correct/Amend
        // command class exists anywhere in the Application layer for this aggregate.
        var commandTypes = typeof(global::PlantSafety.Application.PublicSafetyCommunication.Commands.LogCommunicationEntryCommand).Assembly
            .GetTypes()
            .Where(t => t.Namespace == "PlantSafety.Application.PublicSafetyCommunication.Commands")
            .Where(t => t.Name.Contains("CommunicationLogEntry", StringComparison.Ordinal) || t.Name.Contains("CommunicationLog", StringComparison.Ordinal))
            .Select(t => t.Name)
            .ToList();

        commandTypes.Should().OnlyContain(n =>
            n == "LogCommunicationEntryCommand" ||
            n == "LogCommunicationEntryCommandHandler" ||
            n == "CommunicationLogEntryActionResult",
            "no Update/Delete command may ever be introduced for the immutable CommunicationLogEntry aggregate");
    }
}
