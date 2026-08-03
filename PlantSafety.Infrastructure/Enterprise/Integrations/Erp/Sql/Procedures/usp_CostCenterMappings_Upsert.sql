-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Enterprise.Integrations.Erp
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_CostCenterMappings_Upsert.sql
--  Created:        2026-08-03
--  SchemaVersion:  2026.1
--  Description:    Creates or replaces the cost center mapping for a Tenant/Site pair. Additive-only
--                  evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Enterprise].[usp_CostCenterMappings_Upsert]
    @TenantId               UNIQUEIDENTIFIER,
    @SiteId                 UNIQUEIDENTIFIER = NULL,
    @ExternalCostCenterCode NVARCHAR(50),
    @Description            NVARCHAR(200) = NULL,
    @UpdatedAtUtc           DATETIME2(3),
    @UpdatedBy              UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;

DECLARE @SiteIdKey UNIQUEIDENTIFIER = ISNULL(@SiteId, '00000000-0000-0000-0000-000000000000');

MERGE [Enterprise].[CostCenterMappings] AS target
USING (SELECT @TenantId AS TenantId, @SiteIdKey AS SiteIdKey) AS source
    ON target.TenantId = source.TenantId AND target.SiteIdKey = source.SiteIdKey
WHEN MATCHED THEN
    UPDATE SET
        ExternalCostCenterCode = @ExternalCostCenterCode,
        Description            = @Description,
        UpdatedAtUtc            = @UpdatedAtUtc,
        UpdatedBy               = @UpdatedBy
WHEN NOT MATCHED THEN
    INSERT (Id, TenantId, SiteId, ExternalCostCenterCode, Description, UpdatedAtUtc, UpdatedBy)
    VALUES (NEWID(), @TenantId, @SiteId, @ExternalCostCenterCode, @Description, @UpdatedAtUtc, @UpdatedBy);
GO
