#!/bin/sh

cat $1 | sort | uniq | grep -v UnknownRTRig > $1.clean
mv $1.clean $1
