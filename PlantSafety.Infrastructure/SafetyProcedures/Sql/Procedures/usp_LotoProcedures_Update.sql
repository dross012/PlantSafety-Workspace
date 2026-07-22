-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyProcedures.LotoProcedures
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_LotoProcedures_Update.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Full-row update procedure for [SafetyProcedures].[LotoProcedures], backing
--                  ILotoProceduresRepository.UpdateAsync (all lifecycle transitions persist the
--                  aggregate's full current state). Standard error envelope: TRY/CATCH with
--                  transaction rollback and THROW. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [SafetyProcedures].[usp_LotoProcedures_Update]
    @CorrelationId UNIQUEIDENTIFIER = NULL,
    @ActorId       UNIQUEIDENTIFIER = NULL,
    @NowUtc        DATETIME2(3),
    @LotoProceduresId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(100),
    @ProcedureTitle NVARCHAR(400),
    @EnergySourceTypes NVARCHAR(100),
    @IsolationPointDescription NVARCHAR(2000),
    @ApprovedByUserId UNIQUEIDENTIFIER,
    @PublishedAtUtc DATETIME2(3) = NULL,
    @NextAuditDueAtUtc DATETIME2(3) = NULL,
    @AuthorizedPersonnelCount INT,
    @ComplianceScore DECIMAL(9,2) = NULL,
    @CreatedAtUtc DATETIME2(3),
    @CreatedBy UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        UPDATE [SafetyProcedures].[LotoProcedures]
        SET
        [SiteId] = @SiteId,
        [Status] = @Status,
        [ProcedureTitle] = @ProcedureTitle,
        [EnergySourceTypes] = @EnergySourceTypes,
        [IsolationPointDescription] = @IsolationPointDescription,
        [ApprovedByUserId] = @ApprovedByUserId,
        [PublishedAtUtc] = @PublishedAtUtc,
        [NextAuditDueAtUtc] = @NextAuditDueAtUtc,
        [AuthorizedPersonnelCount] = @AuthorizedPersonnelCount,
        [ComplianceScore] = @ComplianceScore,
        [ModifiedAtUtc] = @NowUtc,
        [ModifiedBy] = @ActorId,
        [CorrelationId] = @CorrelationId
        WHERE [LotoProceduresId] = @LotoProceduresId
          AND [TenantId] = @TenantId
          AND ISNULL([IsDeleted], 0) = 0;
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
