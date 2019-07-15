# ConsumerVPN-v2 Example



## Introduction

ConsumerVPN is a VPN client application that implements the WLVPN SDK as a usable application for customers. This is based upon WPF and uses standard MVVM practices.

For further details on how to work with modifying the ConsumerVPN code to be branded and work with your requirements, please view the reseller documentation for the "Branding - How To Make ConsumerVPN Yours" article under Guides & Articles.


## Requirements



* [Visual Studio 2019](https://visualstudio.microsoft.com/downloads/)
* [WiX Toolset 3.11.1](http://wixtoolset.org/releases/v3.11.1/stable)
* [WiX Visual Studio Addon](https://marketplace.visualstudio.com/items?itemName=RobMensching.WiXToolset) (if you're a developer working in Visual Studio, build servers do not require this.)
* .NET Framework 4.6.2
* Microsoft Authenticode Certificate (for code signing, EV certs are suggested if 'Unauthorized Publisher warnings on Windows 10 are an issue, the application will work signed or unsigned regardless.)

(Note: If you are intending to run this on a build server, the [MS Build Tools](https://visualstudio.microsoft.com/thank-you-downloading-visual-studio/?sku=BuildTools&rel=16#) can act as a lightweight replacement for a full Visual Studio 2019 install.)


## Building

### Prerequisites

Before building the application, you will need to place a .pfx copy of your codesigning certificate in the following location: `<Repository Root>/Resources/Signing/cert.pfx`. You will also require a MyGet API key that is provided by the `wlvpn`  feed details page.

If you wish to use `build.cmd` follow the below. This is useful if you wish to have your credentials baked in to the repository itself.

Edit the file `<Repository Root>/build.cmd` to set the codesign certificate key (if required), the MyGet API key retrieved from . This is done on the following line:

```bash
.\tools\Cake\Cake.exe build.cake -Configuration="Release" -CertPassword="CODE SIGNING CERTIFICATE PASSWORD HERE" -MyGetApiKey="MYGET API KEY HERE" -ApplicationName="MyApplication.exe" -UsesEVCert=false -verbosity=diagnostic
```
CAVEAT
If you are using an EV based certificate, ensure the physical EV key (this may be a smart card or USB key) provided by your certificate authority is plugged in. There may be software that is required from your provider to be installed so please read through the instructions provided by your certificate authority on how to use the key. During building of the application, your EV certificate may require you to use a PIN number that shows on the physical device or is printed on the device itself. For more information on this, please check with your certificate provider. Also ensure the `UsesEVCert` option in `build.cmd` is set to `true`, that will execute EV Code signing process.


### Building the application

#### Updating the version number

In the file `src/App/Properties/AssemblyInfo.cs` look for the lines containing:

```c#
[assembly: AssemblyVersion("2.1.0.0")]
[assembly: AssemblyFileVersion("2.1.0.0")]
```

Update the version numbers in the parameter to your new version number before building. It is suggested you look at the other options in this file and ensure your application name is what you want it to be and that the company name is set correctly.

#### Updating the installer settings

In the root folder, there is a file called `Installer_Settings.wxi`. Open this in your text editor of choice and update all the values in there to fit your application and company name. For the `UpgradeCode` and `BundleUpgradeCode` key, you will need to generate two GUIDs to place in here. It is suggested when you set it once, you never change it again unless you're going through a major upgrade (e.g. 1.0 to 2.0). Generate a GUID in Powershell by running `[guid]::NewGuid().ToString().ToUpper()` and placing the result of that in between the quotation marks that have the temporary test `PUT_UPGRADECODE_HERE`/`PUT_BUNDLE_UPGRADECODE_HERE`.

#### Building the installer

If you have chosen not to use the build.cmd method of building the application, the following explains the Powershell commands required to go through with it.

In a Command prompt, enter the repository directory and run the following command.
`.\tools\Cake\Cake.exe build.cake -Configuration Release -CertPassword="CertPasswordHere" -MyGetApiKey="MyGetApiKeyHere" -ApplicationName="ConsumerVPN" UsesEVCert="False"`

* Configuration refers to whether you want to build a Debug or Release version.

* CertPassword refers to the password that the codesigning certificate uses, if you do not have a password protected certificate, you can keep this blank. 

* MyGetApiKey refers to the API key used to access the MyGet repository for WLVPN. Sign in to www.myget.org with your credentials and go to the  [Feed Details section](https://www.myget.org/feed/Details/wlvpn) for the `wlvpn` feed. Here you'll find your personal API key to use by scrolling down, copy that and paste it within the brackets for `MyGetApiKey`.

* UsesEVCert refers to whether you are codesigning using an EV cert, set this to either `True` or `False`.

At the end of this process, you'll have a Setup_2.1.0.0.exe file in the root of the repository.


## Distributing the application

Distribution of your application and handling updates is up to you as the developer, internally we use a framework called NetSparkle but you are to free to use what suits your environment best. Since these applications require administrator, we do not suggest methods such as ClickOnce. If you wish to use NetSparkle, there are tools in the PreRequisites folder to both generate a set of public/private keys to get started.