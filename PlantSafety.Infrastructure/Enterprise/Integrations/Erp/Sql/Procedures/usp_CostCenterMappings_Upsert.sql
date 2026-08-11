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

BEGIN TRY

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

        -- Canonical audit trail (Handbook section 2.11): recorded after the write, in its own
        -- TRY/CATCH so an audit failure can never fail the operation it describes.
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'CostCenterMappings', DB_NAME(), N'UPSERT', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'TenantId=', @TenantId, N'; SiteId=', ISNULL(CONVERT(NVARCHAR(36), @SiteId), N'(none)'), N'; ExternalCostCenterCode=', @ExternalCostCenterCode), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
END TRY
BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'TenantId=', @TenantId, N'; ExternalCostCenterCode=', @ExternalCostCenterCode, N'; User=', SYSTEM_USER);
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
GO
