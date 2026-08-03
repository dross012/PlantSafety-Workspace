-- =============================================
-- Migration:     200-tenant-user-region-feature-flag-overrides.sql
-- Author:        PlantSafety Platform Team
-- Create date:   2026-08-03
-- Description:   Phase 7 (SaaS Productization Blueprint §7.1 Feature Flag System, §7.2
--                Release Pipeline). Adds the real backing tables for the tenant/user/
--                region-level feature flag override precedence chain that sits above the
--                existing tenant-agnostic [System].[FeatureFlag] (144-feature-flags.sql,
--                190-feature-flag-rollout-percentage.sql), plus the per-tenant release
--                channel used for canary auto-inclusion.
--
--                Genuine-gap check performed first: a full search of
--                PlantSafety.Infrastructure\Persistence\Migrations\**\*.sql for tables named
--                TenantOverride/TenantConfig/UserFeatureFlag/RegionFeatureFlag found NONE
--                prior to this migration (documented explicitly in 144-feature-flags.sql's
--                SCOPE NOTE as an out-of-scope area at that time, since no backing tables
--                existed). This migration fills that genuine gap.
--
--                Placement: co-located in [PlantSafety.ArchiveDB] alongside the existing
--                [System].[FeatureFlag] governance table, under a new [Release] schema.
--                Unlike [System].[FeatureFlag] (which is deliberately duplicated
--                independently per-database as a SQL-Agent/Service-Broker-reachable ops
--                kill-switch), these override tables are consulted exclusively by the .NET
--                Application-tier IFeatureFlagResolver -- a single, centrally-queried
--                definition is correct here; no cross-database duplication is needed.
--
--                No PlantSafety.Domain Tenant aggregate/table exists yet as of this
--                migration (verified) -- TenantId/UserId are therefore plain
--                uniqueidentifier columns with no FK, exactly as every other
--                tenant/user-scoped table in this codebase already does pending that
--                aggregate's arrival.
--
--                Idempotency: table creation guarded by existence checks; CHECK
--                constraints guarded the same way.
-- =============================================
SET NOCOUNT ON;
GO

USE [PlantSafety.ArchiveDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'Release')
BEGIN
    EXEC('CREATE SCHEMA [Release] AUTHORIZATION [dbo]');
END
GO

------------------------------------------------------------
-- 1. Release.TenantFeatureFlagOverride
------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = N'Release' AND t.name = N'TenantFeatureFlagOverride')
BEGIN
    CREATE TABLE [Release].[TenantFeatureFlagOverride] (
        TenantFeatureFlagOverrideId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Release_TenantFeatureFlagOverride PRIMARY KEY CLUSTERED,
        TenantId           uniqueidentifier NOT NULL,
        FeatureName        nvarchar(200)    NOT NULL,
        Enabled            bit              NOT NULL CONSTRAINT DF_Release_TenantFFO_Enabled DEFAULT (1),
        RolloutPercentage  int              NULL,
        UpdatedUtc         datetime2(3)     NOT NULL CONSTRAINT DF_Release_TenantFFO_UpdatedUtc DEFAULT (SYSUTCDATETIME()),
        UpdatedBy          nvarchar(128)    NOT NULL CONSTRAINT DF_Release_TenantFFO_UpdatedBy DEFAULT (SYSTEM_USER),
        CONSTRAINT UQ_Release_TenantFFO_Tenant_Feature UNIQUE (TenantId, FeatureName)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_Release_TenantFFO_RolloutPercentage')
BEGIN
    ALTER TABLE [Release].[TenantFeatureFlagOverride]
        ADD CONSTRAINT CK_Release_TenantFFO_RolloutPercentage
        CHECK (RolloutPercentage IS NULL OR RolloutPercentage BETWEEN 0 AND 100);
END
GO

------------------------------------------------------------
-- 2. Release.UserFeatureFlagOverride
------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = N'Release' AND t.name = N'UserFeatureFlagOverride')
BEGIN
    CREATE TABLE [Release].[UserFeatureFlagOverride] (
        UserFeatureFlagOverrideId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Release_UserFeatureFlagOverride PRIMARY KEY CLUSTERED,
        UserId       uniqueidentifier NOT NULL,
        FeatureName  nvarchar(200)    NOT NULL,
        Enabled      bit              NOT NULL CONSTRAINT DF_Release_UserFFO_Enabled DEFAULT (1),
        UpdatedUtc   datetime2(3)     NOT NULL CONSTRAINT DF_Release_UserFFO_UpdatedUtc DEFAULT (SYSUTCDATETIME()),
        UpdatedBy    nvarchar(128)    NOT NULL CONSTRAINT DF_Release_UserFFO_UpdatedBy DEFAULT (SYSTEM_USER),
        CONSTRAINT UQ_Release_UserFFO_User_Feature UNIQUE (UserId, FeatureName)
    );
