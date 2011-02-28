#!/bin/bash
#
# Mentor 0.1 compilation script.
# 2008-11-27. Edited by Stefano Chizzolini (http://www.stefanochizzolini.it)
#
# CLI parameters:
# $1 is the file directory.
# $2 is the file title.
# $3 is the distribution base directory.

xalan='java org.apache.xalan.xslt.Process'
fop='fop'
xslDir="$3/main/res/xsl"
fileTitle="$2"
filePath="$1/$fileTitle"

echo $'\n'"$filePath begins."

# Generate DocBook!
echo $'\n'"Compiling $filePath.docbook ..."
$xalan -IN $filePath.mentor -XSL $xslDir/mentor/mentor.docbook.xsl -OUT $filePath.docbook -PARAM dir $1 -PARAM fileTitle $fileTitle

# Generate plain text!
echo $'\n'"Compiling $filePath.txt ..."
$xalan -IN $filePath.docbook -XSL $xslDir/mentor/mentor.docbook.txt.xsl -OUT $filePath.txt

# Generate HTML!
echo $'\n'"Compiling $filePath.html ..."
$xalan -IN $filePath.docbook -XSL $xslDir/docbook/custom/docbook.html.xsl -OUT $filePath.html

# Generate PDF!
#echo $'\n'"Compiling $filePath.pdf ..."
#$fop -xml $filePath.docbook -xsl $xslDir/docbook/custom/mentor.docbook.fo.xsl -pdf $filePath.pdf

# Discard DocBook!
rm $filePath.docbook

echo $'\n'"$filePath ends." $'\n'
