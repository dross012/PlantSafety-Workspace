-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         PhysicalAccessControl.AccessPoints
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_AccessPoints_Create.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Inserts a new AccessPoint aggregate row. Best-effort audit-log side effect.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [PhysicalAccessControl].[usp_AccessPoints_Create]
    @AccessPointId    UNIQUEIDENTIFIER,
    @TenantId         UNIQUEIDENTIFIER,
    @SiteId           UNIQUEIDENTIFIER = NULL,
    @Status           NVARCHAR(100),
    @AccessPointCode  NVARCHAR(100),
    @Description      NVARCHAR(2000),
    @DeviceType       NVARCHAR(100),
    @SafetyZoneId     UNIQUEIDENTIFIER = NULL,
    @IsPerimeter      BIT,
    @CreatedAtUtc     DATETIME2(3),
    @CreatedBy        UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        INSERT INTO [PhysicalAccessControl].[AccessPoints]
            ([AccessPointId], [TenantId], [SiteId], [Status], [AccessPointCode], [Description],
             [DeviceType], [SafetyZoneId], [IsPerimeter], [CreatedAtUtc], [CreatedBy])
        VALUES
            (@AccessPointId, @TenantId, @SiteId, @Status, @AccessPointCode, @Description,
             @DeviceType, @SafetyZoneId, @IsPerimeter, @CreatedAtUtc, @CreatedBy);

        INSERT INTO dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime)
        VALUES ('PhysicalAccessControl.AccessPoints', DB_NAME(), 'INSERT', @CreatedBy, SYSUTCDATETIME());
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO
