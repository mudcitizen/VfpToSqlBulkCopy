TestUploadMemos()

DEFINE CLASS VfpToSqlBulkCopy as Session OLEPUBLIC

  PROTECTED SqlConnectStringTemplate
  SqlConnectStringTemplate = [DRIVER={SQL Server Native Client 11.0};Trusted_Connection=yes;Server={1};DataBase={2}]

  PROCEDURE Init()

    SET TALK OFF
    SET CENTURY OFF
    SET CENTURY TO 19 ROLLOVER 71
    SET EXCLUSIVE OFF
    SET EXACT OFF
    SET NEAR OFF
    SET SAFETY OFF


    SET BELL OFF
    SET CARRY OFF
    SET COLLATE TO 'Machine'
    SET COMPATIBLE OFF
    SET CONFIRM OFF
    SET CPDIALOG OFF
    SET DELETED OFF

    IF Application.StartMode = 0
      SET DEVELOPMENT ON
    ELSE
      SET DEVELOPMENT OFF
    ENDIF

    SET ECHO OFF
    SET MEMOWIDTH TO 50
    SET MACKEY TO
    SET MULTILOCKS OFF
    SET NULLDISPLAY TO ''
    SET STRICTDATE TO 0
    SET STATUS BAR OFF
    SET TABLEVALIDATE TO 0
    SET UDFPARMS TO VALUE

    IF Application.StartMode > 0
      SET RESOURCE OFF
    ENDIF

    IF Application.StartMode > 0
      SET SYSMENU OFF
    ENDIF
  ENDPROC

  PROCEDURE ListMemos(tcTableNameIn as String, tcOutDirectoryName as String,tcTableNameOut as String) as String
    * tcTableNameIn - Fully qualified name of the VFP we need to read / check for ASCII 0 in memo
    * tcOutDirectoryName - The folder to which we copy the memo data and in which we create the table
    * tcTableNameOut - Stem only of the table we build listing problematic memos

    LOCAL lcReturn
    lcReturn = ''

    LOCAL loExcp as Exception

    TRY

      LOCAL loExcp as Exception
      loExcp = .Null.

      tcOutDirectoryName = ADDBS(tcOutDirectoryName)

      IF NOT DIRECTORY(tcOutDirectoryName)
        mkdir(tcOutDirectoryName)
      ENDIF

      LOCAL lnSele
      lnSele = SELECT()

      SELECT 0
      LOCAL lcAliasOut,lcAliasIn
      CREATE TABLE (tcOutDirectoryName+tcTableNameOut) (FieldName C(20), RecNo I, FileName M)
      lcAliasOut = ALIAS()

      SELECT 0
      USE (tcTableNameIn)
      lcAliasIn = ALIAS()
      LOCAL loMemoFields
      loMemoFields = This.GetMemoFields(DBF())

      IF loMemoFields.Count <> 0
        LOCAL lcFileName,lcData,lcMemoField,lcCmd,lnRecNo
        SCAN
          FOR EACH lcMemoField IN loMemoFields
            lcData = EVALUATE(lcMemoField)
            IF AT(CHR(0),lcData) > 0
              lnRecNo = RECNO()
              lcFileName = '{1}.{2}.Memo'
              lcFileName = STRTRAN(lcFileName,'{1}',lcMemoField)
              lcFileName = STRTRAN(lcFileName,'{2}',ALLTRIM(STR(lnRecNo)))
              lcFileName = ADDBS(tcOutDirectoryName) + lcFileName
              INSERT INTO (lcAliasOut) VALUES (lcMemoField,lnRecNo,lcFileName)
              lcCmd = 'COPY MEMO {1} TO {2}'
              lcCmd = STRTRAN(lcCmd,'{1}',lcMemoField)
              lcCmd = STRTRAN(lcCmd,'{2}',lcFileName)
              &lcCmd
            ENDIF
          NEXT
        ENDSCAN
      ENDIF

      USE IN (lcAliasIn)
      USE IN (lcAliasOut)

      SELECT (lnSele)

    CATCH TO loExcp
      LOCAL loErrHandler
      loErrHandler = CREATEOBJECT('ErrorHandler',loExcp)
      lcReturn = loErrHandler.ErrorDetails
    ENDTRY

    *!*	    IF NOT ISNULL(loExcp)
    *!*	      LOCAL loErrHandler
    *!*	      loErrHandler = CREATEOBJECT('ErrorHandler',loExcp)
    *!*	      lcReturn = loErrHandler.ErrorDetails
    *!*	      MODIFY COMMAND (loErrHandler.ErrorFileName) NOWAIT
    *!*	    ENDIF

    RETURN lcReturn

  ENDPROC


  PROCEDURE UploadMemos(tcSourceTableName as String, tcDestinationTableName as String, tcSqlServerName as String,tcSqlDbName as String) as String

    LOCAL loMemoFields
    loMemoFields = This.GetMemoFields(tcSourceTableName)
    IF loMemoFields.Count = 0
      RETURN ''
    ENDIF

    LOCAL loExcp as Exception

    TRY

      LOCAL lcReturn
      lcReturn  = ''
      LOCAL loSqlClient 
      loSqlClient = CREATEOBJECT('SqlClient') 
      lcReturn = loSqlClient.Connect(tcSqlServerName,tcSqlDbName)

      IF EMPTY(lcReturn) 

        LOCAL lnSelect
        lnSelect = SELECT()
        LOCAL lcAlias,lcMemoField
        lcAlias = STRTRAN(PROGRAM(),'.','_') + SYS(2015)
        SELECT 0
        USE (tcSourceTableName) Again ALIAS (lcAlias)
        LOCAL lcCmd,lcBytes,lnRecNo

        SCAN

          FOR EACH lcMemofield IN loMemoFields

            lcData = EVALUATE(lcMemoField)

            IF AT(CHR(0),lcData) > 0
              lcBytes = lcData
              lnRecNo = RECNO()
              lcCmd = 'UPDATE {1} SET {2} = ?lcBytes WHERE SqlRecNo = ?lnRecNo'
              lcCmd = STRTRAN(lcCmd,'{1}',tcDestinationTableName )
              lcCmd = STRTRAN(lcCmd,'{2}',lcMemoField)
              lcReturn = loSqlClient.Exec(lnConnHandle)
              IF NOT EMPTY(lcReturn) 
                EXIT
              ENDIF
            ENDIF
          NEXT

          IF NOT EMPTY(lcReturn)
            EXIT
          ENDIF

        ENDSCAN

        USE IN (lcAlias)
        SELECT (lnSelect)

      ELSE

        lcReturn = 'Unable to connect - ' + tcSqlServerName + ' ' + tcSqlDbName

      ENDIF


    CATCH TO loExcp
      LOCAL loErrHandler
      loErrHandler = CREATEOBJECT('ErrorHandler',loExcp)
      lcReturn = loErrHandler.ErrorDetails
    ENDTRY

    RETURN lcReturn

  ENDPROC

  PROTECTED PROCEDURE GetMemoFields(tcTableName as String) as Collection
    LOCAL lnSelect,lcAlias
    lnSelect = SELECT()
    lcAlias = SYS(2015)
    SELECT 0
    USE (tcTableName) AGAIN ALIAS (lcAlias)

    LOCAL laStru(1),lni,lcCmd,loMemoFields
    loMemoFields = CREATEOBJECT('Collection')
    = AFIELDS(laStru)
    FOR lni = 1 TO ALEN(laStru,1)
      IF laStru(lni,2) = 'M'
        loMemoFields.Add(laStru(lni,1))
      ENDIF
    NEXT

    USE IN (lcAlias)
    SELECT (lnSelect)
    RETURN loMemoFields

  ENDPROC

  PROCEDURE ProcessConversionActions(tcDestinationTableName as String, tcSqlServerName as String,tcSqlDbName as String) as String

    LOCAL loExcp as Exception

    LOCAL lcReturn
    lcReturn  = ''

    LOCAL loConversionActionProvider,lcConversionActionProvider
    lcConversionActionProvider = tcDestinationTableName + '_ConversionActionProvider'

    TRY
      loConversionActionProvider = CREATEOBJECT(lcConversionActionProvider)
    CATCH
      loConversionActionProvider = .Null.
    ENDTRY

    IF ISNULL(loConversionActionProvider)
      RETURN ''
    ENDIF

    TRY
      lnConnHandle = This.ConnectToSql(tcSqlServerName,tcSqlDbName)
      IF lnConnHandle > 0
        LOCAL loActions,loAction
        loActions = loConversionActionProvider.GetActions()
        FOR EACH loAction IN loActions
          SQLEXEC(lnConnHandle,loAction.CommandString)
        NEXT
        SQLDISCONNECT(lnConnHandle)
      ELSE
        lcReturn = 'Unable to connect - ' + lcConnStr
      ENDIF

    CATCH TO loExcp
      LOCAL loErrHandler
      loErrHandler = CREATEOBJECT('ErrorHandler',loExcp)
      lcReturn = loErrHandler.ErrorDetails
    ENDTRY

    RETURN lcReturn

  ENDPROC

