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
  = halthere(PROGRAM())
  IF EMPTY(lcReturn)
    SELECT * FROM (ADDBS(lcFolderOut)+lcTableOut)
  ENDIF
ENDWITH


DEFINE CLASS VfpToSqlBulkCopy as Session OLEPUBLIC

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
      LOCAL laStru(1),lni,lcCmd,loMemoFields
      loMemoFields = CREATEOBJECT('Collection')
      = AFIELDS(laStru)
      FOR lni = 1 TO ALEN(laStru,1)
        IF laStru(lni,2) = 'M'
          loMemoFields.Add(laStru(lni,1))
        ENDIF
      NEXT

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

