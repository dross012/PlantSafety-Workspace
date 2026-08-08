-- ============================================================================
--  Project:        Shared.SafetyData
--  Module:         Phase2-SafetyTaxonomy
--  Layer:          Infrastructure - SQL Scripts
--  File:           11-seed-regulatorcategory.sql
--  Created:        2026-08-08
--  Author:         Platform Team
--  SchemaVersion:  2026.1
--  Description:    Seeds [SafetyTaxonomy].[RegulatorCategory] with all 9
--                  categories from RegulatorCategory.cs, verbatim
--                  (declaration order).
--
--  SESSION CONTEXT: see 10-seed-safetyflag.sql's header -- same pattern.
--  Idempotent: MERGE keyed on RegulatorCategoryId. Safe to re-run.
-- ============================================================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

EXEC sys.sp_set_session_context @key = N'SafetyTaxonomySeedContext', @value = 1;
GO

MERGE [SafetyTaxonomy].[RegulatorCategory] AS target
USING (VALUES
    (1, N'OSHA'),
    (2, N'EPA'),
    (3, N'NFPA'),
    (4, N'PHMSA'),
    (5, N'FDA'),
    (6, N'ATF'),
    (7, N'MSHA'),
    (8, N'USCG'),
    (9, N'DHS_CFATS')
) AS source ([RegulatorCategoryId], [CategoryName])
ON target.[RegulatorCategoryId] = source.[RegulatorCategoryId]
WHEN NOT MATCHED BY TARGET THEN
    INSERT ([RegulatorCategoryId], [CategoryName])
    VALUES (source.[RegulatorCategoryId], source.[CategoryName])
WHEN MATCHED THEN
    UPDATE SET target.[CategoryName] = source.[CategoryName];
GO

EXEC sys.sp_set_session_context @key = N'SafetyTaxonomySeedContext', @value = 0;
GO
