-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         PhysicalAccessControl.AccessEvents
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_AccessEvents_Update.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Updates an existing AccessEvent aggregate row (processing status
--                  transitions). Best-effort audit-log side effect.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [PhysicalAccessControl].[usp_AccessEvents_Update]
    @NowUtc              DATETIME2(3),
    @AccessEventId       UNIQUEIDENTIFIER,
    @TenantId            UNIQUEIDENTIFIER,
    @SiteId              UNIQUEIDENTIFIER = NULL,
    @Status              NVARCHAR(100),
    @AccessPointId       UNIQUEIDENTIFIER,
    @CredentialId        UNIQUEIDENTIFIER = NULL,
    @SafetyZoneId        UNIQUEIDENTIFIER = NULL,
    @EventType           NVARCHAR(100),
    @Direction           NVARCHAR(20),
    @FlaggedAnomalyType  NVARCHAR(100) = NULL
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        UPDATE [PhysicalAccessControl].[AccessEvents]
        SET [Status]             = @Status,
            [AccessPointId]      = @AccessPointId,
            [CredentialId]       = @CredentialId,
            [SafetyZoneId]       = @SafetyZoneId,
            [EventType]          = @EventType,
            [Direction]          = @Direction,
            [FlaggedAnomalyType] = @FlaggedAnomalyType,
            [ModifiedAtUtc]      = @NowUtc
        WHERE [AccessEventId] = @AccessEventId
          AND [TenantId] = @TenantId;

        INSERT INTO dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime)
        VALUES ('PhysicalAccessControl.AccessEvents', DB_NAME(), 'UPDATE', NULL, SYSUTCDATETIME());
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO
