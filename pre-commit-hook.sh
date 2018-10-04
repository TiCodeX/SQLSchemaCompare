#!/bin/sh.exe
changedFiles=$(git diff --cached --name-only)

for file in $changedFiles; do
    [ ! -f "$file" ] && continue;
    [ -z "${file##"SQLCompare.UI/wwwroot/lib/"*}" ] && continue;
    [ -z "${file##"SQLCompare.UI/wwwroot/img/"*}" ] && continue;
    [ -z "${file##"SQLCompare.UI/wwwroot/font/"*}" ] && continue;
    [ -z "${file##"SQLCompare/package"*".json"}" ] && continue;
    [ -z "${file##"SQLCompare/img/"*}" ] && continue;
    [ -z "${file##"SQLCompare/font/"*}" ] && continue;
    [ -z "${file##"SQLCompare/build/"*}" ] && continue;
    [ -z "${file##*".xlsx"}" ] && continue;
    [ -z "${file##*".pfx"}" ] && continue;
    [ -z "${file##*".p12"}" ] && continue;
    
    unix2dos < "$file" | cmp -s - "$file"
    if [ ! $? -eq 0 ]; then
        error=1
        echo "WRONG LINE ENGING => $file"
    fi
done

if [ "${error-}" ]; then
    exit 1
fi
