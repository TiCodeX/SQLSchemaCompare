#!/bin/sh.exe
changedFiles=$(git diff --cached --name-only)

for file in $changedFiles; do
    unix2dos < "$file" | cmp -s - "$file"
    if [ ! $? -eq 0 ]; then
        error=1
        echo "WRONG LINE ENGING => $file"
    fi
done

if [ "${error-}" ]; then
    exit 1
fi
