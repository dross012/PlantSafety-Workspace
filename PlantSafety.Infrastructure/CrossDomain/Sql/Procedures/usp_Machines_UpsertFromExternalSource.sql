-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         CrossDomain.Machines
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_Machines_UpsertFromExternalSource.sql
--  Created:        2026-08-03
--  Author:         PlantSafety Platform Team (Phase 6 §1.1 Enterprise System Integrations, ERP)
--  SchemaVersion:  2026.1
--  Description:    Idempotent upsert procedure backing
--                  IMachineAssetReferenceDataIngestor.UpsertAsync -- the inbound ERP asset-master
--                  reference-data cache ingestion path for the projection-only CrossDomain.Machines
--                  bounded context (Handbook Part XXII). Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [CrossDomain].[usp_Machines_UpsertFromExternalSource]
    @MachineId          UNIQUEIDENTIFIER,
    @TenantId           UNIQUEIDENTIFIER,
    @SiteId             UNIQUEIDENTIFIER = NULL,
    @MachineName        NVARCHAR(200),
    @MachineType        NVARCHAR(100),
    @SerialNumber       NVARCHAR(100),
    @Status             NVARCHAR(50),
    @LastServicedAtUtc  DATETIME2(3),
    @LastServicedBy     UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;

BEGIN TRY

MERGE [CrossDomain].[Machines] AS target
USING (SELECT @MachineId AS MachineId, @TenantId AS TenantId) AS source
    ON target.MachineId = source.MachineId AND target.TenantId = source.TenantId
WHEN MATCHED THEN
    UPDATE SET
        SiteId              = @SiteId,
        MachineName         = @MachineName,
        MachineType         = @MachineType,
        SerialNumber        = @SerialNumber,
        Status              = @Status,
        LastServicedAtUtc   = @LastServicedAtUtc,
        LastServicedBy      = @LastServicedBy
WHEN NOT MATCHED THEN
    INSERT (MachineId, TenantId, SiteId, MachineName, MachineType, SerialNumber, Status, LastServicedAtUtc, LastServicedBy)
    VALUES (@MachineId, @TenantId, @SiteId, @MachineName, @MachineType, @SerialNumber, @Status, @LastServicedAtUtc, @LastServicedBy);

        -- Canonical audit trail (Handbook section 2.11): recorded after the write, in its own
        -- TRY/CATCH so an audit failure can never fail the operation it describes.
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'Machines', DB_NAME(), N'UPSERT', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'MachineId=', @MachineId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
END TRY
BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'MachineId=', @MachineId, N'; User=', SYSTEM_USER);
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
