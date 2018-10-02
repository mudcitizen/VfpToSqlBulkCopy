DEFINE CLASS testSqlClient as baseTest OF baseTest.prg

  #IF .f.
    LOCAL THIS AS testSqlClient OF testSqlClient.PRG
  #ENDIF

  PROCEDURE TestStuff
    WITH This

      LOCAL loClient,lcResult
      loClient = NEWOBJECT('SqlClient','vfptosqlbulkcopy.prg')
      loClient.Connect(GETENV('COMPUTERNAME'),'Laptop_22_100816')
      lcResult = loClient.Exec([SELECT * FROM IN_RES WHERE LEVEL = 'INH'],'INHRES')

      .AssertTrue('Expected empty return code',EMPTY(lcResult))
      .AssertTrue('Expected INHRES to be in use',USED('INHRES'))
      IF USED('INHRES')
        .AssertTrue('Expected non-zero reccount in INHRES',RECC('INHRES')>0)
        USE IN INHRES
      ENDIF

      lcResult = loClient.Exec([SELECT * FROM IN_ADFAFADFA WHERE LEVEL = 'INH'],'INHRES')
      .AssertTrue('Expected non-empty return code',NOT EMPTY(lcResult))

    ENDWITH

  ENDPROC

ENDDEFINE

