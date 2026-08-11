-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         EmployeeSafety.LotoPrograms
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_LotoPrograms_Update.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Full-row update procedure for [EmployeeSafety].[LotoPrograms], backing
--                  ILotoProgramsRepository.UpdateAsync (all lifecycle transitions persist the
--                  aggregate's full current state). Standard error envelope: TRY/CATCH with
--                  transaction rollback and THROW. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [EmployeeSafety].[usp_LotoPrograms_Update]
    @CorrelationId UNIQUEIDENTIFIER = NULL,
    @ActorId       UNIQUEIDENTIFIER = NULL,
    @NowUtc        DATETIME2(3),
    @LotoProgramsId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(100),
    @ProgramTitle NVARCHAR(400),
    @EquipmentTag NVARCHAR(400),
    @EnergySourceTypes NVARCHAR(100),
    @IsolationPointDescription NVARCHAR(2000),
    @IsolationPointCount INT,
    @AuthorizedByUserId UNIQUEIDENTIFIER,
    @LocksAppliedCount INT,
    @VerifiedByUserId UNIQUEIDENTIFIER = NULL,
    @VerifiedZeroEnergyAtUtc DATETIME2(3) = NULL,
    @ReleasedByUserId UNIQUEIDENTIFIER = NULL,
    @ReleasedAtUtc DATETIME2(3) = NULL,
    @LastAuditedAtUtc DATETIME2(3) = NULL,
    @ProgramComplianceScore DECIMAL(9,2) = NULL,
    @NonComplianceNotes NVARCHAR(2000) = NULL,
    @CreatedAtUtc DATETIME2(3),
    @CreatedBy UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        UPDATE [EmployeeSafety].[LotoPrograms]
        SET
        [SiteId] = @SiteId,
        [Status] = @Status,
        [ProgramTitle] = @ProgramTitle,
        [EquipmentTag] = @EquipmentTag,
        [EnergySourceTypes] = @EnergySourceTypes,
        [IsolationPointDescription] = @IsolationPointDescription,
        [IsolationPointCount] = @IsolationPointCount,
        [AuthorizedByUserId] = @AuthorizedByUserId,
        [LocksAppliedCount] = @LocksAppliedCount,
        [VerifiedByUserId] = @VerifiedByUserId,
        [VerifiedZeroEnergyAtUtc] = @VerifiedZeroEnergyAtUtc,
        [ReleasedByUserId] = @ReleasedByUserId,
        [ReleasedAtUtc] = @ReleasedAtUtc,
        [LastAuditedAtUtc] = @LastAuditedAtUtc,
        [ProgramComplianceScore] = @ProgramComplianceScore,
        [NonComplianceNotes] = @NonComplianceNotes,
        [ModifiedAtUtc] = @NowUtc,
        [ModifiedBy] = @ActorId,
        [CorrelationId] = @CorrelationId
        WHERE [LotoProgramsId] = @LotoProgramsId
          AND [TenantId] = @TenantId
          AND ISNULL([IsDeleted], 0) = 0;
    COMMIT TRANSACTION;
        -- Canonical audit trail (Handbook §2.11): the write is recorded after it commits, in its
        -- own TRY/CATCH so an audit failure can never fail the operation it is describing.
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'LotoPrograms', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'LotoProgramsId=', @LotoProgramsId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'LotoProgramsId=', @LotoProgramsId, N'; User=', SYSTEM_USER);
    DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

    INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
    VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

    THROW;
END CATCH;
GO
