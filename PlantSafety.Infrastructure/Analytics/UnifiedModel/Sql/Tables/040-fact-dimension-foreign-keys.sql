-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Analytics.UnifiedModel
--  Layer:          Infrastructure -- SQL Scripts
--  File:           040-fact-dimension-foreign-keys.sql
--  Created:        2026-08-02
--  Author:         PlantSafety Platform Team (SQL & Database Agent, Phase 5 hardening pass)
--  SchemaVersion:  2026.1
--  Description:    Adds the missing referential-integrity FOREIGN KEY constraints from the six
--                  AnalyticsUnifiedModel fact tables to their dimension tables. None of the
--                  original CREATE TABLE scripts declared these -- every *Key column
--                  (DateKey/SiteKey/EmployeeKey/AssetKey/ShiftKey) on every fact table was a
--                  bare UNIQUEIDENTIFIER with no FK, so orphaned/mistyped surrogate keys in the
--                  ETL load would go completely undetected. DateKey is NOT NULL on every fact
--                  (every fact event has a date) so those FKs are non-nullable; the
--                  Site/Employee/Asset/Shift key columns are NULL-able on the fact tables (not
--                  every event has a known site/employee/asset/shift), so SQL Server permits
--                  NULL to satisfy the FK without requiring a matching dimension row -- this
--                  matches the columns' existing NULL-able declaration and requires no table
--                  rewrite.
--
--                  DimAsset/DimEmployee/DimSite are SCD Type 2 (see their filtered
--                  UX_Dim*_BusinessKey_CurrentOnly indexes): the fact tables' *Key columns
--                  reference the DIMENSION SURROGATE KEY (SurrogateKey), i.e. the specific
--                  dimension row version effective when the fact was captured -- not the
--                  BusinessKey -- per standard Kimball SCD2 fact-to-dimension fanout practice,
--                  and matching how DimAsset_Tenant_Site/DimEmployee_Tenant/etc. are already
--                  indexed for point-in-time lookups. DimShift and DimDate are simple
--                  (non-SCD2) lookup dimensions, so their FK targets are just their own PK.
--
--                  Idempotent (guarded by sys.foreign_keys existence checks); additive-only,
--                  matches the "Additive-only evolution" convention used throughout this
--                  schema. Run after all Tables/*.sql scripts (fact and dimension tables) have
--                  already created their target tables.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

-- FactRiskAssessment: DateKey (NOT NULL) -> DimDate; SiteKey (NULL-able) -> DimSite.SurrogateKey;
-- EmployeeKey (NULL-able) -> DimEmployee.SurrogateKey.
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_FactRiskAssessment_DimDate')
    ALTER TABLE [AnalyticsUnifiedModel].[FactRiskAssessment] WITH CHECK ADD CONSTRAINT FK_FactRiskAssessment_DimDate FOREIGN KEY (DateKey) REFERENCES [AnalyticsUnifiedModel].[DimDate] (DateKey);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_FactRiskAssessment_DimSite')
    ALTER TABLE [AnalyticsUnifiedModel].[FactRiskAssessment] WITH CHECK ADD CONSTRAINT FK_FactRiskAssessment_DimSite FOREIGN KEY (SiteKey) REFERENCES [AnalyticsUnifiedModel].[DimSite] (SurrogateKey);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_FactRiskAssessment_DimEmployee')
    ALTER TABLE [AnalyticsUnifiedModel].[FactRiskAssessment] WITH CHECK ADD CONSTRAINT FK_FactRiskAssessment_DimEmployee FOREIGN KEY (EmployeeKey) REFERENCES [AnalyticsUnifiedModel].[DimEmployee] (SurrogateKey);
GO

-- FactAuditFinding: DateKey -> DimDate; SiteKey -> DimSite.SurrogateKey; EmployeeKey ->
-- DimEmployee.SurrogateKey. AuditEngagementId intentionally NOT FK'd here -- it references the
-- pre-existing [Auditing].[AuditEngagement] aggregate root, a cross-module reference this OLAP
-- schema deliberately keeps loosely coupled (matches SourceAggregateId's existing pattern).
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_FactAuditFinding_DimDate')
    ALTER TABLE [AnalyticsUnifiedModel].[FactAuditFinding] WITH CHECK ADD CONSTRAINT FK_FactAuditFinding_DimDate FOREIGN KEY (DateKey) REFERENCES [AnalyticsUnifiedModel].[DimDate] (DateKey);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_FactAuditFinding_DimSite')
    ALTER TABLE [AnalyticsUnifiedModel].[FactAuditFinding] WITH CHECK ADD CONSTRAINT FK_FactAuditFinding_DimSite FOREIGN KEY (SiteKey) REFERENCES [AnalyticsUnifiedModel].[DimSite] (SurrogateKey);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_FactAuditFinding_DimEmployee')
    ALTER TABLE [AnalyticsUnifiedModel].[FactAuditFinding] WITH CHECK ADD CONSTRAINT FK_FactAuditFinding_DimEmployee FOREIGN KEY (EmployeeKey) REFERENCES [AnalyticsUnifiedModel].[DimEmployee] (SurrogateKey);
GO

-- FactMaintenanceWorkOrder: DateKey -> DimDate; SiteKey -> DimSite.SurrogateKey; AssetKey
-- (NOT NULL on this fact) -> DimAsset.SurrogateKey; EmployeeKey -> DimEmployee.SurrogateKey.
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_FactMaintenanceWorkOrder_DimDate')
    ALTER TABLE [AnalyticsUnifiedModel].[FactMaintenanceWorkOrder] WITH CHECK ADD CONSTRAINT FK_FactMaintenanceWorkOrder_DimDate FOREIGN KEY (DateKey) REFERENCES [AnalyticsUnifiedModel].[DimDate] (DateKey);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_FactMaintenanceWorkOrder_DimSite')
    ALTER TABLE [AnalyticsUnifiedModel].[FactMaintenanceWorkOrder] WITH CHECK ADD CONSTRAINT FK_FactMaintenanceWorkOrder_DimSite FOREIGN KEY (SiteKey) REFERENCES [AnalyticsUnifiedModel].[DimSite] (SurrogateKey);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_FactMaintenanceWorkOrder_DimAsset')
    ALTER TABLE [AnalyticsUnifiedModel].[FactMaintenanceWorkOrder] WITH CHECK ADD CONSTRAINT FK_FactMaintenanceWorkOrder_DimAsset FOREIGN KEY (AssetKey) REFERENCES [AnalyticsUnifiedModel].[DimAsset] (SurrogateKey);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_FactMaintenanceWorkOrder_DimEmployee')
    ALTER TABLE [AnalyticsUnifiedModel].[FactMaintenanceWorkOrder] WITH CHECK ADD CONSTRAINT FK_FactMaintenanceWorkOrder_DimEmployee FOREIGN KEY (EmployeeKey) REFERENCES [AnalyticsUnifiedModel].[DimEmployee] (SurrogateKey);
GO

-- FactOperationalEvent: DateKey -> DimDate; SiteKey -> DimSite.SurrogateKey; AssetKey ->
-- DimAsset.SurrogateKey; EmployeeKey -> DimEmployee.SurrogateKey.
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_FactOperationalEvent_DimDate')
    ALTER TABLE [AnalyticsUnifiedModel].[FactOperationalEvent] WITH CHECK ADD CONSTRAINT FK_FactOperationalEvent_DimDate FOREIGN KEY (DateKey) REFERENCES [AnalyticsUnifiedModel].[DimDate] (DateKey);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_FactOperationalEvent_DimSite')
    ALTER TABLE [AnalyticsUnifiedModel].[FactOperationalEvent] WITH CHECK ADD CONSTRAINT FK_FactOperationalEvent_DimSite FOREIGN KEY (SiteKey) REFERENCES [AnalyticsUnifiedModel].[DimSite] (SurrogateKey);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_FactOperationalEvent_DimAsset')
    ALTER TABLE [AnalyticsUnifiedModel].[FactOperationalEvent] WITH CHECK ADD CONSTRAINT FK_FactOperationalEvent_DimAsset FOREIGN KEY (AssetKey) REFERENCES [AnalyticsUnifiedModel].[DimAsset] (SurrogateKey);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_FactOperationalEvent_DimEmployee')
    ALTER TABLE [AnalyticsUnifiedModel].[FactOperationalEvent] WITH CHECK ADD CONSTRAINT FK_FactOperationalEvent_DimEmployee FOREIGN KEY (EmployeeKey) REFERENCES [AnalyticsUnifiedModel].[DimEmployee] (SurrogateKey);
GO

-- FactSchedulingEvent: DateKey -> DimDate; SiteKey -> DimSite.SurrogateKey; AssetKey ->
-- DimAsset.SurrogateKey; EmployeeKey -> DimEmployee.SurrogateKey; ShiftKey -> DimShift (simple
-- lookup dimension, FK targets its own PK directly).
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_FactSchedulingEvent_DimDate')
    ALTER TABLE [AnalyticsUnifiedModel].[FactSchedulingEvent] WITH CHECK ADD CONSTRAINT FK_FactSchedulingEvent_DimDate FOREIGN KEY (DateKey) REFERENCES [AnalyticsUnifiedModel].[DimDate] (DateKey);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_FactSchedulingEvent_DimSite')
    ALTER TABLE [AnalyticsUnifiedModel].[FactSchedulingEvent] WITH CHECK ADD CONSTRAINT FK_FactSchedulingEvent_DimSite FOREIGN KEY (SiteKey) REFERENCES [AnalyticsUnifiedModel].[DimSite] (SurrogateKey);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_FactSchedulingEvent_DimAsset')
    ALTER TABLE [AnalyticsUnifiedModel].[FactSchedulingEvent] WITH CHECK ADD CONSTRAINT FK_FactSchedulingEvent_DimAsset FOREIGN KEY (AssetKey) REFERENCES [AnalyticsUnifiedModel].[DimAsset] (SurrogateKey);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_FactSchedulingEvent_DimEmployee')
    ALTER TABLE [AnalyticsUnifiedModel].[FactSchedulingEvent] WITH CHECK ADD CONSTRAINT FK_FactSchedulingEvent_DimEmployee FOREIGN KEY (EmployeeKey) REFERENCES [AnalyticsUnifiedModel].[DimEmployee] (SurrogateKey);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_FactSchedulingEvent_DimShift')
    ALTER TABLE [AnalyticsUnifiedModel].[FactSchedulingEvent] WITH CHECK ADD CONSTRAINT FK_FactSchedulingEvent_DimShift FOREIGN KEY (ShiftKey) REFERENCES [AnalyticsUnifiedModel].[DimShift] (ShiftKey);
GO

-- FactContinuousImprovementAction: DateKey -> DimDate; SiteKey -> DimSite.SurrogateKey;
-- EmployeeKey -> DimEmployee.SurrogateKey.
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_FactCIAction_DimDate')
    ALTER TABLE [AnalyticsUnifiedModel].[FactContinuousImprovementAction] WITH CHECK ADD CONSTRAINT FK_FactCIAction_DimDate FOREIGN KEY (DateKey) REFERENCES [AnalyticsUnifiedModel].[DimDate] (DateKey);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_FactCIAction_DimSite')
    ALTER TABLE [AnalyticsUnifiedModel].[FactContinuousImprovementAction] WITH CHECK ADD CONSTRAINT FK_FactCIAction_DimSite FOREIGN KEY (SiteKey) REFERENCES [AnalyticsUnifiedModel].[DimSite] (SurrogateKey);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_FactCIAction_DimEmployee')
    ALTER TABLE [AnalyticsUnifiedModel].[FactContinuousImprovementAction] WITH CHECK ADD CONSTRAINT FK_FactCIAction_DimEmployee FOREIGN KEY (EmployeeKey) REFERENCES [AnalyticsUnifiedModel].[DimEmployee] (SurrogateKey);
GO
