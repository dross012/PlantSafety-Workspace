-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         MachineSafety.CraneHoistSystems
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_CraneHoistSystems_Create.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Insert procedure for [MachineSafety].[CraneHoistSystems], backing
--                  ICraneHoistSystemsRepository.AddAsync. Standard error envelope: TRY/CATCH with
--                  transaction rollback and THROW. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [MachineSafety].[usp_CraneHoistSystems_Create]
    @CorrelationId UNIQUEIDENTIFIER = NULL,
    @ActorId       UNIQUEIDENTIFIER = NULL,
    @CraneHoistSystemsId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(100),
    @AssetTag NVARCHAR(400),
    @AssetTitle NVARCHAR(400),
    @AssetType NVARCHAR(100),
    @RatedLoadCapacityKg DECIMAL(12,2),
    @LastObservedLoadKg DECIMAL(12,2) = NULL,
    @LastLiftAtUtc DATETIME2(3) = NULL,
    @OverloadEventCount INT,
    @OverloadFlagged BIT,
    @InspectionDueAtUtc DATETIME2(3) = NULL,
    @LastInspectionAtUtc DATETIME2(3) = NULL,
    @LastInspectionPassed BIT = NULL,
    @OperatorUserId UNIQUEIDENTIFIER,
    @OperatorQualifiedAtUtc DATETIME2(3) = NULL,
    @OperatorQualificationExpiresAtUtc DATETIME2(3) = NULL,
    @OperatorQualificationSuspended BIT,
    @LastRiggingInspectionAtUtc DATETIME2(3) = NULL,
    @LastRiggingDefectDetected BIT = NULL,
    @CreatedAtUtc DATETIME2(3),
    @CreatedBy UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        INSERT INTO [MachineSafety].[CraneHoistSystems]
            ([CraneHoistSystemsId], [TenantId], [SiteId], [Status], [AssetTag], [AssetTitle], [AssetType], [RatedLoadCapacityKg], [LastObservedLoadKg], [LastLiftAtUtc], [OverloadEventCount], [OverloadFlagged], [InspectionDueAtUtc], [LastInspectionAtUtc], [LastInspectionPassed], [OperatorUserId], [OperatorQualifiedAtUtc], [OperatorQualificationExpiresAtUtc], [OperatorQualificationSuspended], [LastRiggingInspectionAtUtc], [LastRiggingDefectDetected], [CreatedAtUtc], [CreatedBy], [CorrelationId], [ActorId])
        VALUES
            (@CraneHoistSystemsId, @TenantId, @SiteId, @Status, @AssetTag, @AssetTitle, @AssetType, @RatedLoadCapacityKg, @LastObservedLoadKg, @LastLiftAtUtc, @OverloadEventCount, @OverloadFlagged, @InspectionDueAtUtc, @LastInspectionAtUtc, @LastInspectionPassed, @OperatorUserId, @OperatorQualifiedAtUtc, @OperatorQualificationExpiresAtUtc, @OperatorQualificationSuspended, @LastRiggingInspectionAtUtc, @LastRiggingDefectDetected, @CreatedAtUtc, @CreatedBy, @CorrelationId, @ActorId);
    COMMIT TRANSACTION;
        -- Canonical audit trail (Handbook §2.11): the write is recorded after it commits, in its
        -- own TRY/CATCH so an audit failure can never fail the operation it is describing.
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'CraneHoistSystems', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'CraneHoistSystemsId=', @CraneHoistSystemsId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'CraneHoistSystemsId=', @CraneHoistSystemsId, N'; User=', SYSTEM_USER);
    DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

    INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
    VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

    THROW;
END CATCH;
GO
