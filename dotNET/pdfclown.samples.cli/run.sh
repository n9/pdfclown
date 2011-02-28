#!/bin/bash
#
# Shell script to run PDF Clown samples on Mono.

(
# Insert a look-up reference to the PDFClown library directory!
export MONO_PATH=MONO_PATH:`pwd`/../pdfclown.lib/build/package:`pwd`/../pdfclown.lib/lib
cp PDFClownCLISamples.exe.config build/package
# Execute the test!
mono --debug ./build/package/PDFClownCLISamples.exe #2> ../log/PDFClownCLISamples.exe.log
)
