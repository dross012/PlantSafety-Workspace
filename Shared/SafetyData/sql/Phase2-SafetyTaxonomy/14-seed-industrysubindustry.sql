-- ============================================================================
--  Project:        Shared.SafetyData
--  Module:         Phase2-SafetyTaxonomy
--  Layer:          Infrastructure - SQL Scripts
--  File:           14-seed-industrysubindustry.sql
--  Created:        2026-08-08
--  Author:         Platform Team
--  SchemaVersion:  2026.1
--  Description:    Seeds [SafetyTaxonomy].[IndustrySubIndustry] with every
--                  sub-industry string from IndustryRegistry.All's
--                  SubIndustries arrays, verbatim, in source order
--                  (OrdinalPosition). Only the 12 "Core Manufacturing"
--                  industries (IndustryId 1-12) enumerate sub-industries in
--                  the C# source; every other industry's SubIndustries list
--                  is IndustryRegistry.NoSubIndustries (empty), so this
--                  table has zero rows for IndustryId 13-40 -- not a gap, an
--                  exact mirror of the source.
--
--  SESSION CONTEXT: see 10-seed-safetyflag.sql's header -- same pattern.
--  Idempotent: MERGE keyed on (IndustryId, SubIndustryName).
-- ============================================================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

EXEC sys.sp_set_session_context @key = N'SafetyTaxonomySeedContext', @value = 1;
GO

MERGE [SafetyTaxonomy].[IndustrySubIndustry] AS target
USING (VALUES
    -- Chemical Manufacturing (1)
    (1, N'Bulk chemicals',       1),
    (1, N'solvents',             2),
    (1, N'resins',               3),
    (1, N'polymers',             4),
    (1, N'fertilizers',          5),
    (1, N'industrial gases',     6),
    -- Petrochemical & Refining (2)
    (2, N'Refineries',           1),
    (2, N'lubricants',           2),
    (2, N'fuels',                3),
    (2, N'asphalt',              4),
    -- Pharmaceuticals & Biotech (3)
    (3, N'Drug manufacturing',   1),
    (3, N'sterile processing',   2),
    (3, N'biologics',            3),
    -- Food & Beverage (4)
    (4, N'Meat',                 1),
    (4, N'dairy',                2),
    (4, N'beverage bottling',    3),
    (4, N'grain milling',        4),
    -- Automotive & Heavy Equipment (5)
    (5, N'Assembly',             1),
    (5, N'stamping',             2),
    (5, N'machining',            3),
    (5, N'robotics',             4),
    -- Aerospace & Defense (6)
    (6, N'Components',           1),
    (6, N'propulsion',           2),
    (6, N'composites',           3),
    (6, N'electronics',          4),
    -- Electronics & Semiconductor (7)
    (7, N'Chip fabs',            1),
    (7, N'PCB assembly',         2),
    (7, N'batteries',            3),
    -- Plastics & Rubber (8)
    (8, N'Extrusion',            1),
    (8, N'molding',              2),
    (8, N'compounding',          3),
    -- Metal Fabrication & Foundries (9)
    (9, N'Welding',              1),
    (9, N'machining',            2),
    (9, N'casting',              3),
    (9, N'forging',              4),
    -- Wood Products & Furniture (10)
    (10, N'Sawmills',            1),
    (10, N'cabinetry',           2),
    (10, N'furniture',           3),
    -- Paper & Pulp (11)
    (11, N'Pulp mills',          1),
    (11, N'paper plants',        2),
    -- Textiles & Apparel (12)
    (12, N'Weaving',             1),
    (12, N'dyeing',              2),
    (12, N'finishing',           3)
) AS source ([IndustryId], [SubIndustryName], [OrdinalPosition])
ON target.[IndustryId] = source.[IndustryId] AND target.[SubIndustryName] = source.[SubIndustryName]
WHEN NOT MATCHED BY TARGET THEN
    INSERT ([IndustryId], [SubIndustryName], [OrdinalPosition])
    VALUES (source.[IndustryId], source.[SubIndustryName], source.[OrdinalPosition])
WHEN MATCHED THEN
    UPDATE SET target.[OrdinalPosition] = source.[OrdinalPosition];
GO

EXEC sys.sp_set_session_context @key = N'SafetyTaxonomySeedContext', @value = 0;
GO
