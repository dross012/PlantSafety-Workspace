// ============================================================================
//  File:           AlertCategoryPolicy.cs
//  Created:        2026-05-26
//  Description:
//      Domain policy defining category-specific rules for Alerts.
//      Categories include: Safety, Environmental, Equipment, Compliance.
// ============================================================================


// ============================================================================
//  File:           AlertCategoryPolicy.cs
//  Created:        2026-05-26
//  Description:
//      Domain policy defining category-specific rules for Alerts.
//      Categories include: Safety, Environmental, Equipment, Compliance.
// ============================================================================

namespace PlantSafety.Domain.Operational.Alerts.Policies;

public sealed class AlertCategoryPolicy
{
    public bool IsValidCategory(AlertCategory category)
        => category.Code is "Safety" or "Environmental" or "Equipment" or "Compliance";

    public bool RequiresImmediateReview(AlertCategory category)
        => category.Code is "Safety" or "Environmental";
}
