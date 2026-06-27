// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Common.ValueObjects
//  Layer:          Tests (Unit)
//  File:           PerformedByActorTests.cs
//  Created:        2026-06-17
//  SchemaVersion:  2026.1
//  Description:    Unit tests for PerformedByActor SharedKernel value type.
// ============================================================================
namespace PlantSafety.Tests.Common.ValueObjects;

public sealed class PerformedByActorTests : UnitTestBase
{
    [Fact]
    public void User_ShouldCreateActorWithUserKind()
    {
        var actor = PerformedByActor.User(TestUserId);
        actor.Kind.Should().Be(ActorKind.User);
    }

    [Fact]
    public void User_ShouldPreserveUserId()
    {
        var actor = PerformedByActor.User(TestUserId);
        actor.UserId.Should().Be(TestUserId);
    }

    [Fact]
    public void System_ShouldCreateActorWithSystemKind()
    {
        var actor = PerformedByActor.System();
        actor.Kind.Should().Be(ActorKind.System);
    }

    [Fact]
    public void TwoUserActors_WithSameId_ShouldBeEqual()
    {
        var a1 = PerformedByActor.User(TestUserId);
        var a2 = PerformedByActor.User(TestUserId);
        a1.Should().Be(a2);
    }

    [Fact]
    public void UserActor_AndSystemActor_ShouldNotBeEqual()
    {
        var user = PerformedByActor.User(TestUserId);
        var system = PerformedByActor.System();
        user.Should().NotBe(system);
    }
}
