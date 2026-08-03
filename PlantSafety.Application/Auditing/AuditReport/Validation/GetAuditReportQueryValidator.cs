// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-02
// FILE DESCRIPTION: FluentValidation validators for the AuditReport queries.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.Auditing.AuditReport.Validation;

/// <summary>Validates <see cref="Queries.GetAuditReportQuery"/>.</summary>
public sealed class GetAuditReportQueryValidator : AbstractValidator<Queries.GetAuditReportQuery>
{
    /// <summary>Defines validation rules for <see cref="Queries.GetAuditReportQuery"/>.</summary>
    public GetAuditReportQueryValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.AuditReportId).NotEmpty();
    }
}

/// <summary>Validates <see cref="Queries.GetAuditReportHistoryQuery"/>.</summary>
public sealed class GetAuditReportHistoryQueryValidator : AbstractValidator<Queries.GetAuditReportHistoryQuery>
{
    /// <summary>Defines validation rules for <see cref="Queries.GetAuditReportHistoryQuery"/>.</summary>
    public GetAuditReportHistoryQueryValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.TargetEntityType).NotEmpty().MaximumLength(200);
        RuleFor(x => x.TargetEntityId).NotEmpty();
    }
}

/// <summary>Validates <see cref="Queries.GetAuditReportContentQuery"/>.</summary>
public sealed class GetAuditReportContentQueryValidator : AbstractValidator<Queries.GetAuditReportContentQuery>
{
    /// <summary>Defines validation rules for <see cref="Queries.GetAuditReportContentQuery"/>.</summary>
    public GetAuditReportContentQueryValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.AuditReportId).NotEmpty();
    }
}
