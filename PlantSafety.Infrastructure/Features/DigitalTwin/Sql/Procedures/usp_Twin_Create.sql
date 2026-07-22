-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Features.DigitalTwin
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_Twin_Create.sql
--  Created:        2026-07-22
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Registers a new Digital Twin. Backs
--                  IDigitalTwinRepository.CreateAsync. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [DigitalTwin].[usp_Twin_Create]
    @TenantId      UNIQUEIDENTIFIER,
    @CorrelationId UNIQUEIDENTIFIER,
    @ActorId       UNIQUEIDENTIFIER,
    @NowUtc        DATETIME2(3),
    @TwinId        UNIQUEIDENTIFIER,
    @SiteId        UNIQUEIDENTIFIER = NULL,
    @AssetName     NVARCHAR(200),
    @AssetType     NVARCHAR(50)
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        INSERT INTO [DigitalTwin].[Twin]
            (TwinId, TenantId, SiteId, AssetName, AssetType, Status, LastSyncAtUtc, TelemetryHealthScore,
             IsDeleted, CorrelationId, CreatedAtUtc, CreatedBy)
        VALUES
            (@TwinId, @TenantId, @SiteId, @AssetName, @AssetType, N'Pending', @NowUtc, 0,
             0, @CorrelationId, @NowUtc, @ActorId);
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'TenantId=', @TenantId, N'; TwinId=', @TwinId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

        THROW;
    END CATCH
END
GO
