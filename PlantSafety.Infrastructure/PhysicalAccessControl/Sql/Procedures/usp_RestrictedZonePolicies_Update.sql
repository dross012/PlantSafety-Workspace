-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         PhysicalAccessControl.RestrictedZonePolicies
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_RestrictedZonePolicies_Update.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Updates an existing RestrictedZonePolicy aggregate row. Best-effort
--                  audit-log side effect.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [PhysicalAccessControl].[usp_RestrictedZonePolicies_Update]
    @NowUtc                    DATETIME2(3),
    @RestrictedZonePolicyId    UNIQUEIDENTIFIER,
    @TenantId                  UNIQUEIDENTIFIER,
    @SiteId                    UNIQUEIDENTIFIER = NULL,
    @Status                    NVARCHAR(100),
    @SafetyZoneId              UNIQUEIDENTIFIER,
    @PolicyName                NVARCHAR(200),
    @AllowedAccessLevel        NVARCHAR(100),
    @EnforceAfterHoursLockout  BIT
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        UPDATE [PhysicalAccessControl].[RestrictedZonePolicies]
        SET [Status]                    = @Status,
            [SafetyZoneId]              = @SafetyZoneId,
            [PolicyName]                = @PolicyName,
            [AllowedAccessLevel]        = @AllowedAccessLevel,
            [EnforceAfterHoursLockout]  = @EnforceAfterHoursLockout,
            [ModifiedAtUtc]             = @NowUtc
        WHERE [RestrictedZonePolicyId] = @RestrictedZonePolicyId
          AND [TenantId] = @TenantId;

        INSERT INTO dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime)
        VALUES ('PhysicalAccessControl.RestrictedZonePolicies', DB_NAME(), 'UPDATE', NULL, SYSUTCDATETIME());
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

        BEGIN TRY
            INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
            VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
            -- Error logging must never mask the original error
        END CATCH;

        THROW;
    END CATCH
END
GO
