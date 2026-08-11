// ============================================================================
//  Test IDs:  PLNT-PEN-TENANT-001..006
//  Attack:    Horizontal privilege escalation via a client-supplied TenantId
//             (OWASP A01, Broken Access Control).
//
//  Surface:   Roughly 148 request DTOs bind a TenantId from the query string
//             or body, and that value flows into the query used for data
//             filtering. Authorization, meanwhile, is evaluated against the
//             server-resolved tenant from the JWT. Nothing forced the two to
//             agree, so a caller authenticated as tenant A could ask for
//             tenant B's rows and be authorized for A while reading B.
//
//             TenantIsolationBehavior is the single choke point that closes
//             this centrally rather than in 148 separate DTOs. These tests
//             attack that choke point directly: every one of them supplies a
//             forged TenantId and passes only if the request is refused
//             BEFORE the handler runs.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging.Abstractions;

using PlantSafety.Application.Abstractions.Context;
using PlantSafety.Application.Abstractions.CurrentUser;
using PlantSafety.Application.Behaviors.Security;
using PlantSafety.Application.Common.Errors;

namespace PlantSafety.PenetrationTests.Tenancy;

public sealed class CrossTenantEscalationPenetrationTests
{
    private static readonly Guid VictimTenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid AttackerTenantId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    /// <summary>A request carrying a non-nullable TenantId, the dominant convention in this codebase.</summary>
    public sealed record ReadAccidentsQuery(Guid TenantId) : IRequest<string>;

    /// <summary>A request whose TenantId is optional, so the null path is exercised too.</summary>
    public sealed record ReadOptionalQuery(Guid? TenantId) : IRequest<string>;

    /// <summary>A request with no tenant binding at all: the behavior must let it through untouched.</summary>
    public sealed record ReadUnscopedQuery(string Filter) : IRequest<string>;

    private const string HandlerReached = "handler-reached";

    private static TenantIsolationBehavior<TRequest, string> BuildBehavior<TRequest>(Guid resolvedTenantId)
        where TRequest : notnull
    {
        var operationContext = Substitute.For<IOperationContext>();
        operationContext.TenantId.Returns(resolvedTenantId);
        operationContext.CorrelationId.Returns(Guid.NewGuid());

        var currentUser = Substitute.For<ICurrentUserService>();
        currentUser.IsAuthenticated.Returns(true);
        currentUser.UserId.Returns(Guid.NewGuid());
        currentUser.TenantId.Returns(resolvedTenantId);

        return new TenantIsolationBehavior<TRequest, string>(
            operationContext,
            currentUser,
            NullLogger<TenantIsolationBehavior<TRequest, string>>.Instance);
    }