ENDDEFINE


DEFINE CLASS ErrorHandler as Custom

  ErrorFileName = ''
  ErrorMessage = ''
  ErrorDetails = ''

  PROCEDURE Init(toExcp as Exception)
    LOCAL lcStack(1)
    WITH This
      .ErrorFileName = ADDBS(GETENV('TEMP')) + 'VfpToSqlBulkCopy' + '_' + TTOC(DATETIME(),1) + '_' + SYS(2015) + '.err'
      LIST STATUS TO FILE (.ErrorFileName) NOCONSOLE
      .ErrorMessage = 'Error # - {0} ; Message - {1} @ Line # - {2} ; Line # - {3} ; See {4} for details.'
      .ErrorMessage = STRTRAN(.ErrorMessage,'{0}',ALLTRIM(STR(toExcp.ErrorNo)))
      .ErrorMessage = STRTRAN(.ErrorMessage,'{1}',toExcp.Message)
      .ErrorMessage = STRTRAN(.ErrorMessage,'{2}',ALLTRIM(STR(toExcp.LineNo)))
      TRY
        .ErrorMessage = STRTRAN(.ErrorMessage,'{3}',ALLTRIM(STR(toExcp.LineContents)))
      CATCH
      ENDTRY
      .ErrorMessage = STRTRAN(.ErrorMessage,'{4}',.ErrorFileName)
      LOCAL lcExcp
      lcExcp =  .ErrorMessage
      = ASTACKINFO(laStack)
      LIST MEMORY TO (.ErrorFileName) ADDITIVE NOCONSOLE
      .ErrorDetails = FILETOSTR(.ErrorFileName)
    ENDWITH
  ENDPROC

