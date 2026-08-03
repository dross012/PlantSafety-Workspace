-- =============================================
-- Migration:     190-feature-flag-rollout-percentage.sql
-- Author:        PlantSafety Platform Team
-- Create date:   2026-08-03
-- Description:   Phase 6.6 (Enterprise Deployment & Release Architecture, §6.2 Blue/Green +
--                Canary + Tenant Rollout). Extends the real, already-built Phase 14
--                Governance [System].[FeatureFlag] tables (144-feature-flags.sql) in
--                PlantSafety.ArchiveDB and PlantSafety.IntegrationDB with an additive,
--                nullable RolloutPercentage column, so a flag can express a canary-rollout
--                percentage (0-100) alongside its existing binary IsEnabled switch.
--
--                RolloutPercentage semantics: NULL (default) means "no percentage rollout
--                configured -- IsEnabled alone governs, exactly as before this migration".
--                A non-NULL value (0-100) means "when IsEnabled = 1, only tenants whose
--                deterministic FeatureRolloutEvaluator hash falls under this percentage are
--                considered in the rollout wave for this flag" -- see
--                PlantSafety.Application's Common\Rollout\FeatureRolloutEvaluator.cs for the
--                evaluation logic (pure C#, no SQL-side computation).
--
--                Idempotency: column addition guarded by an existence check;
--                CHECK constraint guarded the same way.
-- =============================================
SET NOCOUNT ON;
GO

------------------------------------------------------------
-- 1. PlantSafety.ArchiveDB
------------------------------------------------------------
USE [PlantSafety.ArchiveDB];
GO
IF NOT EXISTS (
    SELECT 1 FROM sys.columns c
    JOIN sys.tables t ON c.object_id = t.object_id
    JOIN sys.schemas s ON t.schema_id = s.schema_id
    WHERE s.name = N'System' AND t.name = N'FeatureFlag' AND c.name = N'RolloutPercentage'
)
BEGIN
    ALTER TABLE [System].[FeatureFlag] ADD RolloutPercentage int NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_System_FeatureFlag_RolloutPercentage_ArchiveDB')
BEGIN
    ALTER TABLE [System].[FeatureFlag]
        ADD CONSTRAINT CK_System_FeatureFlag_RolloutPercentage_ArchiveDB
        CHECK (RolloutPercentage IS NULL OR RolloutPercentage BETWEEN 0 AND 100);
END
GO

------------------------------------------------------------
-- 2. PlantSafety.IntegrationDB
------------------------------------------------------------
USE [PlantSafety.IntegrationDB];
GO
IF NOT EXISTS (
    SELECT 1 FROM sys.columns c
    JOIN sys.tables t ON c.object_id = t.object_id
    JOIN sys.schemas s ON t.schema_id = s.schema_id
    WHERE s.name = N'System' AND t.name = N'FeatureFlag' AND c.name = N'RolloutPercentage'
)
BEGIN
    ALTER TABLE [System].[FeatureFlag] ADD RolloutPercentage int NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_System_FeatureFlag_RolloutPercentage_IntegrationDB')
BEGIN
    ALTER TABLE [System].[FeatureFlag]
        ADD CONSTRAINT CK_System_FeatureFlag_RolloutPercentage_IntegrationDB
        CHECK (RolloutPercentage IS NULL OR RolloutPercentage BETWEEN 0 AND 100);
END
GO

------------------------------------------------------------
-- 3. Verification
------------------------------------------------------------
SELECT DB_NAME() AS DbName, FlagName, IsEnabled, RolloutPercentage FROM [PlantSafety.IntegrationDB].System.FeatureFlag;
SELECT DB_NAME() AS DbName, FlagName, IsEnabled, RolloutPercentage FROM [PlantSafety.ArchiveDB].System.FeatureFlag;
GO
