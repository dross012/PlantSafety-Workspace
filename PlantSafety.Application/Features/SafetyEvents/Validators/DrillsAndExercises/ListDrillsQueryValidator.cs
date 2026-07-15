// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-15
// FILE DESCRIPTION: FluentValidation validator for ListDrillsQuery.
// SchemaVersion: 1

using FluentValidation;
using PlantSafety.Application.Common.Constants;
using PlantSafety.Application.Features.SafetyEvents.Queries.DrillsAndExercises.List;

namespace PlantSafety.Application.Features.SafetyEvents.Validators.DrillsAndExercises;

/// <summary>
/// Validates <see cref="ListDrillsQuery"/> before the handler executes.
/// </summary>
public sealed class ListDrillsQueryValidator : AbstractValidator<ListDrillsQuery>
{
    private static readonly string[] ValidSortFields = ["ScheduledAtUtc", "Status", "Title"];

    private static readonly string[] ValidDirections = ["ASC", "DESC"];

    /// <summary>Initialises all validation rules.</summary>
    public ListDrillsQueryValidator()
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
    }
}
