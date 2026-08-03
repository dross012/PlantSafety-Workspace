// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Enterprise.Integrations.Hr
//  Layer:          Application
//  File:           HrUserProvisioningInboundTranslator.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 6 §1.4 Enterprise System Integrations, HR/Identity)
//  SchemaVersion:  2026.1
//  Description:    Translates an inbound HR user-provisioning event ("hr.user.provisioned.v1") into
//                  the platform's real, public
//                  Features.EnterpriseFabric.Commands.Users.Create.CreateUserCommand -- verified as
//                  the correct, real entry point (CrossDomain.Users is a projection-only bounded
//                  context per Handbook Part XXII with query-only commands, so it is NOT the correct
//                  target; EnterpriseFabric.Users.Create is the actual, real user-write surface).
//                  DEACTIVATION and standalone role-reassignment are explicitly NOT handled here:
//                  IUserRepository (the real repository backing CreateUserCommand) exposes only
//                  CreateAsync/GetByIdAsync/ListAsync -- no DeactivateAsync or UpdateRolesAsync method
//                  exists yet. An inbound "hr.user.deactivated.v1"/"hr.user.rolechanged.v1" event is
//                  therefore gracefully rejected with a clear, actionable error rather than silently
//                  dropped or fabricated against a non-existent command (flagged in this Phase's
//                  report as the concrete follow-up: add DeactivateUserCommand/UpdateUserRolesCommand
//                  + repository methods, then extend this translator). Additive-only evolution.
// ============================================================================
using System.Text.Json;
using MediatR;
using PlantSafety.Application.Features.EnterpriseFabric.Commands.Users.Create;
using PlantSafety.SharedKernel.Contracts.Enterprise.EventMesh;
using PlantSafety.SharedKernel.Primitives.Errors;
using PlantSafety.SharedKernel.Primitives.Results;

namespace PlantSafety.Application.Enterprise.Integrations.Hr.Translators;

/// <summary>Translates an inbound HR user-provisioning event into <see cref="CreateUserCommand"/>.</summary>
public sealed class HrUserProvisioningInboundTranslator(ISender sender) : IEnterpriseInboundEventTranslator
{
    /// <summary>The canonical EventType this translator handles.</summary>
    public const string HandledEventType = "hr.user.provisioned.v1";

    /// <summary>
    /// EventTypes this translator recognises the shape of but does not yet support dispatching, given
    /// no real command surface exists (see file header). Recognised so a clear, actionable error is
    /// returned instead of a generic "no translator registered" outcome.
    /// </summary>
    private static readonly string[] KnownUnsupportedEventTypes =
    [
        "hr.user.deactivated.v1",
        "hr.user.rolechanged.v1",
    ];

    /// <inheritdoc/>
    public bool CanHandle(EnterpriseEventEnvelope envelope) =>
        envelope.SourceSystem == EnterpriseSourceSystem.Hr &&
        (string.Equals(envelope.EventType, HandledEventType, StringComparison.Ordinal) ||
         KnownUnsupportedEventTypes.Contains(envelope.EventType, StringComparer.Ordinal));

    /// <inheritdoc/>
    public async Task<Result<bool>> TranslateAndDispatchAsync(EnterpriseEventEnvelope envelope, CancellationToken cancellationToken)
    {
        if (!string.Equals(envelope.EventType, HandledEventType, StringComparison.Ordinal))
        {
            return Result<bool>.Failure(new Error(
                ErrorCode.Create("enterprise.eventmesh.notsupported"),
                ErrorType.Validation,
                $"HR EventType '{envelope.EventType}' is recognised but not yet supported: no real " +
                "DeactivateUserCommand/UpdateUserRolesCommand exists on the EnterpriseFabric.Users " +
                "write surface. Add the command + IUserRepository method, then extend this translator."));
        }

        var payload = envelope.Payload;

        if (!TryGetString(payload, "firstName", out var firstName))
            return InvalidPayload("'firstName'");

        if (!TryGetString(payload, "lastName", out var lastName))
            return InvalidPayload("'lastName'");

        if (!TryGetString(payload, "email", out var email))
            return InvalidPayload("'email'");

        var roles = payload.TryGetProperty("roles", out var rolesElement) && rolesElement.ValueKind == JsonValueKind.Array
            ? rolesElement.EnumerateArray().Where(e => e.ValueKind == JsonValueKind.String).Select(e => e.GetString()!).ToList()
            : new List<string>();

        var siteIds = payload.TryGetProperty("siteIds", out var siteIdsElement) && siteIdsElement.ValueKind == JsonValueKind.Array
            ? siteIdsElement.EnumerateArray().Where(e => e.TryGetGuid(out _)).Select(e => e.GetGuid()).ToList()
            : new List<Guid>();

        var command = new CreateUserCommand
        {
            CommandId = Guid.NewGuid(),
            CorrelationId = envelope.CorrelationId,
            CausationId = envelope.CausationId ?? envelope.EventId,
            RequestedAtUtc = DateTime.UtcNow,
            ApiVersionToken = "v1",
            TenantId = envelope.TenantId,
            ActorId = envelope.ActorId ?? Guid.Empty,
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            Roles = roles,
            SiteIds = siteIds,
        };

        try
        {
            await sender.Send(command, cancellationToken);
            return Result<bool>.Success(true);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return Result<bool>.Failure(new Error(
                ErrorCode.Create("enterprise.eventmesh.hruserprovisioningfailed"),
                ErrorType.Failure,
                "User provisioning failed. See server logs for details."));
        }
    }

    private static bool TryGetString(JsonElement payload, string propertyName, out string value)
    {
        value = string.Empty;
        if (!payload.TryGetProperty(propertyName, out var element) || element.ValueKind != JsonValueKind.String)
            return false;

        var raw = element.GetString();
        if (string.IsNullOrWhiteSpace(raw))
            return false;

        value = raw;
        return true;
    }

    private static Result<bool> InvalidPayload(string missingField) =>
        Result<bool>.Failure(new Error(
            ErrorCode.Create("enterprise.eventmesh.invalidpayload"),
            ErrorType.Validation,
            $"HR user-provisioning payload is missing a valid {missingField}."));
}
