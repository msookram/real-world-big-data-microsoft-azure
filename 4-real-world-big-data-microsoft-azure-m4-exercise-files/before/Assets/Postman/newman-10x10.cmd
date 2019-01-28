@echo off

del output*.json

for /l %%i in (1,1,10) do call :loop %%i
goto :eof

:loop

    start /min newman -c Telemetry.json.postman_collection -e localhost-Telemetry.Api.postman_environment -n 10 -o output-%1.json
    goto :eof

