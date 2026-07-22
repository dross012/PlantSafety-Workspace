// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Alerts.Alert
//  Layer:          Domain
//  File:           AlertsByCategorySpecification.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Concrete specification matching Alerts within a given catalog category.
//                  Additive-only evolution.
// ============================================================================
using AlertCategory = PlantSafety.Domain.Alerts.Alert.Enums.AlertCategory;

namespace PlantSafety.Domain.Alerts.Alert.Specifications;

/// <summary>Matches <see cref="Aggregates.Alert"/> aggregates whose category equals the given code.</summary>
public sealed class AlertsByCategorySpecification : AlertSpecification
{
    private readonly string _categoryCode;

    /// <summary>Creates a specification filtering by <paramref name="category"/>.</summary>
    public AlertsByCategorySpecification(AlertCategory category)
    {
        ArgumentNullException.ThrowIfNull(category);
        _categoryCode = category.Code;
    }

    /// <inheritdoc/>
    public override Expression<Func<Aggregates.Alert, bool>> ToExpression()
        => alert => alert.Category.Code == _categoryCode;
}
