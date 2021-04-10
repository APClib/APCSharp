@echo off
docfx --build
xcopy _site\* \\192.168.0.28\william\websites\apc.wiki /Y /E
pause