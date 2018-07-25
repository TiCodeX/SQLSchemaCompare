#!/bin/sh.exe
changedFiles=$(git diff --cached --name-only)

for file in $changedFiles; do
    [ ! -f "$file" ] && continue;
    [ -z "${file##"SQLCompare.UI/wwwroot/lib/"*}" ] && continue;
    [ -z "${file##*".xlsx"}" ] && continue;
    
    unix2dos < "$file" | cmp -s - "$file"
    if [ ! $? -eq 0 ]; then
        error=1
        echo "WRONG LINE ENGING => $file"
    fi
done

if [ "${error-}" ]; then
    exit 1
fi
