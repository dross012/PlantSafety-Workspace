-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         PhysicalAccessControl.SafetyZones
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_SafetyZones_Create.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Inserts a new SafetyZone aggregate row. Best-effort audit-log side effect.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [PhysicalAccessControl].[usp_SafetyZones_Create]
    @SafetyZoneId      UNIQUEIDENTIFIER,
    @TenantId          UNIQUEIDENTIFIER,
    @SiteId            UNIQUEIDENTIFIER = NULL,
    @Status            NVARCHAR(100),
    @ZoneCode          NVARCHAR(100),
    @ZoneName          NVARCHAR(200),
    @HazardLevel       NVARCHAR(50),
    @MaxOccupancy      INT,
    @CurrentOccupancy  INT,
    @CreatedAtUtc      DATETIME2(3),
    @CreatedBy         UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        INSERT INTO [PhysicalAccessControl].[SafetyZones]
            ([SafetyZoneId], [TenantId], [SiteId], [Status], [ZoneCode], [ZoneName],
             [HazardLevel], [MaxOccupancy], [CurrentOccupancy], [CreatedAtUtc], [CreatedBy])
        VALUES
            (@SafetyZoneId, @TenantId, @SiteId, @Status, @ZoneCode, @ZoneName,
             @HazardLevel, @MaxOccupancy, @CurrentOccupancy, @CreatedAtUtc, @CreatedBy);

        INSERT INTO dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime)
        VALUES ('PhysicalAccessControl.SafetyZones', DB_NAME(), 'INSERT', @CreatedBy, SYSUTCDATETIME());
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO
