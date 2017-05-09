@echo [7mLaunching "gppg"[0m
@gppg /gplex /listing /conflicts Parser.y
@echo [7mLaunching "gplex"[0m
@gplex  Scanner.lex 

@echo [7mIF ALL OK then continue to delete temp files.[0m
@pause
del Parser.conflicts
del Parser.lst
del Scanner.lst