END
GO

------------------------------------------------------------
-- 3. Release.RegionFeatureFlagOverride
------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = N'Release' AND t.name = N'RegionFeatureFlagOverride')
BEGIN
    CREATE TABLE [Release].[RegionFeatureFlagOverride] (
        RegionFeatureFlagOverrideId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Release_RegionFeatureFlagOverride PRIMARY KEY CLUSTERED,
        Region             nvarchar(50)     NOT NULL,
        FeatureName        nvarchar(200)    NOT NULL,
        Enabled            bit              NOT NULL CONSTRAINT DF_Release_RegionFFO_Enabled DEFAULT (1),
        RolloutPercentage  int              NULL,
        UpdatedUtc         datetime2(3)     NOT NULL CONSTRAINT DF_Release_RegionFFO_UpdatedUtc DEFAULT (SYSUTCDATETIME()),
        UpdatedBy          nvarchar(128)    NOT NULL CONSTRAINT DF_Release_RegionFFO_UpdatedBy DEFAULT (SYSTEM_USER),
        CONSTRAINT UQ_Release_RegionFFO_Region_Feature UNIQUE (Region, FeatureName)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_Release_RegionFFO_RolloutPercentage')
BEGIN
    ALTER TABLE [Release].[RegionFeatureFlagOverride]
        ADD CONSTRAINT CK_Release_RegionFFO_RolloutPercentage
        CHECK (RolloutPercentage IS NULL OR RolloutPercentage BETWEEN 0 AND 100);
END
GO

------------------------------------------------------------
-- 4. Release.TenantReleaseChannel
------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = N'Release' AND t.name = N'TenantReleaseChannel')
BEGIN
    CREATE TABLE [Release].[TenantReleaseChannel] (
        TenantId    uniqueidentifier NOT NULL CONSTRAINT PK_Release_TenantReleaseChannel PRIMARY KEY CLUSTERED,
        Channel     nvarchar(20)     NOT NULL CONSTRAINT DF_Release_TenantReleaseChannel_Channel DEFAULT (N'Stable'),
        UpdatedUtc  datetime2(3)     NOT NULL CONSTRAINT DF_Release_TenantReleaseChannel_UpdatedUtc DEFAULT (SYSUTCDATETIME()),
        UpdatedBy   nvarchar(128)    NOT NULL CONSTRAINT DF_Release_TenantReleaseChannel_UpdatedBy DEFAULT (SYSTEM_USER)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_Release_TenantReleaseChannel_Channel')
BEGIN
    ALTER TABLE [Release].[TenantReleaseChannel]
        ADD CONSTRAINT CK_Release_TenantReleaseChannel_Channel
        CHECK (Channel IN (N'Stable', N'Beta', N'Canary'));
END
GO

------------------------------------------------------------
-- 5. Document the new schema/tables.
------------------------------------------------------------
IF EXISTS (SELECT 1 FROM sys.extended_properties ep WHERE ep.major_id = OBJECT_ID(N'Release.TenantFeatureFlagOverride') AND ep.minor_id = 0 AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Phase 7 tenant-level feature flag override -- precedes the global [System].[FeatureFlag] in IFeatureFlagResolver''s precedence chain (user > tenant > region > global).', @level0type=N'SCHEMA', @level0name=N'Release', @level1type=N'TABLE', @level1name=N'TenantFeatureFlagOverride';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Phase 7 tenant-level feature flag override -- precedes the global [System].[FeatureFlag] in IFeatureFlagResolver''s precedence chain (user > tenant > region > global).', @level0type=N'SCHEMA', @level0name=N'Release', @level1type=N'TABLE', @level1name=N'TenantFeatureFlagOverride';
GO

------------------------------------------------------------
-- 6. Verification
------------------------------------------------------------
SELECT * FROM [Release].[TenantFeatureFlagOverride];
SELECT * FROM [Release].[UserFeatureFlagOverride];
SELECT * FROM [Release].[RegionFeatureFlagOverride];
SELECT * FROM [Release].[TenantReleaseChannel];
GO