ENDDEFINE

DEFINE CLASS SqlClient as Custom

  PROTECTED Handle
  Handle = .Null.

  PROCEDURE Connect(tcServerName as String,tcDbName as String) as String
    WITH This
      .Disconnect()
      LOCAL lcConnStr,lnConnHandle
      lcConnStr = [DRIVER={SQL Server Native Client 11.0};Trusted_Connection=yes;Server={1};DataBase={2}]
      lcConnStr = STRTRAN(lcConnStr,'{1}',tcServerName)
      lcConnStr = STRTRAN(lcConnStr,'{2}',tcDbName)
      lnConnHandle = SQLSTRINGCONNECT(lcConnStr)

      LOCAL lcReturn
      IF lnConnHandle > 0
        .Handle = lnConnHandle
        lcReturn = ''
      ELSE
        lcReturn = 'Unable to connect ' + lcConnStr + CHR(13)+CHR(10) + .GetErrorDetails()
      ENDIF
    ENDWITH

  ENDPROC

  PROCEDURE Exec(tcCommandString as String,tcCursorName AS StrinG) as String
    LOCAL lcResult,lnResult
    LOCAL lnSele
    lnSele = SELECT()
    LOCAL llCloseIt
    llCloseIt = EMPTY(tcCursorName)
    IF llCloseIt
      tcCursorName = SYS(2015)
    ENDIF
    lnResult = SQLEXEC(This.Handle,tcCommandString,tcCursorName)
    IF lnResult < 0
      lcResult = This.GetErrorDetails()
    ELSE
      lcResult = ''
    ENDIF
    IF llCloseIt
      USE IN (tcCursorName)
    ENDIF
    SELECT(lnSele)
    RETURN lcResult
  ENDPROC

  PROCEDURE Destroy()
    This.Disconnect()
    DODEFAULT()
  ENDPROC

  PROTECTED PROCEDURE Disconnect()
    IF NOT ISNULL(This.Handle)
      SQLDISCONNECT(This.Handle)
    ENDIF
  ENDPROC

  PROTECTED PROCEDURE GetErrorDetails() as String
    LOCAL laError(1),lcError,lni,lcVartype
    = AERROR(laError)
    LIST MEMORY LIKE la TO a.txt
    lcError = ''
    FOR lni = 1 TO ALEN(laError)
      lcError = lcError + '(' + ALLTRIM(STR(lni)) + ') - '
      lcVartype = VARTYPE(laError(lni))
      DO CASE
        CASE ISNULL(laError(lni))
          lcError = lcError + 'Null'
        CASE lcVarType = 'N'
          lcError = lcError + ALLTRIM(STR(laError(lni)))
        CASE lcVarType = 'C'
          lcError = lcError + laError(lni)
      ENDCASE
      lcError = lcError + '; '
    NEXT
    RETURN ALLTRIM(lcError)

  ENDPROC

