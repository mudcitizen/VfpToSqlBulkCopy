DEFINE CLASS testComClass as baseTest OF baseTest.prg

  #IF .f.
    LOCAL THIS AS testComClass OF testComClass.PRG
  #ENDIF

  PROTECTED vfptosqlbulkcopy

  PROCEDURE Setup
    DODEFAULT()
    CLOSE DATA
    This.vfptosqlbulkcopy = NEWOBJECT('vfptosqlbulkcopy','vfptosqlbulkcopy.prg')
  ENDPROC

  PROCEDURE TestMemoUpload
    WITH This

      LOCAL lcDb,lcServer,lcTable
      lcTAble = 'IN_WATRM'
      lcServer = GETENV('COMPUTERNAME')
      lcDb = 'Essex_22_000211'

      LOCAL lcDbf
      lcDbf = FORCEEXT('D:\ESSEX\HOSTDEMA\' + lcTable,'DBF')
      .AssertTrue(lcDbf + ' does not exist',FILE(lcDbf))
      
      SELECT 0
      USE (lcDbf) 
      .AssertTrue('Expected 1 row in ' + lcDbf,RECCOUNT()=1)
            
      SELECT * from (lcDbf) WHERE ATC(CHR(0),background) > 0 INTO CURSOR FUNG
      .AssertTrue('Expected 1 in ' + lcDBF + ' with CHR(0) in background',RECCOUNT('fung')=1)
      USE IN fung      
      USE IN (lcTable) 
            
      
      LOCAL loClient,lcResult
      loClient = NEWOBJECT('SqlClient','vfptosqlbulkcopy.prg')
      loClient.Connect(lcServer,lcDb)
      lcResult = loClient.Exec([DELETE FROM ] + lcTable)
      * There needs to be a row in the SQL table to update...
      lcResult = loClient.Exec([INSERT INTO ] + lcTable + [(PK) VALUES ('123456')])

      LOCAL lcCmd
      lcCmd = 'select * from ' + lcTable 
      lcResult = loClient.Exec(lcCmd,'FUNG')

      .AssertTrue('Expected 1 or more rows',RECCOUNT('fung')=1)

      lcResult = .vfptosqlbulkcopy.UploadMemos(lcDbf, lcTable, lcServer,lcDb)

      lcCmd = 'select * from ' + lcTable + ' where charindex(char(0),background) > 0'
      lcResult = loClient.Exec(lcCmd,'FUNG')

      .AssertTrue('Expected 1 or more rows',RECCOUNT('fung')=1)
      USE IN FUNG

    ENDWITH

  ENDPROC

ENDDEFINE

