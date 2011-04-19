# Assembly
Assembly = PDFClownCLISamples
AssemblyType = exe
SourceDir = ./src
AssemblySourceFilter = $(SourceDir)/*.cs
BuildDir = ./build
TargetDir = $(BuildDir)/package
AssemblyTargetFile = $(Assembly).exe
AssemblyTarget = $(TargetDir)/$(AssemblyTargetFile)
AssemblyReferences = ../pdfclown.lib/build/package/PDFClown.dll,System.Configuration.dll,System.Drawing.dll,System.Xml.dll
Compiler = gmcs
OutputDir = ./output
# Documentation
DocDir = $(BuildDir)/docs/api
# Log
LogDir = ./log

all: $(AssemblyTarget)

$(AssemblyTarget): FORCE
	-mkdir -p $(TargetDir)
	$(Compiler) -debug -out:$@ -t:$(AssemblyType) -r:$(AssemblyReferences) -recurse:$(AssemblySourceFilter) 2> $(LogDir)/$(AssemblyTargetFile).log

doc: FORCE
	-mkdir -p $(DocDir)
	rm -R -f $(DocDir)
	doxygen Doxyfile 2> $(LogDir)/Doxygen.log
	../../main/res/script/doxygen.postprocess.sh $(DocDir)

clean:
	-rm -R $(LogDir)
	-rm -R $(BuildDir)
	-rm -R $(OutputDir)

FORCE:
	-mkdir -p $(LogDir)
	-mkdir -p $(BuildDir)
	-mkdir -p $(OutputDir)
