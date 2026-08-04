-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         PhysicalAccessControl.AccessAnomalies
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_AccessAnomalies_Update.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Updates an existing AccessAnomaly aggregate row (triage transitions).
--                  Best-effort audit-log side effect.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [PhysicalAccessControl].[usp_AccessAnomalies_Update]
    @NowUtc                DATETIME2(3),
    @AccessAnomalyId       UNIQUEIDENTIFIER,
    @TenantId              UNIQUEIDENTIFIER,
    @SiteId                UNIQUEIDENTIFIER = NULL,
    @Status                NVARCHAR(100),
    @AnomalyType           NVARCHAR(100),
    @SeverityScore         DECIMAL(9,2),
    @AcknowledgedByUserId  UNIQUEIDENTIFIER = NULL,
    @Resolution            NVARCHAR(2000) = NULL
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        UPDATE [PhysicalAccessControl].[AccessAnomalies]
        SET [Status]               = @Status,
            [AnomalyType]          = @AnomalyType,
            [SeverityScore]        = @SeverityScore,
            [AcknowledgedByUserId] = @AcknowledgedByUserId,
            [Resolution]           = @Resolution,
            [ModifiedAtUtc]        = @NowUtc
        WHERE [AccessAnomalyId] = @AccessAnomalyId
          AND [TenantId] = @TenantId;

        INSERT INTO dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime)
        VALUES ('PhysicalAccessControl.AccessAnomalies', DB_NAME(), 'UPDATE', NULL, SYSUTCDATETIME());
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO
