// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-26
// FILE DESCRIPTION: Command to create a new Culture aggregate. Simple constructor-injection CQRS
//   shape (not the ICommand<Result<T>> envelope pattern) — matches this module's existing style.
// SchemaVersion: 1

using MediatR;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;

namespace PlantSafety.Application.Analytics.Culture.Commands;

/// <summary>Creates a new <see cref="Culture"/> aggregate under the Analytics.Culture bounded context.</summary>
/// <param name="TenantId">Tenant that owns the new culture record.</param>
/// <param name="UserId">Identity of the user creating the record (CRUD authorship).</param>
/// <param name="CorrelationId">Operation-group correlation identifier.</param>
/// <param name="CausationId">Identifier of the immediate parent command or event.</param>
/// <param name="Actor">Actor performing this domain action.</param>
public sealed record CreateCultureCommand(
    TenantId TenantId,
    UserId UserId,
    CorrelationId CorrelationId,
    CausationId CausationId,
    PerformedByActor Actor) : IRequest;
