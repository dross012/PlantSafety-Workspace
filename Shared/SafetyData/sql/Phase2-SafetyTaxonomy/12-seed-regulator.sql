-- ============================================================================
--  Project:        Shared.SafetyData
--  Module:         Phase2-SafetyTaxonomy
--  Layer:          Infrastructure - SQL Scripts
--  File:           12-seed-regulator.sql
--  Created:        2026-08-08
--  Author:         Platform Team
--  SchemaVersion:  2026.1
--  Description:    Seeds [SafetyTaxonomy].[Regulator] with all 15 regulators
--                  from Regulator.cs, verbatim (declaration order, XML-doc
--                  summaries as Description, RegulatorExtensions.Code()
--                  output as Code, and RegulatorCategoryCatalog.ForRegulator
--                  output as RegulatorCategoryId).
--
--  SESSION CONTEXT: see 10-seed-safetyflag.sql's header -- same pattern.
--  Idempotent: MERGE keyed on RegulatorId. Safe to re-run.
-- ============================================================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

EXEC sys.sp_set_session_context @key = N'SafetyTaxonomySeedContext', @value = 1;
GO

MERGE [SafetyTaxonomy].[Regulator] AS target
USING (VALUES
    (1,  N'OSHA',      N'OSHA',      N'Occupational Safety and Health Administration.',                                  1),
    (2,  N'EPA',       N'EPA',       N'U.S. Environmental Protection Agency (general).',                                 2),
    (3,  N'EPA_RCRA',  N'EPA-RCRA',  N'EPA -- Resource Conservation and Recovery Act.',                                  2),
    (4,  N'EPA_TSCA',  N'EPA-TSCA',  N'EPA -- Toxic Substances Control Act.',                                            2),
    (5,  N'PHMSA',     N'PHMSA',     N'Pipeline and Hazardous Materials Safety Administration.',                         4),
    (6,  N'NFPA',      N'NFPA',      N'National Fire Protection Association.',                                          3),
    (7,  N'NFPA_Dust', N'NFPA-Dust', N'NFPA combustible-dust standards.',                                                3),
    (8,  N'FMCSA',     N'FMCSA',     N'Federal Motor Carrier Safety Administration.',                                    1),
    (9,  N'DHS_CFATS', N'DHS-CFATS', N'Department of Homeland Security -- Chemical Facility Anti-Terrorism Standards.',  9),
    (10, N'USCG',      N'USCG',      N'U.S. Coast Guard.',                                                               8),
    (11, N'FDA',       N'FDA',       N'Food and Drug Administration.',                                                   5),
    (12, N'ANSI_ISO',  N'ANSI-ISO',  N'ANSI/ISO consensus standards.',                                                   1),
    (13, N'ATF',       N'ATF',       N'Bureau of Alcohol, Tobacco, Firearms and Explosives.',                            6),
    (14, N'DoD',       N'DoD',       N'Department of Defense.',                                                          6),
    (15, N'MSHA',      N'MSHA',      N'Mine Safety and Health Administration.',                                         7)
) AS source ([RegulatorId], [EnumName], [Code], [Description], [RegulatorCategoryId])
ON target.[RegulatorId] = source.[RegulatorId]
WHEN NOT MATCHED BY TARGET THEN
    INSERT ([RegulatorId], [EnumName], [Code], [Description], [RegulatorCategoryId])
    VALUES (source.[RegulatorId], source.[EnumName], source.[Code], source.[Description], source.[RegulatorCategoryId])
WHEN MATCHED THEN
    UPDATE SET
        target.[EnumName]            = source.[EnumName],
        target.[Code]                = source.[Code],
        target.[Description]         = source.[Description],
        target.[RegulatorCategoryId] = source.[RegulatorCategoryId];
GO

EXEC sys.sp_set_session_context @key = N'SafetyTaxonomySeedContext', @value = 0;
GO
