# How to build the Installer.

## Setup your environment. Required only once.
* Install MS Build Tools from here "Prerequisites\vs_buildtools.exe" or download the latest version from Microsoft website (https://www.visualstudio.com/downloads/)
* Add `msbuild.exe` to Windows PATH environment variable.
* Install WiX Toolset from here "Prerequisites\wix311.exe".
* Generate UpgradeCode for your installer. 
  - Press Windows button, it will open the Windows Start Menu
  - Type: powershell, you'll see the Windows PowerShell menu option, click on it.
  - In the PowerShell command line window type: `[guid]::NewGuid()` that will give you GUID which looks something like this `b661c3d0-864f-48cd-90ff-2a7b9637db8d`
  - Copy GUID, open Installer settings file which is located here "src\Installer\Settings.wxi",
    Find `<?define UpgradeCode = "PUT_UPGRADECODE_HERE" ?>` line, replace `PUT_UPGRADECODE_HERE` with GUID code you generated. Save it.

## Bulding the installer
* Update assets in "Resources/Assets" folder.
* Update Installer settings. The settings file is located here "src\Installer\Settings.wxi".
* Put your certificate to "Resources/Signing" folder, make sure the certificate name is `cert.pfx`.
* Edit build.bat file.
  - If your certificate has a password, edit build.bat file. Find the line
    `::SET CERTIFICATE_PASSWORD=myPsswrd` 
    remove `::` and replace `myPsswrd` with your certificate password.
* Run build.bat. The result would be the Setup.exe file in the root directory.