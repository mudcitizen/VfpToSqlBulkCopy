DEFINE CLASS baseTest as FxuTestCase OF FxuTestCase.prg

  #IF .f.
    LOCAL THIS AS baseTest OF baseTest.PRG
  #ENDIF

  PROCEDURE Setup
  ENDPROC

  PROCEDURE TearDown
  ENDPROC

ENDDEFINE

