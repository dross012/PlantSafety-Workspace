-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         PhysicalAccessControl.RestrictedZonePolicies
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_RestrictedZonePolicies_Create.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Inserts a new RestrictedZonePolicy aggregate row. Best-effort audit-log
--                  side effect.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [PhysicalAccessControl].[usp_RestrictedZonePolicies_Create]
    @RestrictedZonePolicyId    UNIQUEIDENTIFIER,
    @TenantId                  UNIQUEIDENTIFIER,
    @SiteId                    UNIQUEIDENTIFIER = NULL,
    @Status                    NVARCHAR(100),
    @SafetyZoneId              UNIQUEIDENTIFIER,
    @PolicyName                NVARCHAR(200),
    @AllowedAccessLevel        NVARCHAR(100),
    @EnforceAfterHoursLockout  BIT,
    @CreatedAtUtc              DATETIME2(3),
    @CreatedBy                 UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        INSERT INTO [PhysicalAccessControl].[RestrictedZonePolicies]
            ([RestrictedZonePolicyId], [TenantId], [SiteId], [Status], [SafetyZoneId],
             [PolicyName], [AllowedAccessLevel], [EnforceAfterHoursLockout], [CreatedAtUtc], [CreatedBy])
        VALUES
            (@RestrictedZonePolicyId, @TenantId, @SiteId, @Status, @SafetyZoneId,
             @PolicyName, @AllowedAccessLevel, @EnforceAfterHoursLockout, @CreatedAtUtc, @CreatedBy);

        INSERT INTO dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime)
        VALUES ('PhysicalAccessControl.RestrictedZonePolicies', DB_NAME(), 'INSERT', @CreatedBy, SYSUTCDATETIME());
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO
