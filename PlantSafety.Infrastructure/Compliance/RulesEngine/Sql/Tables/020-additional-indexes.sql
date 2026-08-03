-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.RulesEngine
--  Layer:          Infrastructure -- SQL Scripts
--  File:           020-additional-indexes.sql
--  Created:        2026-08-02
--  Author:         PlantSafety Platform Team (SQL & Database Agent, Phase 5 hardening pass)
--  SchemaVersion:  2026.1
--  Description:    Adds the covering index needed by usp_ComplianceEvaluation_GetAllForTenant,
--                  which filters on (TenantId, IsDeleted) and orders by StartedAtUtc DESC. The
--                  existing IX_ComplianceEvaluation_Tenant_Target index is
--                  (TenantId, TargetEntityType, TargetEntityId, StartedAtUtc DESC) -- perfectly
--                  covers usp_ComplianceEvaluation_GetHistoryForTarget, but
--                  TargetEntityType/TargetEntityId sit between TenantId and StartedAtUtc in
--                  that index's key order, so it cannot be used to satisfy the tenant-wide
--                  "ORDER BY StartedAtUtc DESC" in GetAllForTenant without an extra sort. This
--                  adds a second, purpose-built filtered+covering index for that access pattern.
--                  Idempotent (guarded by sys.indexes existence check); additive-only.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ComplianceEvaluation_Tenant_Started_ActiveOnly' AND object_id = OBJECT_ID('[ComplianceRulesEngine].[ComplianceEvaluation]'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_ComplianceEvaluation_Tenant_Started_ActiveOnly
        ON [ComplianceRulesEngine].[ComplianceEvaluation] (TenantId, StartedAtUtc DESC)
        INCLUDE (SiteId, TargetEntityType, TargetEntityId, Status, CompletedAtUtc, TriggeredBy)
        WHERE IsDeleted = 0;
END
GO
