@echo off

rem Win32 batch file
rem
rem Shell script to run PDF Clown for .NET samples on MS Windows.

rem Deploy dependencies on local (private) path!
xcopy ..\pdfclown.lib\build\package\*.dll .\build\package /D /Y
xcopy ..\pdfclown.lib\lib\*.dll .\build\package /D /Y
copy PDFClownCLISamples.exe.config .\build\package

rem Execute the samples!
.\build\package\PDFClownCLISamples.exe
