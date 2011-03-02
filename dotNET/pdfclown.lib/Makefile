# Common
BaseDir = .
# Assembly
Assembly = PDFClown
AssemblyType = library
SourceDir = ./src
AssemblySourceFilter = $(SourceDir)/*.cs
AssemblyResourceDir = ./res/pkg
AssemblyCommonResourceDir = ../../main/res/pkg
BuildDir = ./build
TargetDir = $(BuildDir)/package
AssemblyTargetFile = $(Assembly).dll
AssemblyTarget = $(TargetDir)/$(AssemblyTargetFile)
AssemblyReferences = System.dll,System.Drawing.dll,System.Windows.Forms.dll,lib/ICSharpCode.SharpZipLib.dll
Compiler = gmcs
# Documentation
DocDir = $(BuildDir)/docs/api
# Log
LogDir = ./log

all: $(AssemblyTarget)

$(AssemblyTarget): FORCE
	-mkdir -p $(TargetDir)
	$(Compiler) -debug -out:$@ -t:$(AssemblyType) -r:$(AssemblyReferences) -resource:$(AssemblyCommonResourceDir)/fonts/AGL20.scsv,fonts.AGL20 -resource:$(AssemblyCommonResourceDir)/fonts/afm/Courier.afm,fonts.afm.Courier -resource:$(AssemblyCommonResourceDir)/fonts/afm/Courier-Bold.afm,fonts.afm.Courier-Bold -resource:$(AssemblyCommonResourceDir)/fonts/afm/Courier-BoldOblique.afm,fonts.afm.Courier-BoldOblique -resource:$(AssemblyCommonResourceDir)/fonts/afm/Courier-Oblique.afm,fonts.afm.Courier-Oblique -resource:$(AssemblyCommonResourceDir)/fonts/afm/Helvetica.afm,fonts.afm.Helvetica -resource:$(AssemblyCommonResourceDir)/fonts/afm/Helvetica-Bold.afm,fonts.afm.Helvetica-Bold -resource:$(AssemblyCommonResourceDir)/fonts/afm/Helvetica-BoldOblique.afm,fonts.afm.Helvetica-BoldOblique -resource:$(AssemblyCommonResourceDir)/fonts/afm/Helvetica-Oblique.afm,fonts.afm.Helvetica-Oblique -resource:$(AssemblyCommonResourceDir)/fonts/afm/Symbol.afm,fonts.afm.Symbol -resource:$(AssemblyCommonResourceDir)/fonts/afm/Times-Bold.afm,fonts.afm.Times-Bold -resource:$(AssemblyCommonResourceDir)/fonts/afm/Times-BoldItalic.afm,fonts.afm.Times-BoldItalic -resource:$(AssemblyCommonResourceDir)/fonts/afm/Times-Italic.afm,fonts.afm.Times-Italic -resource:$(AssemblyCommonResourceDir)/fonts/afm/Times-Roman.afm,fonts.afm.Times-Roman -resource:$(AssemblyCommonResourceDir)/fonts/afm/ZapfDingbats.afm,fonts.afm.ZapfDingbats -resource:$(AssemblyCommonResourceDir)/fonts/cmap/Identity-H,fonts.cmap.Identity-H -resource:$(AssemblyCommonResourceDir)/fonts/cmap/Identity-V,fonts.cmap.Identity-V AssemblyInfo.cs -recurse:$(AssemblySourceFilter) 2> $(LogDir)/$(AssemblyTargetFile).log

doc: FORCE
	-mkdir -p $(DocDir)
	rm -R -f $(DocDir)
	doxygen Doxyfile 2> $(LogDir)/Doxygen.log
	../../main/res/script/doxygen.postprocess.sh $(DocDir)

clean:
	-rm -R $(LogDir)
	-rm -R $(BuildDir)

FORCE:
	-mkdir -p $(LogDir)
	-mkdir -p $(BuildDir)
