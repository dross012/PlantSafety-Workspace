-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         PhysicalAccessControl.AccessPoints
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_AccessPoints_Update.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Updates an existing AccessPoint aggregate row. Best-effort audit-log side
--                  effect.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [PhysicalAccessControl].[usp_AccessPoints_Update]
    @NowUtc           DATETIME2(3),
    @AccessPointId    UNIQUEIDENTIFIER,
    @TenantId         UNIQUEIDENTIFIER,
    @SiteId           UNIQUEIDENTIFIER = NULL,
    @Status           NVARCHAR(100),
    @AccessPointCode  NVARCHAR(100),
    @Description      NVARCHAR(2000),
    @DeviceType       NVARCHAR(100),
    @SafetyZoneId     UNIQUEIDENTIFIER = NULL,
    @IsPerimeter      BIT
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        UPDATE [PhysicalAccessControl].[AccessPoints]
        SET [Status]          = @Status,
            [AccessPointCode] = @AccessPointCode,
            [Description]     = @Description,
            [DeviceType]      = @DeviceType,
            [SafetyZoneId]    = @SafetyZoneId,
            [IsPerimeter]     = @IsPerimeter,
            [ModifiedAtUtc]   = @NowUtc
        WHERE [AccessPointId] = @AccessPointId
          AND [TenantId] = @TenantId;

        INSERT INTO dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime)
        VALUES ('PhysicalAccessControl.AccessPoints', DB_NAME(), 'UPDATE', NULL, SYSUTCDATETIME());
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO
