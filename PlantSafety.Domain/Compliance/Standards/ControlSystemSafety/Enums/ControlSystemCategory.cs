// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Standards.ControlSystemSafety
//  Layer:          Domain
//  File:           ControlSystemCategory.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    ISO 13849-1 Category classification for the structure of a safety-related part
//                  of a control system. Category B is the basic state-of-the-art category;
//                  Category 1 uses tried-and-tested components/principles; Category 2 introduces
//                  automatic or manual checking at intervals; Categories 3 and 4 add redundancy
//                  and fault-detection (well-known industry knowledge -- this codebase does not
//                  fabricate the precise Category 3/4 architectural definitions beyond citing
//                  ISO 13849-1 generally for them). Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.Standards.ControlSystemSafety.Enums;

/// <summary>ISO 13849-1 Category classification, CategoryB (basic) through Category4 (redundant, fault-detecting).</summary>
public enum ControlSystemCategory
{
    CategoryB = 0,
    Category1 = 1,
    Category2 = 2,
    Category3 = 3,
    Category4 = 4,
}
