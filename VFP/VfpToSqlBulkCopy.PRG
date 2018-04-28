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

      LOCAL lcConnStr,lnConnHandle
      lcConnStr = [DRIVER={SQL Server Native Client 11.0};Trusted_Connection=yes;Server={1};DataBase={2}]
      lcConnStr = STRTRAN(lcConnStr,'{1}',tcSqlServerName)
      lcConnStr = STRTRAN(lcConnStr,'{2}',tcSqlDbName)
      lnConnHandle = SQLSTRINGCONNECT(lcConnStr)

      IF lnConnHandle > 0

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
              IF SQLEXEC(lnConnHandle,lcCmd) < 0
                lcReturn = This.GetErrorText()
                EXIT
              ENDIF
            ENDIF
          NEXT

          IF NOT EMPTY(lcReturn)
            EXIT
          ENDIF

        ENDSCAN

        USE IN (lcAlias)
        SQLDISCONNECT(lnConnHandle)

        SELECT (lnSelect)

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

  PROTECTED PROCEDURE GetErrorText() as String
    LOCAL laError(1),lc,lcError
    AERROR(laError)
    lcError = ''
    FOR lni = 1 TO ALEN(laError,1)
      lcError = lcError + STR(laError(lni,1)) + ' - ' + laError(lni,2) + CHR(13)+CHR(10)
    NEXT
    RETURN lcError
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

