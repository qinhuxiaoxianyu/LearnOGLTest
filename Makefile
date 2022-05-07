CC=g++
INCLUDE=-Iinclude -Iincl
SOURCE=source/*
SOURCE_FILE=$^

CFLAG_M=
LIBRARY=-lGL -lglfw -ldl -lX11 -lpthread -lassimp

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

#深度测试可以理解为每个像素都有一个深度值，让深度值和模板缓冲的值比较，决定是否剔除，通过后把像素的深度值写入深度缓冲
#模板测试和深度测试有点不一样，是通过glStencilFunc函数中的ref值与模板缓冲的stencil值进行比较，并且在某种比较通过后把ref值写入缓冲中
#cull face在blending_sorted.cpp中