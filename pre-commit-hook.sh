#!/bin/sh.exe
changedFiles=$(git diff --cached --name-only)

for file in $changedFiles; do
    [ ! -f "$file" ] && continue;
    [ -z "${file##".github/"*}" ] && continue;
    [ -z "${file##"images/"*}" ] && continue;
    [ -z "${file##"SQLSchemaCompare.UI/wwwroot/lib/"*}" ] && continue;
    [ -z "${file##"SQLSchemaCompare.UI/wwwroot/img/"*}" ] && continue;
    [ -z "${file##"SQLSchemaCompare.UI/wwwroot/font/"*}" ] && continue;
    [ -z "${file##"SQLSchemaCompare/package"*".json"}" ] && continue;
    [ -z "${file##"SQLSchemaCompare/img/"*}" ] && continue;
    [ -z "${file##"SQLSchemaCompare/font/"*}" ] && continue;
    [ -z "${file##"SQLSchemaCompare/build/"*}" ] && continue;
    [ -z "${file##*".xlsx"}" ] && continue;
    [ -z "${file##*".pfx"}" ] && continue;
    [ -z "${file##*".p12"}" ] && continue;
    [ -z "${file##*".exe"}" ] && continue;
    
    unix2dos < "$file" | cmp -s - "$file"
    if [ ! $? -eq 0 ]; then
        error=1
        echo "WRONG LINE ENGING => $file"
    fi
done

if [ "${error-}" ]; then
    exit 1
fi
