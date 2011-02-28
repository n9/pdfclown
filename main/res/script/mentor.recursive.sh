#!/bin/bash
#
# Mentor 0.1 Recursive compilation script.
# 2006-09-12. Edited by Stefano Chizzolini (http://www.stefanochizzolini.it)
#
# CLI parameters:
# $1 is the distribution base directory.

files=$(find $1 -name '*.mentor')
for file in $files
do
  $1/main/res/script/mentor.sh $(dirname $file) $(basename $file .mentor) $1
done
