@echo off
::Settings
::SET CERTIFICATE_PASSWORD=

::Building the installer
MSBuild src\Bootstrapper\Bootstrapper.wixproj /p:Configuration=Release /p:SolutionDir=%cd%\

::Copying the installer file to the root dir
copy src\Bootstrapper\bin\Release\Bootstrapper.exe Setup.exe

::Undefine cert password
SET CERTIFICATE_PASSWORD=