
if "%1"=="" (
	set target=Package 
)
else (
	set target=%1
)

"C:\Program Files (x86)\MSBuild\12.0\Bin\msbuild.exe" build.proj -t:%target% -p:BUILD_NUMBER=8 -p:GIT_COMMIT=842ddaf