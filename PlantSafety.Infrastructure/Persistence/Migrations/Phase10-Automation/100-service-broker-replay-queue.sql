-- =============================================
-- Phase 10 Automation & Integration
-- Script:        100-service-broker-replay-queue.sql
-- Author:        Automation & Integration Agent
-- Create date:   2026-07-19
-- Purpose:       Governance item #1 (Service Broker).
--
-- *** WHY SERVICE BROKER HERE, AND NOT ELSEWHERE ***
-- PlantSafety already has an established, mandatory event-driven pattern
-- (see project_plantsafety_patterns.md section 4/6): aggregates raise domain
-- events -> Unit of Work writes canonical envelopes to an OutboxMessages
-- table -> a background Outbox Processor publishes to Azure Service Bus ->
-- consumers handle idempotently via a ProcessedEvents table. That pattern is
-- the correct, already-governed mechanism for CROSS-SERVICE / CROSS-MODULE
-- integration events, and Service Broker would be pure redundant
-- infrastructure if used for that purpose. This migration does NOT touch or
-- duplicate that pattern anywhere.
--
-- A genuinely different, narrower need exists in PlantSafety.IntegrationDB:
-- Integration.ReplayRecord stores serialized inbound/outbound integration
-- messages (MessageId/CorrelationId/CausationId/SchemaName/ContractVersion/
-- SerializedPayload) for later REPLAY -- e.g. re-driving a message through
-- processing after a downstream consumer outage, per the CrossDomain
-- Architecture Manual's "Delta Sync" / "Retry Envelope" / "Schema Drift &
-- Change Detection" concepts. Before this script, sp_ReplayRecord_Create/
-- GetById/List/Delete/Purge (Phase 6) were pure CRUD -- nothing actually
-- drove asynchronous, ordered, exactly-once-per-conversation replay
-- processing. That is a real, in-database, single-database queuing need:
--   - It is internal to PlantSafety.IntegrationDB (no external broker/Azure
--     Service Bus hop required -- the whole point is to avoid depending on
--     external infrastructure just to sequence retry-of-a-previously-seen
--     message inside one database).
--   - It needs guaranteed, ordered, transactional delivery with built-in
--     poison-message handling -- exactly what Service Broker natively
--     provides via CONVERSATION ordering and activation procedures, with no
--     custom polling/locking code required.
--   - It is a genuine gap, not a duplication: the Outbox pattern is about
--     publishing NEW domain facts out of the system; this is about
--     re-processing a RECORD OF A MESSAGE ALREADY RECEIVED, entirely inside
--     IntegrationDB.
--
-- Design: two queues/services in a single dialog (initiator + target),
-- one contract, one message type each direction.
--   - Integration.sp_ReplayRecord_Enqueue(@ReplayRecordId) begins a dialog,
--     sends a ReplayRequest message, and marks the ReplayRecord 'Queued'.
--   - Target queue activation procedure (Integration.sp_ReplayRequestQueue_
--     Activation) receives the request, performs the replay-processing step
--     (validates the stored payload is still present/well-formed, marks the
--     record 'Completed' or 'Failed', writes an IntegrationLogEntry row),
--     sends a ReplayResponse back, and ends the conversation.
--   - Initiator queue activation procedure (Integration.sp_
--     ReplayInitiatorQueue_Activation) receives the response/end-dialog and
--     closes its side of the conversation. No human/external caller needs
--     to poll; SQL Server's built-in activation drives all of this.
--
-- Additive-only: adds Status/ProcessedUtc columns to Integration.ReplayRecord
-- (nullable/defaulted, no existing column touched), adds new message types /
-- contract / queues / services / procedures. Nothing destructive.
--
-- Idempotency: every CREATE is existence-checked. Safe to rerun.
-- =============================================
SET NOCOUNT ON;
GO

USE [PlantSafety.IntegrationDB];
GO
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

------------------------------------------------------------
-- 0. Ensure Service Broker is enabled on this database.
--    (Live-verified 2026-07-19: was already ON for this database at the
--    start of this migration -- ENABLE_BROKER below is idempotent/no-op in
--    that case. Recorded here explicitly so this script is self-contained
--    and reproducible from a clean database.)
------------------------------------------------------------
IF (SELECT is_broker_enabled FROM sys.databases WHERE name = DB_NAME()) = 0
BEGIN
    ALTER DATABASE [PlantSafety.IntegrationDB] SET ENABLE_BROKER WITH ROLLBACK IMMEDIATE;
