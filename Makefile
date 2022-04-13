CC=g++
INCLUDE=-Iinclude -Iincl
SOURCE=source/*
SOURCE_FILE=$^

CFLAG_M=
LIBRARY=-lGL -lglfw -ldl -lX11 -lpthread

CFLAG_OUT=-o
OUT_FILE=out/$@

PRE=$(CC) $(INCLUDE) $(SOURCE) $(SOURCE_FILE)
MID=$(LIBRARY) $(CFLAG_M)
POST=$(CFLAG_OUT) $(OUT_FILE)

%:src/%.cpp
	$(PRE) $(MID) $(POST)



# RM 代替 rm -f
RM_FLAG=-r
RM_FILE=out/*

.PHONY:clean
clean:
	$(RM) $(RM_FLAG) $(RM_FILE)