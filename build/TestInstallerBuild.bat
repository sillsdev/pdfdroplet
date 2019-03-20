setlocal
@set PATH=C:\Program Files (x86)\Microsoft Visual Studio\Installer;%PATH%
for /f "usebackq delims=" %%i in (`vswhere -latest -requires Microsoft.Component.MSBuild -property installationPath`) do (
	set InstallDir=%%i
)
call "%InstallDir%\VC\Auxiliary\Build\vcvarsall.bat" x86

pushd .
MSbuild /target:installer /property:teamcity_build_checkoutDir=..\  /property:teamcity_dotnet_nunitlauncher_msbuild_task="notthere" /property:BUILD_NUMBER="*.*.6.789" /property:Minor="1"
popd
PAUSE

#/verbosity:detailed