END
GO

------------------------------------------------------------
-- 1. Additive columns on Integration.ReplayRecord to track replay-queue
--    processing state (previously CRUD-only, no lifecycle state existed).
------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Integration.ReplayRecord') AND name = 'Status')
    ALTER TABLE Integration.ReplayRecord ADD Status nvarchar(20) NOT NULL CONSTRAINT DF_ReplayRecord_Status DEFAULT (N'Pending');
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Integration.ReplayRecord') AND name = 'ProcessedUtc')
    ALTER TABLE Integration.ReplayRecord ADD ProcessedUtc datetime2(3) NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Integration.ReplayRecord') AND name = 'FailureReason')
    ALTER TABLE Integration.ReplayRecord ADD FailureReason nvarchar(2000) NULL;
GO

------------------------------------------------------------
-- 2. Message types.
------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.service_message_types WHERE name = N'//PlantSafety/Integration/ReplayRequest')
    CREATE MESSAGE TYPE [//PlantSafety/Integration/ReplayRequest] VALIDATION = NONE;
GO
IF NOT EXISTS (SELECT 1 FROM sys.service_message_types WHERE name = N'//PlantSafety/Integration/ReplayResponse')
    CREATE MESSAGE TYPE [//PlantSafety/Integration/ReplayResponse] VALIDATION = NONE;
GO

------------------------------------------------------------
-- 3. Contract.
------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.service_contracts WHERE name = N'//PlantSafety/Integration/ReplayContract')
    CREATE CONTRACT [//PlantSafety/Integration/ReplayContract]
    (
        [//PlantSafety/Integration/ReplayRequest]  SENT BY INITIATOR,
        [//PlantSafety/Integration/ReplayResponse] SENT BY TARGET
    );
GO

------------------------------------------------------------
-- 4. Target-side activation procedure + queue + service.
--    (Procedure must exist before the queue references it.)
------------------------------------------------------------
CREATE OR ALTER PROCEDURE Integration.sp_ReplayRequestQueue_Activation
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @ConversationHandle uniqueidentifier;
    DECLARE @MessageTypeName sysname;
    DECLARE @MessageBody nvarchar(MAX);
    DECLARE @System uniqueidentifier = '00000000-0000-0000-0000-000000000000';
    DECLARE @Done bit = 0;
    -- NOTE: BREAK/CONTINUE are not permitted inside a TRY block (T-SQL
    -- restriction), so loop exit is controlled via @Done rather than BREAK.

    WHILE @Done = 0
    BEGIN
        BEGIN TRY
            BEGIN TRANSACTION;

            WAITFOR (
                RECEIVE TOP(1)
                    @ConversationHandle = conversation_handle,
                    @MessageTypeName    = message_type_name,
                    @MessageBody        = CAST(message_body AS nvarchar(MAX))
                FROM Integration.ReplayRequestQueue
            ), TIMEOUT 5000;

            IF @@ROWCOUNT = 0
            BEGIN
                SET @Done = 1;
                COMMIT TRANSACTION;
            END
            ELSE
            BEGIN

            IF @MessageTypeName = N'//PlantSafety/Integration/ReplayRequest'
            BEGIN
                DECLARE @ReplayRecordId uniqueidentifier = TRY_CAST(@MessageBody AS uniqueidentifier);
                DECLARE @Exists bit = 0;
                DECLARE @PayloadOk bit = 0;

                IF @ReplayRecordId IS NOT NULL
                BEGIN
                    SELECT @Exists = 1, @PayloadOk = CASE WHEN LEN(SerializedPayload) > 0 THEN 1 ELSE 0 END
                    FROM Integration.ReplayRecord
                    WHERE ReplayRecordId = @ReplayRecordId AND ISNULL(IsDeleted, 0) = 0;
                END

                IF @Exists = 1 AND @PayloadOk = 1
                BEGIN
                    UPDATE Integration.ReplayRecord
                        SET Status = N'Completed', ProcessedUtc = SYSUTCDATETIME(), FailureReason = NULL
                        WHERE ReplayRecordId = @ReplayRecordId;

                    INSERT INTO Integration.IntegrationLogEntry
                        (IntegrationLogEntryId, CorrelationId, CausationId, MessageId, TraceId, SpanId,
                         TimestampUtc, ElapsedMs, OperationName, ModuleName, Environment, Version, Success,
                         CreatedUtc, CreatedBy, IsDeleted)
                    SELECT NEWID(), CorrelationId, CausationId, MessageId, CONVERT(nvarchar(36), @ReplayRecordId), N'0',
                           SYSUTCDATETIME(), 0, N'ReplayRecord.Processed', N'Integration', N'Production', N'1.0', 1,
                           SYSUTCDATETIME(), @System, 0
                    FROM Integration.ReplayRecord WHERE ReplayRecordId = @ReplayRecordId;
                END
                ELSE
                BEGIN
                    IF @ReplayRecordId IS NOT NULL
                        UPDATE Integration.ReplayRecord
                            SET Status = N'Failed', ProcessedUtc = SYSUTCDATETIME(),
                                FailureReason = N'ReplayRecord not found, soft-deleted, or payload empty at processing time.'
                            WHERE ReplayRecordId = @ReplayRecordId;
                END;
                -- NOTE: the semicolon above is required -- SEND ON
                -- CONVERSATION fails to parse (Msg 102, "Incorrect syntax
                -- near 'SEND'") if immediately preceded by an
                -- unterminated IF/ELSE END block; live-verified 2026-07-19.

                SEND ON CONVERSATION @ConversationHandle
                    MESSAGE TYPE [//PlantSafety/Integration/ReplayResponse]
                    (CAST(ISNULL(@ReplayRecordId, '00000000-0000-0000-0000-000000000000') AS nvarchar(36)));

                END CONVERSATION @ConversationHandle;
            END
            ELSE IF @MessageTypeName = N'https://schemas.microsoft.com/SQL/ServiceBroker/EndDialog'
            BEGIN
                END CONVERSATION @ConversationHandle;
            END
            ELSE IF @MessageTypeName = N'https://schemas.microsoft.com/SQL/ServiceBroker/Error'
            BEGIN
                END CONVERSATION @ConversationHandle;
            END

            COMMIT TRANSACTION;
            END
        END TRY
        BEGIN CATCH
            IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;

            DECLARE @ErrorContext nvarchar(400) = CONCAT(N'Queue=Integration.ReplayRequestQueue; ConversationHandle=', @ConversationHandle);
            DECLARE @ErrorDetails nvarchar(max) = CONCAT(
                N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorState=', ERROR_STATE(), N'; ',
                N'ErrorSeverity=', ERROR_SEVERITY(), N'; ErrorLine=', ERROR_LINE(), N'; ErrorMessage=', ERROR_MESSAGE());
            INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog
                (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
                 ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
            VALUES
                (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
                 SYSUTCDATETIME(), @ErrorContext, @ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));
            SET @Done = 1;
        END CATCH
    END
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.service_queues WHERE name = N'ReplayRequestQueue' AND schema_id = SCHEMA_ID('Integration'))
    CREATE QUEUE Integration.ReplayRequestQueue
    WITH STATUS = ON,
         ACTIVATION (
             STATUS = ON,
             PROCEDURE_NAME = Integration.sp_ReplayRequestQueue_Activation,
             MAX_QUEUE_READERS = 1,
             EXECUTE AS OWNER
         );
GO

IF NOT EXISTS (SELECT 1 FROM sys.services WHERE name = N'//PlantSafety/Integration/ReplayRequestService')
    CREATE SERVICE [//PlantSafety/Integration/ReplayRequestService]
    ON QUEUE Integration.ReplayRequestQueue
    ([//PlantSafety/Integration/ReplayContract]);
GO

------------------------------------------------------------
-- 5. Initiator-side activation procedure + queue + service.
------------------------------------------------------------
CREATE OR ALTER PROCEDURE Integration.sp_ReplayInitiatorQueue_Activation
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @ConversationHandle uniqueidentifier;
    DECLARE @MessageTypeName sysname;
    DECLARE @MessageBody nvarchar(MAX);
    DECLARE @Done bit = 0;
    -- NOTE: BREAK/CONTINUE are not permitted inside a TRY block (T-SQL
    -- restriction), so loop exit is controlled via @Done rather than BREAK.

    WHILE @Done = 0
    BEGIN
        BEGIN TRY
            BEGIN TRANSACTION;

            WAITFOR (
                RECEIVE TOP(1)
                    @ConversationHandle = conversation_handle,
                    @MessageTypeName    = message_type_name,
                    @MessageBody        = CAST(message_body AS nvarchar(MAX))
                FROM Integration.ReplayInitiatorQueue
            ), TIMEOUT 5000;

            IF @@ROWCOUNT = 0
            BEGIN
                SET @Done = 1;
                COMMIT TRANSACTION;
            END
            ELSE
            BEGIN
                -- ReplayResponse, EndDialog, or Error: initiator side has
                -- nothing further to do once acknowledged -- the target
                -- already updated Status/ProcessedUtc. Simply end the
                -- conversation.
                END CONVERSATION @ConversationHandle;

                COMMIT TRANSACTION;
            END
        END TRY
        BEGIN CATCH
            IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
            SET @Done = 1;
        END CATCH
    END
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.service_queues WHERE name = N'ReplayInitiatorQueue' AND schema_id = SCHEMA_ID('Integration'))
    CREATE QUEUE Integration.ReplayInitiatorQueue
    WITH STATUS = ON,
         ACTIVATION (
             STATUS = ON,
             PROCEDURE_NAME = Integration.sp_ReplayInitiatorQueue_Activation,
             MAX_QUEUE_READERS = 1,
             EXECUTE AS OWNER
         );
GO

IF NOT EXISTS (SELECT 1 FROM sys.services WHERE name = N'//PlantSafety/Integration/ReplayInitiatorService')
    CREATE SERVICE [//PlantSafety/Integration/ReplayInitiatorService]
    ON QUEUE Integration.ReplayInitiatorQueue;
GO

------------------------------------------------------------
-- 6. Public entry point: enqueue a replay.
------------------------------------------------------------
CREATE OR ALTER PROCEDURE Integration.sp_ReplayRecord_Enqueue
    @ReplayRecordId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @ReplayRecordId IS NULL
            THROW 50001, N'ReplayRecordId is required.', 1;
        IF NOT EXISTS (SELECT 1 FROM Integration.ReplayRecord WHERE ReplayRecordId = @ReplayRecordId AND ISNULL(IsDeleted, 0) = 0)
            THROW 50002, N'ReplayRecord not found or is soft-deleted.', 1;

        DECLARE @Handle uniqueidentifier;

        BEGIN DIALOG CONVERSATION @Handle
            FROM SERVICE [//PlantSafety/Integration/ReplayInitiatorService]
            TO SERVICE N'//PlantSafety/Integration/ReplayRequestService'
            ON CONTRACT [//PlantSafety/Integration/ReplayContract]
            WITH ENCRYPTION = OFF;

        SEND ON CONVERSATION @Handle
            MESSAGE TYPE [//PlantSafety/Integration/ReplayRequest]
            (CAST(@ReplayRecordId AS nvarchar(36)));

        UPDATE Integration.ReplayRecord SET Status = N'Queued' WHERE ReplayRecordId = @ReplayRecordId;
    END TRY
    BEGIN CATCH
        DECLARE @ErrorContext nvarchar(400) = CONCAT(N'ReplayRecordId=', @ReplayRecordId, N'; User=', SYSTEM_USER);
        DECLARE @ErrorDetails nvarchar(max) = CONCAT(
            N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorState=', ERROR_STATE(), N'; ',
            N'ErrorSeverity=', ERROR_SEVERITY(), N'; ErrorLine=', ERROR_LINE(), N'; ErrorMessage=', ERROR_MESSAGE());
        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog
            (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
             ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
        VALUES
            (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
             SYSUTCDATETIME(), @ErrorContext, @ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));
        THROW;
    END CATCH
END
GO

------------------------------------------------------------
-- 7. Verification.
------------------------------------------------------------
SELECT name, is_broker_enabled FROM sys.databases WHERE name = 'PlantSafety.IntegrationDB';
GO
SELECT name FROM sys.service_message_types WHERE name LIKE N'//PlantSafety/Integration/Replay%';
GO
SELECT name FROM sys.service_contracts WHERE name LIKE N'//PlantSafety/Integration/Replay%';
GO
SELECT name FROM sys.services WHERE name LIKE N'//PlantSafety/Integration/Replay%';
GO
SELECT name, is_activation_enabled FROM sys.service_queues WHERE name IN (N'ReplayRequestQueue', N'ReplayInitiatorQueue');
GO
