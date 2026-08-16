Set WshShell = WScript.CreateObject("WScript.Shell")
strCurrentFolder = Left(Wscript.ScriptFullName, Len(Wscript.ScriptFullName) - Len(Wscript.ScriptName))
cmd = strCurrentFolder & "RunDprint.bat " & WScript.Arguments(0)
WshShell.Run cmd, 0, False

