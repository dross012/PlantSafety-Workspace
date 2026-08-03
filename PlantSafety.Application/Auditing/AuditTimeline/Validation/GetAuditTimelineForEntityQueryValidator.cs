// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-02
// FILE DESCRIPTION: FluentValidation validator for GetAuditTimelineForEntityQuery.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.Auditing.AuditTimeline.Validation;

/// <summary>Validates <see cref="Queries.GetAuditTimelineForEntityQuery"/>.</summary>
public sealed class GetAuditTimelineForEntityQueryValidator : AbstractValidator<Queries.GetAuditTimelineForEntityQuery>
{
    /// <summary>Defines validation rules for <see cref="Queries.GetAuditTimelineForEntityQuery"/>.</summary>
    public GetAuditTimelineForEntityQueryValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.EntityType).NotEmpty().MaximumLength(200);
        RuleFor(x => x.EntityId).NotEmpty();
    }
}
