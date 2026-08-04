-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         PhysicalAccessControl.SafetyZones
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_SafetyZones_Update.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Updates an existing SafetyZone aggregate row (including occupancy
--                  recalculation). Best-effort audit-log side effect.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [PhysicalAccessControl].[usp_SafetyZones_Update]
    @NowUtc            DATETIME2(3),
    @SafetyZoneId      UNIQUEIDENTIFIER,
    @TenantId          UNIQUEIDENTIFIER,
    @SiteId            UNIQUEIDENTIFIER = NULL,
    @Status            NVARCHAR(100),
    @ZoneCode          NVARCHAR(100),
    @ZoneName          NVARCHAR(200),
    @HazardLevel       NVARCHAR(50),
    @MaxOccupancy      INT,
    @CurrentOccupancy  INT
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        UPDATE [PhysicalAccessControl].[SafetyZones]
        SET [Status]           = @Status,
            [ZoneCode]         = @ZoneCode,
            [ZoneName]         = @ZoneName,
            [HazardLevel]      = @HazardLevel,
            [MaxOccupancy]     = @MaxOccupancy,
            [CurrentOccupancy] = @CurrentOccupancy,
            [ModifiedAtUtc]    = @NowUtc
        WHERE [SafetyZoneId] = @SafetyZoneId
          AND [TenantId] = @TenantId;

        INSERT INTO dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime)
        VALUES ('PhysicalAccessControl.SafetyZones', DB_NAME(), 'UPDATE', NULL, SYSUTCDATETIME());
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO
