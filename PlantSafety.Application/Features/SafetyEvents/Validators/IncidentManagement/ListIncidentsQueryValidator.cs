// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: FluentValidation validator for ListIncidentsQuery.
// SchemaVersion: 1

using FluentValidation;
using PlantSafety.Application.Common.Constants;
using PlantSafety.Application.Features.SafetyEvents.Queries.IncidentManagement.List;

namespace PlantSafety.Application.Features.SafetyEvents.Validators.IncidentManagement;

/// <summary>
/// Validates <see cref="ListIncidentsQuery"/> before the handler executes.
/// </summary>
public sealed class ListIncidentsQueryValidator : AbstractValidator<ListIncidentsQuery>
{
    private static readonly string[] ValidSortFields =
        ["IncidentOccurredAtUtc", "ReportedAtUtc", "Severity", "Status", "Title", "Location"];

    private static readonly string[] ValidDirections = ["ASC", "DESC"];

    /// <summary>Initialises all validation rules.</summary>
    public ListIncidentsQueryValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty().WithMessage("TenantId is required.");

        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(PaginationConstants.MinPageSize)
            .WithMessage($"PageNumber must be >= {PaginationConstants.MinPageSize}.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(PaginationConstants.MinPageSize, PaginationConstants.MaxPageSize)
            .WithMessage($"PageSize must be between {PaginationConstants.MinPageSize} and {PaginationConstants.MaxPageSize}.");

        RuleFor(x => x.OrderBy)
            .Must(f => ValidSortFields.Contains(f, StringComparer.Ordinal))
            .WithMessage($"OrderBy must be one of: {string.Join(", ", ValidSortFields)}.");

        RuleFor(x => x.SortDirection)
            .Must(d => ValidDirections.Contains(d, StringComparer.OrdinalIgnoreCase))
            .WithMessage("SortDirection must be ASC or DESC.");

        When(x => x.OccurredFromUtc.HasValue && x.OccurredToUtc.HasValue, () =>
            RuleFor(x => x.OccurredToUtc!.Value)
                .GreaterThanOrEqualTo(x => x.OccurredFromUtc!.Value)
                .WithMessage("OccurredToUtc must be on or after OccurredFromUtc."));
    }
}
