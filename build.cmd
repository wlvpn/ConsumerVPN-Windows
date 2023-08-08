@ECHO OFF
:CakeInstallCheck
IF EXIST ./tools/ GOTO Execute
Powershell -ExecutionPolicy Unrestricted -file build.ps1 --dryrun
GOTO :Execute

:Execute
.\tools\Cake\Cake.exe build.cake --Configuration="Release" --CertPassword="" --MyGetApiKey="" --verbosity=diagnostic --ApplicationName="ConsumerVPN" --EVCert=false
