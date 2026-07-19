USE [PlantSafety.PredictiveDB]

GO

SET ANSI_NULLS ON

GO

SET QUOTED_IDENTIFIER ON

GO

-- =============================================

-- Phase 6: Stored Procedures (canonical FY2027 template)

-- Database: PlantSafety.PredictiveDB

-- Generated: 2026-07-19 by Procedure Architect agent, grounded in

-- sql-database-skill\references\00-canonical-templates.md

-- Idempotent: CREATE OR ALTER PROCEDURE throughout.

-- =============================================

GO



-- =============================================
-- Procedure:     [PredictiveSafety].sp_PredictionEvidence_Create
-- Description:   Inserts a new PredictionEvidence record (insert-once; no Update proc, since table has no UpdatedUtc/ConcurrencyToken).
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [PredictiveSafety].sp_PredictionEvidence_Create
    @PredictionEvidenceId uniqueidentifier,
    @SafetyPredictionId uniqueidentifier,
    @EvidenceType nvarchar(100),
    @Description nvarchar(1000),
    @Explanation nvarchar(1000),
    @Weight decimal(5,4),
    @RecordedAtUtc datetime2(3),
    @CreatedBy uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @PredictionEvidenceId IS NULL
            THROW 50001, N'PredictionEvidenceId is required.', 1;
        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'PredictionEvidence', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'PredictionEvidenceId=', @PredictionEvidenceId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        INSERT INTO [PredictiveSafety].[PredictionEvidence] (PredictionEvidenceId, SafetyPredictionId, EvidenceType, Description, Explanation, Weight, RecordedAtUtc, CreatedBy, IsDeleted, CreatedUtc)
        VALUES (@PredictionEvidenceId, @SafetyPredictionId, @EvidenceType, @Description, @Explanation, @Weight, @RecordedAtUtc, @CreatedBy, 0, SYSUTCDATETIME());
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'PredictionEvidenceId=', @PredictionEvidenceId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(
            N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorState=', ERROR_STATE(), N'; ',
            N'ErrorSeverity=', ERROR_SEVERITY(), N'; ErrorProcedure=', ISNULL(ERROR_PROCEDURE(), N'NULL'), N'; ',
            N'ErrorLine=', ERROR_LINE(), N'; ErrorMessage=', ERROR_MESSAGE());
        DECLARE @__SourceDatabase nvarchar(128) = DB_NAME();
        DECLARE @__Module nvarchar(128) = OBJECT_NAME(@@PROCID);

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
            ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
            ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
        )
        VALUES (
            ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
            SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, @__SourceDatabase, @__Module
        );

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                BeforeSnapshot, AfterSnapshot, ErrorContext, Severity, Module, ViewerAccessed
            )
            VALUES (
                N'PredictionEvidence', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
                NULL, NULL, @__ErrorContext, ERROR_SEVERITY(), OBJECT_NAME(@@PROCID), 0
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary error propagation
        END CATCH;

        THROW;
    END CATCH
END
GO

-- =============================================
-- Procedure:     [PredictiveSafety].sp_PredictionEvidence_GetById
-- Description:   Retrieves a single PredictionEvidence record by primary key.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [PredictiveSafety].sp_PredictionEvidence_GetById
    @PredictionEvidenceId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @PredictionEvidenceId IS NULL
            THROW 50001, N'PredictionEvidenceId is required.', 1;


        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'PredictionEvidence', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'PredictionEvidenceId=', @PredictionEvidenceId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.PredictionEvidenceId, e.SafetyPredictionId, e.EvidenceType, e.Description, e.Explanation, e.Weight, e.RecordedAtUtc, e.CreatedUtc, e.CreatedBy, e.IsDeleted
        FROM [PredictiveSafety].[PredictionEvidence] AS e
        WHERE e.PredictionEvidenceId = @PredictionEvidenceId
          AND ISNULL(e.IsDeleted, 0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'PredictionEvidenceId=', @PredictionEvidenceId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(
            N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorState=', ERROR_STATE(), N'; ',
            N'ErrorSeverity=', ERROR_SEVERITY(), N'; ErrorProcedure=', ISNULL(ERROR_PROCEDURE(), N'NULL'), N'; ',
            N'ErrorLine=', ERROR_LINE(), N'; ErrorMessage=', ERROR_MESSAGE());
        DECLARE @__SourceDatabase nvarchar(128) = DB_NAME();
        DECLARE @__Module nvarchar(128) = OBJECT_NAME(@@PROCID);

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
            ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
            ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
        )
        VALUES (
            ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
            SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, @__SourceDatabase, @__Module
        );

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                BeforeSnapshot, AfterSnapshot, ErrorContext, Severity, Module, ViewerAccessed
            )
            VALUES (
                N'PredictionEvidence', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                NULL, NULL, @__ErrorContext, ERROR_SEVERITY(), OBJECT_NAME(@@PROCID), 0
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary error propagation
        END CATCH;

        THROW;
    END CATCH
END
GO

-- =============================================
-- Procedure:     [PredictiveSafety].sp_PredictionEvidence_List
-- Description:   Returns a paginated, soft-delete-filtered list of PredictionEvidence records.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [PredictiveSafety].sp_PredictionEvidence_List
    @PageNumber int = 1,
    @PageSize int = 50
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @PageNumber IS NULL OR @PageNumber < 1
            THROW 50001, N'PageNumber must be a positive integer.', 1;
        IF @PageSize IS NULL OR @PageSize < 1
            THROW 50001, N'PageSize must be a positive integer.', 1;
        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'PredictionEvidence', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'PageNumber=', @PageNumber, N'; PageSize=', @PageSize), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.PredictionEvidenceId, e.SafetyPredictionId, e.EvidenceType, e.Description, e.Explanation, e.Weight, e.RecordedAtUtc, e.CreatedUtc, e.CreatedBy, e.IsDeleted
        FROM [PredictiveSafety].[PredictionEvidence] AS e
        WHERE ISNULL(e.IsDeleted, 0) = 0
        ORDER BY e.PredictionEvidenceId
        OFFSET (@PageNumber - 1) * @PageSize ROWS
        FETCH NEXT @PageSize ROWS ONLY;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'PageNumber=', @PageNumber, N'; PageSize=', @PageSize, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(
            N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorState=', ERROR_STATE(), N'; ',
            N'ErrorSeverity=', ERROR_SEVERITY(), N'; ErrorProcedure=', ISNULL(ERROR_PROCEDURE(), N'NULL'), N'; ',
            N'ErrorLine=', ERROR_LINE(), N'; ErrorMessage=', ERROR_MESSAGE());
        DECLARE @__SourceDatabase nvarchar(128) = DB_NAME();
        DECLARE @__Module nvarchar(128) = OBJECT_NAME(@@PROCID);

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
            ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
            ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
        )
        VALUES (
            ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
            SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, @__SourceDatabase, @__Module
        );

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                BeforeSnapshot, AfterSnapshot, ErrorContext, Severity, Module, ViewerAccessed
            )
            VALUES (
                N'PredictionEvidence', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                NULL, NULL, @__ErrorContext, ERROR_SEVERITY(), OBJECT_NAME(@@PROCID), 0
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary error propagation
        END CATCH;

        THROW;
    END CATCH
END
GO

-- =============================================
-- Procedure:     [PredictiveSafety].sp_PredictionEvidence_Delete
-- Description:   Soft-deletes a PredictionEvidence record (sets IsDeleted = 1). Never a hard DELETE.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [PredictiveSafety].sp_PredictionEvidence_Delete
    @PredictionEvidenceId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @PredictionEvidenceId IS NULL
            THROW 50001, N'PredictionEvidenceId is required.', 1;


        IF NOT EXISTS (SELECT 1 FROM [PredictiveSafety].[PredictionEvidence] WHERE PredictionEvidenceId = @PredictionEvidenceId AND ISNULL(IsDeleted,0) = 0)
            THROW 50004, N'Record not found or already deleted.', 1;
        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'PredictionEvidence', DB_NAME(), N'DELETE', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'PredictionEvidenceId=', @PredictionEvidenceId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation (soft delete only)
        ------------------------------------------------------------
        UPDATE [PredictiveSafety].[PredictionEvidence]
        SET IsDeleted = 1
        WHERE PredictionEvidenceId = @PredictionEvidenceId
          AND ISNULL(IsDeleted,0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'PredictionEvidenceId=', @PredictionEvidenceId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(
            N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorState=', ERROR_STATE(), N'; ',
            N'ErrorSeverity=', ERROR_SEVERITY(), N'; ErrorProcedure=', ISNULL(ERROR_PROCEDURE(), N'NULL'), N'; ',
            N'ErrorLine=', ERROR_LINE(), N'; ErrorMessage=', ERROR_MESSAGE());
        DECLARE @__SourceDatabase nvarchar(128) = DB_NAME();
        DECLARE @__Module nvarchar(128) = OBJECT_NAME(@@PROCID);

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
            ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
            ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
        )
        VALUES (
            ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
            SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, @__SourceDatabase, @__Module
        );

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                BeforeSnapshot, AfterSnapshot, ErrorContext, Severity, Module, ViewerAccessed
            )
            VALUES (
                N'PredictionEvidence', DB_NAME(), N'DELETE', SYSTEM_USER, SYSUTCDATETIME(),
                NULL, NULL, @__ErrorContext, ERROR_SEVERITY(), OBJECT_NAME(@@PROCID), 0
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary error propagation
        END CATCH;

        THROW;
    END CATCH
END
GO

-- =============================================
-- Procedure:     [PredictiveSafety].sp_SafetyPrediction_Create
-- Description:   Inserts a new SafetyPrediction record.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [PredictiveSafety].sp_SafetyPrediction_Create
    @SafetyPredictionId uniqueidentifier,
    @Subject nvarchar(200),
    @Status nvarchar(30),
    @ModelName nvarchar(200),
    @ModelVersion nvarchar(50),
    @ConfidenceScore decimal(5,4),
    @Explanation nvarchar(MAX),
    @HumanAgreed bit,
    @ClosedAtUtc datetime2(3),
    @RequestedByActorId nvarchar(200),
    @CorrelationId uniqueidentifier,
    @CreatedUtc datetime2(3),
    @CreatedBy uniqueidentifier,
    @UpdatedBy uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @CreatedUtc IS NULL
            THROW 50001, N'CreatedUtc is required.', 1;
        IF @SafetyPredictionId IS NULL
            THROW 50001, N'SafetyPredictionId is required.', 1;
        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'SafetyPrediction', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'SafetyPredictionId=', @SafetyPredictionId, N'; ', N'Subject=', @Subject, N'; ', N'Status=', @Status), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        INSERT INTO [PredictiveSafety].[SafetyPrediction] (SafetyPredictionId, Subject, Status, ModelName, ModelVersion, ConfidenceScore, Explanation, HumanAgreed, ClosedAtUtc, RequestedByActorId, CorrelationId, CreatedUtc, CreatedBy, UpdatedBy, IsDeleted)
        VALUES (@SafetyPredictionId, @Subject, @Status, @ModelName, @ModelVersion, @ConfidenceScore, @Explanation, @HumanAgreed, @ClosedAtUtc, @RequestedByActorId, @CorrelationId, @CreatedUtc, @CreatedBy, @UpdatedBy, 0);
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'SafetyPredictionId=', @SafetyPredictionId, N'; ', N'Subject=', @Subject, N'; ', N'Status=', @Status, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(
            N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorState=', ERROR_STATE(), N'; ',
            N'ErrorSeverity=', ERROR_SEVERITY(), N'; ErrorProcedure=', ISNULL(ERROR_PROCEDURE(), N'NULL'), N'; ',
            N'ErrorLine=', ERROR_LINE(), N'; ErrorMessage=', ERROR_MESSAGE());
        DECLARE @__SourceDatabase nvarchar(128) = DB_NAME();
        DECLARE @__Module nvarchar(128) = OBJECT_NAME(@@PROCID);

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
            ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
            ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
        )
        VALUES (
            ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
            SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, @__SourceDatabase, @__Module
        );

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                BeforeSnapshot, AfterSnapshot, ErrorContext, Severity, Module, ViewerAccessed
            )
            VALUES (
                N'SafetyPrediction', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
                NULL, NULL, @__ErrorContext, ERROR_SEVERITY(), OBJECT_NAME(@@PROCID), 0
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary error propagation
        END CATCH;

        THROW;
    END CATCH
END
GO

-- =============================================
-- Procedure:     [PredictiveSafety].sp_SafetyPrediction_Update
-- Description:   Updates an existing SafetyPrediction record with optimistic concurrency.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [PredictiveSafety].sp_SafetyPrediction_Update
    @CreatedUtc datetime2(3),
    @SafetyPredictionId uniqueidentifier,
    @Subject nvarchar(200),
    @Status nvarchar(30),
    @ModelName nvarchar(200),
    @ModelVersion nvarchar(50),
    @ConfidenceScore decimal(5,4),
    @Explanation nvarchar(MAX),
    @HumanAgreed bit,
    @ClosedAtUtc datetime2(3),
    @RequestedByActorId nvarchar(200),
    @CorrelationId uniqueidentifier,
    @UpdatedBy uniqueidentifier,
    @ConcurrencyToken rowversion
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @CreatedUtc IS NULL
            THROW 50001, N'CreatedUtc is required.', 1;

        IF @SafetyPredictionId IS NULL
            THROW 50001, N'SafetyPredictionId is required.', 1;


        IF NOT EXISTS (SELECT 1 FROM [PredictiveSafety].[SafetyPrediction] WHERE CreatedUtc = @CreatedUtc AND SafetyPredictionId = @SafetyPredictionId AND ISNULL(IsDeleted,0) = 0)
            THROW 50004, N'Record not found or has been deleted.', 1;

        IF NOT EXISTS (SELECT 1 FROM [PredictiveSafety].[SafetyPrediction] WHERE CreatedUtc = @CreatedUtc AND SafetyPredictionId = @SafetyPredictionId AND ConcurrencyToken = @ConcurrencyToken)
            THROW 50002, N'Concurrency conflict: the record was modified by another process.', 1;
        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'SafetyPrediction', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'CreatedUtc=', @CreatedUtc, N'; ', N'SafetyPredictionId=', @SafetyPredictionId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        UPDATE [PredictiveSafety].[SafetyPrediction]
        SET
            Subject = @Subject,
            Status = @Status,
            ModelName = @ModelName,
            ModelVersion = @ModelVersion,
            ConfidenceScore = @ConfidenceScore,
            Explanation = @Explanation,
            HumanAgreed = @HumanAgreed,
            ClosedAtUtc = @ClosedAtUtc,
            RequestedByActorId = @RequestedByActorId,
            CorrelationId = @CorrelationId,
            UpdatedBy = @UpdatedBy,
            UpdatedUtc = SYSUTCDATETIME()
        WHERE CreatedUtc = @CreatedUtc AND SafetyPredictionId = @SafetyPredictionId
          AND ISNULL(IsDeleted,0) = 0 AND ConcurrencyToken = @ConcurrencyToken;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'CreatedUtc=', @CreatedUtc, N'; ', N'SafetyPredictionId=', @SafetyPredictionId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(
            N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorState=', ERROR_STATE(), N'; ',
            N'ErrorSeverity=', ERROR_SEVERITY(), N'; ErrorProcedure=', ISNULL(ERROR_PROCEDURE(), N'NULL'), N'; ',
            N'ErrorLine=', ERROR_LINE(), N'; ErrorMessage=', ERROR_MESSAGE());
        DECLARE @__SourceDatabase nvarchar(128) = DB_NAME();
        DECLARE @__Module nvarchar(128) = OBJECT_NAME(@@PROCID);

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
            ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
            ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
        )
        VALUES (
            ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
            SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, @__SourceDatabase, @__Module
        );

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                BeforeSnapshot, AfterSnapshot, ErrorContext, Severity, Module, ViewerAccessed
            )
            VALUES (
                N'SafetyPrediction', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(),
                NULL, NULL, @__ErrorContext, ERROR_SEVERITY(), OBJECT_NAME(@@PROCID), 0
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary error propagation
        END CATCH;

        THROW;
    END CATCH
END
GO

-- =============================================
-- Procedure:     [PredictiveSafety].sp_SafetyPrediction_GetById
-- Description:   Retrieves a single SafetyPrediction record by primary key.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [PredictiveSafety].sp_SafetyPrediction_GetById
    @CreatedUtc datetime2(3),
    @SafetyPredictionId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @CreatedUtc IS NULL
            THROW 50001, N'CreatedUtc is required.', 1;

        IF @SafetyPredictionId IS NULL
            THROW 50001, N'SafetyPredictionId is required.', 1;


        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'SafetyPrediction', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'CreatedUtc=', @CreatedUtc, N'; ', N'SafetyPredictionId=', @SafetyPredictionId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.SafetyPredictionId, e.Subject, e.Status, e.ModelName, e.ModelVersion, e.ConfidenceScore, e.Explanation, e.HumanAgreed, e.ClosedAtUtc, e.RequestedByActorId, e.CorrelationId, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.ConcurrencyToken
        FROM [PredictiveSafety].[SafetyPrediction] AS e
        WHERE e.CreatedUtc = @CreatedUtc AND e.SafetyPredictionId = @SafetyPredictionId
          AND ISNULL(e.IsDeleted, 0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'CreatedUtc=', @CreatedUtc, N'; ', N'SafetyPredictionId=', @SafetyPredictionId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(
            N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorState=', ERROR_STATE(), N'; ',
            N'ErrorSeverity=', ERROR_SEVERITY(), N'; ErrorProcedure=', ISNULL(ERROR_PROCEDURE(), N'NULL'), N'; ',
            N'ErrorLine=', ERROR_LINE(), N'; ErrorMessage=', ERROR_MESSAGE());
        DECLARE @__SourceDatabase nvarchar(128) = DB_NAME();
        DECLARE @__Module nvarchar(128) = OBJECT_NAME(@@PROCID);

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
            ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
            ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
        )
        VALUES (
            ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
            SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, @__SourceDatabase, @__Module
        );

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                BeforeSnapshot, AfterSnapshot, ErrorContext, Severity, Module, ViewerAccessed
            )
            VALUES (
                N'SafetyPrediction', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                NULL, NULL, @__ErrorContext, ERROR_SEVERITY(), OBJECT_NAME(@@PROCID), 0
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary error propagation
        END CATCH;

        THROW;
    END CATCH
END
GO

-- =============================================
-- Procedure:     [PredictiveSafety].sp_SafetyPrediction_List
-- Description:   Returns a paginated, soft-delete-filtered list of SafetyPrediction records.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [PredictiveSafety].sp_SafetyPrediction_List
    @PageNumber int = 1,
    @PageSize int = 50
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @PageNumber IS NULL OR @PageNumber < 1
            THROW 50001, N'PageNumber must be a positive integer.', 1;
        IF @PageSize IS NULL OR @PageSize < 1
            THROW 50001, N'PageSize must be a positive integer.', 1;
        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'SafetyPrediction', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'PageNumber=', @PageNumber, N'; PageSize=', @PageSize), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.SafetyPredictionId, e.Subject, e.Status, e.ModelName, e.ModelVersion, e.ConfidenceScore, e.Explanation, e.HumanAgreed, e.ClosedAtUtc, e.RequestedByActorId, e.CorrelationId, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.ConcurrencyToken
        FROM [PredictiveSafety].[SafetyPrediction] AS e
        WHERE ISNULL(e.IsDeleted, 0) = 0
        ORDER BY e.CreatedUtc
        OFFSET (@PageNumber - 1) * @PageSize ROWS
        FETCH NEXT @PageSize ROWS ONLY;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'PageNumber=', @PageNumber, N'; PageSize=', @PageSize, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(
            N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorState=', ERROR_STATE(), N'; ',
            N'ErrorSeverity=', ERROR_SEVERITY(), N'; ErrorProcedure=', ISNULL(ERROR_PROCEDURE(), N'NULL'), N'; ',
            N'ErrorLine=', ERROR_LINE(), N'; ErrorMessage=', ERROR_MESSAGE());
        DECLARE @__SourceDatabase nvarchar(128) = DB_NAME();
        DECLARE @__Module nvarchar(128) = OBJECT_NAME(@@PROCID);

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
            ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
            ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
        )
        VALUES (
            ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
            SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, @__SourceDatabase, @__Module
        );

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                BeforeSnapshot, AfterSnapshot, ErrorContext, Severity, Module, ViewerAccessed
            )
            VALUES (
                N'SafetyPrediction', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                NULL, NULL, @__ErrorContext, ERROR_SEVERITY(), OBJECT_NAME(@@PROCID), 0
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary error propagation
        END CATCH;

        THROW;
    END CATCH
END
GO

-- =============================================
-- Procedure:     [PredictiveSafety].sp_SafetyPrediction_Delete
-- Description:   Soft-deletes a SafetyPrediction record (sets IsDeleted = 1). Never a hard DELETE.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [PredictiveSafety].sp_SafetyPrediction_Delete
    @CreatedUtc datetime2(3),
    @SafetyPredictionId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @CreatedUtc IS NULL
            THROW 50001, N'CreatedUtc is required.', 1;

        IF @SafetyPredictionId IS NULL
            THROW 50001, N'SafetyPredictionId is required.', 1;


        IF NOT EXISTS (SELECT 1 FROM [PredictiveSafety].[SafetyPrediction] WHERE CreatedUtc = @CreatedUtc AND SafetyPredictionId = @SafetyPredictionId AND ISNULL(IsDeleted,0) = 0)
            THROW 50004, N'Record not found or already deleted.', 1;
        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'SafetyPrediction', DB_NAME(), N'DELETE', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'CreatedUtc=', @CreatedUtc, N'; ', N'SafetyPredictionId=', @SafetyPredictionId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation (soft delete only)
        ------------------------------------------------------------
        UPDATE [PredictiveSafety].[SafetyPrediction]
        SET IsDeleted = 1,
            UpdatedUtc = SYSUTCDATETIME()
        WHERE CreatedUtc = @CreatedUtc AND SafetyPredictionId = @SafetyPredictionId
          AND ISNULL(IsDeleted,0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'CreatedUtc=', @CreatedUtc, N'; ', N'SafetyPredictionId=', @SafetyPredictionId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(
            N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorState=', ERROR_STATE(), N'; ',
            N'ErrorSeverity=', ERROR_SEVERITY(), N'; ErrorProcedure=', ISNULL(ERROR_PROCEDURE(), N'NULL'), N'; ',
            N'ErrorLine=', ERROR_LINE(), N'; ErrorMessage=', ERROR_MESSAGE());
        DECLARE @__SourceDatabase nvarchar(128) = DB_NAME();
        DECLARE @__Module nvarchar(128) = OBJECT_NAME(@@PROCID);

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
            ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
            ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
        )
        VALUES (
            ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
            SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, @__SourceDatabase, @__Module
        );

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                BeforeSnapshot, AfterSnapshot, ErrorContext, Severity, Module, ViewerAccessed
            )
            VALUES (
                N'SafetyPrediction', DB_NAME(), N'DELETE', SYSTEM_USER, SYSUTCDATETIME(),
                NULL, NULL, @__ErrorContext, ERROR_SEVERITY(), OBJECT_NAME(@@PROCID), 0
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary error propagation
        END CATCH;

        THROW;
    END CATCH
END
GO