ENDDEFINE 

  PROCEDURE TestUploadMemos
    CLOSE DATABASES
    LOCAL lcVfpDbf
    lcVfpDbf = '..\..\VHOST\IN_WATRM.DBF'
    ASSERT FILE(lcVfpDbf)

    LOCAL la(1)
    SELECT COUNT(*) FROM (lcVfpDbf) WHERE AT(CHR(0),Background) > 0 INTO ARRAY la
    ASSERT la(1) > 0 MESSAGE 'No rows with ASCII 0'
    CLOSE DATA


    LOCAL lcSqlServerName, lcSqlDbName
    lcSqlServerName = GETENV('COMPUTERNAME')
    lcSqlDbName = 'NoRows_22_000211'

    LOCAL lcConnStr
    lcConnStr = [DRIVER={SQL Server Native Client 11.0};UID=PSMSUsr;PWD=$1koj1@VGR2;Server={1};DataBase={2}]
    lcConnStr = STRTRAN(lcConnStr,'{1}',lcSqlServerName)
    lcConnStr = STRTRAN(lcConnStr,'{2}',lcSqlDbName)

    LOCAL lcTable,lcField
    lcTable = 'IN_WATRM'


    * Clear the contents in the SQL DB
    LOCAL lnH
    lnH = SQLSTRINGCONNECT(lcConnStr)
    LOCAL lcCmd
    lcCmd = [UPDATE IN_WATRM SET Background = '']
    ASSERT lnH > 0
    SQLEXEC(lnH,lcCmd)

    WITH CREATEOBJECT('VfpToSqlBulkCopy')
      LOCAL lcResult
      lcResult = .UploadMemos(lcVfpDbf,lcTable,lcSqlServerName,lcSqlDbName)
    ENDWITH

    ASSERT EMPTY(lcResult)

    SELECT RECNO() as RecNo,BackGround FROM (lcVfpDbf) INTO CURSOR VFP NOFILTER
    SQLEXEC(lnh,'SELECT SqlRecno as RecNo, Background FROM IN_WATRM','SQL')

    ASSERT RECCOUNT('VFP') = RECCOUNT('SQL')
    SELECT VFP
    SCAN
      SELECT SQL
      LOCATE FOR Recno = Vfp.Recno
      ASSERT FOUND() MESSAGE 'No matching record in SQL for VFP'
      ASSERT VFP.Background == Sql.Background MESSAGE 'Backgrounds differ'
    ENDSCAN

    CLOSE DATABASES
    SQLDISCONNECT(lnh)

  ENDPROC

  PROCEDURE TestListMemos
    LOCAL lcTableIn,lcFolderOut,lcTableOut
    lcTableIn = 'D:\VHOST\IN_WATRM.DBF'
    lcFolderOut = 'C:\TEMP\MEMOOUT\'
    lcTableOut = 'MemoProblems'




    * USE C:\TEMP\MEMOOUT\MemoProblems'
    LOCAL lcClass,lcReturn
    lcClass = 'VfpToSqlBulkCopy.VfpToSqlBulkCopy'
    lcClass = 'VfpToSqlBulkCopy'
    WITH CREATEOBJECT(lcClass)
      lcReturn = .ListMemos(lcTableIn,lcFolderOut,lcTableOut)
      IF EMPTY(lcReturn)
        SELECT * FROM (ADDBS(lcFolderOut)+lcTableOut)
      ENDIF
    ENDWITH

  ENDPROC

