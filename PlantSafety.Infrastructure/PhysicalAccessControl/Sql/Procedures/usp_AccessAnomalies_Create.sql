-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         PhysicalAccessControl.AccessAnomalies
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_AccessAnomalies_Create.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Inserts a newly detected AccessAnomaly row. Best-effort audit-log side
--                  effect.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [PhysicalAccessControl].[usp_AccessAnomalies_Create]
    @AccessAnomalyId  UNIQUEIDENTIFIER,
    @TenantId         UNIQUEIDENTIFIER,
    @SiteId           UNIQUEIDENTIFIER = NULL,
    @Status           NVARCHAR(100),
    @AccessEventId    UNIQUEIDENTIFIER = NULL,
    @AccessPointId    UNIQUEIDENTIFIER,
    @CredentialId     UNIQUEIDENTIFIER = NULL,
    @SafetyZoneId     UNIQUEIDENTIFIER = NULL,
    @AnomalyType      NVARCHAR(100),
    @SeverityScore    DECIMAL(9,2),
    @CreatedAtUtc     DATETIME2(3),
    @CreatedBy        UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        INSERT INTO [PhysicalAccessControl].[AccessAnomalies]
            ([AccessAnomalyId], [TenantId], [SiteId], [Status], [AccessEventId], [AccessPointId],
             [CredentialId], [SafetyZoneId], [AnomalyType], [SeverityScore], [CreatedAtUtc], [CreatedBy])
        VALUES
            (@AccessAnomalyId, @TenantId, @SiteId, @Status, @AccessEventId, @AccessPointId,
             @CredentialId, @SafetyZoneId, @AnomalyType, @SeverityScore, @CreatedAtUtc, @CreatedBy);

        INSERT INTO dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime)
        VALUES ('PhysicalAccessControl.AccessAnomalies', DB_NAME(), 'INSERT', @CreatedBy, SYSUTCDATETIME());
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO
