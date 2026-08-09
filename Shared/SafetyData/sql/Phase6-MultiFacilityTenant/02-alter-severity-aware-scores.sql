-- ============================================================================
--  Project:        Shared.SafetyData -- Phase6-MultiFacilityTenant
--  File:           02-alter-severity-aware-scores.sql
--  Created:        2026-08-09
--  Description:    Additive ALTER: adds a SeverityCode FK column to
--                  [ComplianceRisk].[ComplianceScore] and
--                  [ComplianceRisk].[RiskScore] (Phase5), so both scores
--                  become severity-aware per Path C's task brief, without
--                  altering either table's existing PK/columns. References
--                  the canonical [Lookup].[SeverityLevel] table created in
--                  Shared.FacilityDomain's Phase1-Lookup (per this task's
--                  "one canonical lookup table per shared enum" rule) --
--                  NULL-able and defaulted to 'Minor' for backward
--                  compatibility with rows written before this column
--                  existed.
--  Idempotent: guarded by sys.columns existence checks. Safe to re-run.
--  Additive-only evolution.
-- ============================================================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('ComplianceRisk.ComplianceScore') AND name = 'SeverityCode')
BEGIN
    ALTER TABLE [ComplianceRisk].[ComplianceScore]
        ADD [SeverityCode] NVARCHAR(30) NOT NULL CONSTRAINT DF_ComplianceScore_SeverityCode DEFAULT (N'Minor');

    ALTER TABLE [ComplianceRisk].[ComplianceScore]
        ADD CONSTRAINT FK_ComplianceScore_SeverityLevel FOREIGN KEY ([SeverityCode])
            REFERENCES [Lookup].[SeverityLevel] ([Code]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('ComplianceRisk.RiskScore') AND name = 'SeverityCode')
BEGIN
    ALTER TABLE [ComplianceRisk].[RiskScore]
        ADD [SeverityCode] NVARCHAR(30) NOT NULL CONSTRAINT DF_RiskScore_SeverityCode DEFAULT (N'Minor');

    ALTER TABLE [ComplianceRisk].[RiskScore]
        ADD CONSTRAINT FK_RiskScore_SeverityLevel FOREIGN KEY ([SeverityCode])
            REFERENCES [Lookup].[SeverityLevel] ([Code]);
END
GO