    [Fact(DisplayName = "PLNT-PEN-TENANT-001: A forged TenantId for another tenant is refused before the handler runs")]
    public async Task ForgedTenantId_ShouldBeRefusedBeforeTheHandlerExecutes()
    {
        // The attacker is authenticated as their own tenant and asks for the victim's.
        var behavior = BuildBehavior<ReadAccidentsQuery>(resolvedTenantId: AttackerTenantId);
        var handlerRan = false;

        var act = async () => await behavior.Handle(
            new ReadAccidentsQuery(VictimTenantId),
            _ => { handlerRan = true; return Task.FromResult(HandlerReached); },
            TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<ApplicationAuthorizationException>(
            "a request for a tenant other than the authenticated one is a cross-tenant read attempt");

        // The stronger half of the assertion. A control that throws only AFTER the handler has
        // already queried and returned the victim's rows has not prevented the breach -- it has
        // merely declined to render it.
        handlerRan.Should().BeFalse("the handler must never execute for a cross-tenant request");
    }

    [Fact(DisplayName = "PLNT-PEN-TENANT-002: The refusal is a tenant-mismatch authorization failure, not an incidental error")]
    public async Task ForgedTenantId_ShouldFailWithTheTenantMismatchCode()
    {
        var behavior = BuildBehavior<ReadAccidentsQuery>(resolvedTenantId: AttackerTenantId);

        var thrown = await behavior.Invoking(b => b.Handle(
                new ReadAccidentsQuery(VictimTenantId),
                _ => Task.FromResult(HandlerReached),
                TestContext.Current.CancellationToken))
            .Should().ThrowAsync<ApplicationAuthorizationException>();

        // Without this, the test would still pass if the behavior were deleted and the request
        // happened to fail for some unrelated reason -- a false sense of coverage.
        thrown.Which.Error.ErrorCode.Should().Be(ErrorCodes.TenantMismatch,
            "the rejection must be attributable to the tenant check specifically");

        // The refusal must not become an oracle. A message echoing either tenant id would confirm
        // to the attacker that the tenant they guessed exists, turning a blocked read into a
        // working enumeration primitive -- the ids belong in the server-side warning log, which is
        // where the behavior puts them, and not in anything the caller receives.
        thrown.Which.Message.Should().NotContain(VictimTenantId.ToString())
            .And.NotContain(AttackerTenantId.ToString(),
                "the response must not confirm which tenant ids exist");
    }

    [Fact(DisplayName = "PLNT-PEN-TENANT-003: An all-zeroes TenantId is not treated as a wildcard")]
    public async Task EmptyGuidTenantId_ShouldNotBypassTheCheck()
    {
        var behavior = BuildBehavior<ReadAccidentsQuery>(resolvedTenantId: AttackerTenantId);

        // Guid.Empty is the classic probe: implementations that treat "no tenant specified" as
        // "all tenants" turn it into a full cross-tenant read. Here it is simply a value that
        // does not equal the resolved tenant, and must be refused like any other.
        var act = async () => await behavior.Handle(
            new ReadAccidentsQuery(Guid.Empty),
            _ => Task.FromResult(HandlerReached),
            TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<ApplicationAuthorizationException>(
            "an all-zeroes tenant id must not be interpreted as 'any tenant'");
    }

    [Fact(DisplayName = "PLNT-PEN-TENANT-004: A forged TenantId on a nullable property is refused too")]
    public async Task ForgedNullableTenantId_ShouldBeRefused()
    {
        var behavior = BuildBehavior<ReadOptionalQuery>(resolvedTenantId: AttackerTenantId);

        // Guid? boxes to the same runtime type as Guid when it has a value, so a nullable binding
        // is not an escape hatch -- but that is a property of the reflection code, not something a
        // reader can assume, so it is asserted rather than trusted.
        var act = async () => await behavior.Handle(
            new ReadOptionalQuery(VictimTenantId),
            _ => Task.FromResult(HandlerReached),
            TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<ApplicationAuthorizationException>(
            "declaring TenantId as nullable must not weaken the isolation check");
    }

    [Fact(DisplayName = "PLNT-PEN-TENANT-005: A request for the caller's own tenant is allowed through")]
    public async Task MatchingTenantId_ShouldReachTheHandler()
    {
        var behavior = BuildBehavior<ReadAccidentsQuery>(resolvedTenantId: VictimTenantId);

        var result = await behavior.Handle(
            new ReadAccidentsQuery(VictimTenantId),
            _ => Task.FromResult(HandlerReached),
            TestContext.Current.CancellationToken);

        // The control has to be usable, not just strict. A behavior that rejected everything would
        // pass every test above while breaking the product, and this is what distinguishes them.
        result.Should().Be(HandlerReached, "a caller's request for their own tenant is legitimate traffic");
    }

    [Fact(DisplayName = "PLNT-PEN-TENANT-006: A request with no TenantId binding is passed through unchanged")]
    public async Task RequestWithoutTenantBinding_ShouldPassThrough()
    {
        var behavior = BuildBehavior<ReadUnscopedQuery>(resolvedTenantId: VictimTenantId);

        var result = await behavior.Handle(
            new ReadUnscopedQuery("severity=high"),
            _ => Task.FromResult(HandlerReached),
            TestContext.Current.CancellationToken);

        // Documents the boundary of this control honestly: it constrains a client-supplied
        // TenantId, and a request that supplies none is out of its remit -- such a request is
        // scoped by the handler using the server-resolved context instead. Read together with
        // PLNT-PEN-TENANT-001, this says exactly what the behavior does and does not defend.
        result.Should().Be(HandlerReached,
            "requests that bind no TenantId are scoped from the server-resolved context, not by this behavior");
    }
}
