-- ============================================================================
--  Project:        Shared.SafetyData -- Phase6-MultiFacilityTenant
--  File:           10-indexes.sql
--  Created:        2026-08-09
--  Idempotent: guarded by sys.indexes existence checks.
-- ============================================================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_TenantFacilityMap_FacilityId' AND object_id = OBJECT_ID('TenantMap.TenantFacilityMap'))
    CREATE NONCLUSTERED INDEX IX_TenantFacilityMap_FacilityId ON [TenantMap].[TenantFacilityMap] ([FacilityId]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ComplianceScore_SeverityCode' AND object_id = OBJECT_ID('ComplianceRisk.ComplianceScore'))
    CREATE NONCLUSTERED INDEX IX_ComplianceScore_SeverityCode ON [ComplianceRisk].[ComplianceScore] ([SeverityCode]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_RiskScore_SeverityCode' AND object_id = OBJECT_ID('ComplianceRisk.RiskScore'))
    CREATE NONCLUSTERED INDEX IX_RiskScore_SeverityCode ON [ComplianceRisk].[RiskScore] ([SeverityCode]);
GO
