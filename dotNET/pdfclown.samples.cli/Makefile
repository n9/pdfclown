# Assembly
Assembly = PDFClownCLISamples
AssemblyType = exe
SourceDir = ./src
AssemblySourceFilter = $(SourceDir)/*.cs
TargetDir = ./build/package
AssemblyTargetFile = $(Assembly).exe
AssemblyTarget = $(TargetDir)/$(AssemblyTargetFile)
AssemblyReferences = ../pdfclown.lib/build/package/PDFClown.dll,System.Configuration.dll,System.Drawing.dll
Compiler = gmcs
OutputDir = ./output
# Documentation
DocDir = ./build/docs/api
# Log
LogDir = ./log

all: $(AssemblyTarget)

$(AssemblyTarget): FORCE
	$(Compiler) -debug -out:$@ -t:$(AssemblyType) -r:$(AssemblyReferences) -recurse:$(AssemblySourceFilter) 2> $(LogDir)/$(AssemblyTargetFile).log

doc: FORCE
	rm -R -f $(DocDir)
	doxygen Doxyfile 2> $(LogDir)/Doxygen.log
	../../main/res/script/doxygen.postprocess.sh $(DocDir)

clean:
	rm -f $(OutputDir)/*
	rm -f $(LogDir)/*

FORCE:
