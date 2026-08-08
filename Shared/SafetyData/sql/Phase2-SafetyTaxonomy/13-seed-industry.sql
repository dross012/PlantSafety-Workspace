-- ============================================================================
--  Project:        Shared.SafetyData
--  Module:         Phase2-SafetyTaxonomy
--  Layer:          Infrastructure - SQL Scripts
--  File:           13-seed-industry.sql
--  Created:        2026-08-08
--  Author:         Platform Team
--  SchemaVersion:  2026.1
--  Description:    Seeds [SafetyTaxonomy].[Industry] with all 40 industries
--                  from IndustryRegistry.All, verbatim, in source taxonomy
--                  order (IndustryId 1-40). Name and TopLevelCategory are
--                  copied exactly, including the two names that use the
--                  Unicode non-breaking hyphen (U+2011), not a plain ASCII
--                  hyphen, in the C# source: "E{U+2011}commerce Fulfillment"
--                  and "Third{U+2011}Party Logistics".
--
--  SESSION CONTEXT: see 10-seed-safetyflag.sql's header -- same pattern.
--  Idempotent: MERGE keyed on IndustryId. Safe to re-run.
-- ============================================================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

EXEC sys.sp_set_session_context @key = N'SafetyTaxonomySeedContext', @value = 1;
GO

MERGE [SafetyTaxonomy].[Industry] AS target
USING (VALUES
    (1,  N'Chemical Manufacturing',                    N'Core Manufacturing'),
    (2,  N'Petrochemical & Refining',                  N'Core Manufacturing'),
    (3,  N'Pharmaceuticals & Biotech',                 N'Core Manufacturing'),
    (4,  N'Food & Beverage',                            N'Core Manufacturing'),
    (5,  N'Automotive & Heavy Equipment',               N'Core Manufacturing'),
    (6,  N'Aerospace & Defense',                        N'Core Manufacturing'),
    (7,  N'Electronics & Semiconductor',                N'Core Manufacturing'),
    (8,  N'Plastics & Rubber',                           N'Core Manufacturing'),
    (9,  N'Metal Fabrication & Foundries',              N'Core Manufacturing'),
    (10, N'Wood Products & Furniture',                  N'Core Manufacturing'),
    (11, N'Paper & Pulp',                                N'Core Manufacturing'),
    (12, N'Textiles & Apparel',                          N'Core Manufacturing'),
    (13, N'Warehousing & Distribution',                 N'Logistics & Transport'),
    (14, N'Cold Storage',                                N'Logistics & Transport'),
    (15, N'E‑commerce Fulfillment',                     N'Logistics & Transport'),
    (16, N'Third‑Party Logistics',                      N'Logistics & Transport'),
    (17, N'Bulk Material Handling',                     N'Logistics & Transport'),
    (18, N'Explosives & Pyrotechnics',                  N'High Fire/Explosion Risk'),
    (19, N'Ammunition & Firearms',                      N'High Fire/Explosion Risk'),
    (20, N'Paints, Coatings, Adhesives',                N'High Fire/Explosion Risk'),
    (21, N'Printing & Ink',                              N'High Fire/Explosion Risk'),
    (22, N'Hazardous Waste Treatment',                  N'High Chemical Hazard'),
    (23, N'Chemical Blending / Tolling',                N'High Chemical Hazard'),
    (24, N'Industrial Cleaning Chemical Production',    N'High Chemical Hazard'),
    (25, N'Mining & Mineral Processing',                N'Heavy Machinery / Industrial'),
    (26, N'Cement & Concrete',                           N'Heavy Machinery / Industrial'),
    (27, N'Steel Mills & Rolling',                       N'Heavy Machinery / Industrial'),
    (28, N'Heavy Industrial Fabrication',                N'Heavy Machinery / Industrial'),
    (29, N'Marine Terminals',                            N'Water/Marine Interface'),
    (30, N'Bulk Liquid Transfer',                        N'Water/Marine Interface'),
    (31, N'Waterfront Manufacturing',                    N'Water/Marine Interface'),
    (32, N'Power Generation',                            N'High Environmental Compliance'),
    (33, N'Renewable Energy Manufacturing',              N'High Environmental Compliance'),
    (34, N'Wastewater Treatment',                        N'High Environmental Compliance'),
    (35, N'Industrial Laundries',                        N'High Environmental Compliance'),
    (36, N'Medical Devices',                             N'Specialized'),
    (37, N'Cosmetics',                                   N'Specialized'),
    (38, N'Agricultural Processing',                     N'Specialized'),
    (39, N'Tobacco',                                     N'Specialized'),
    (40, N'Cannabis',                                    N'Specialized')
) AS source ([IndustryId], [Name], [TopLevelCategory])
ON target.[IndustryId] = source.[IndustryId]
WHEN NOT MATCHED BY TARGET THEN
    INSERT ([IndustryId], [Name], [TopLevelCategory])
    VALUES (source.[IndustryId], source.[Name], source.[TopLevelCategory])
WHEN MATCHED THEN
    UPDATE SET
        target.[Name]              = source.[Name],
        target.[TopLevelCategory]  = source.[TopLevelCategory];
GO

EXEC sys.sp_set_session_context @key = N'SafetyTaxonomySeedContext', @value = 0;
GO
