# ConsumerVPN example

This is an advanced WLVPN VPN SDK usage example.
Provided is a copy of the ConsumerVPN client. For applications that just need a bit UI customization, this is the base WLVPN client. It is designed to be easily rebranded, so that you can have an application that is production-ready with a minimal amount of work.

# What you need
You will need to become a registered WLVPN reseller to run the application. If you have not done so already, please visit https://wlvpn.com/#contact to get started.

To create your own application from this template, you will need a copy of Visual Studio. This can be obtained from here: https://www.visualstudio.com/vs/

Once Visual Studio 2017 has been installed, you will need to clone or download the repository for the ConsumerVPN application.

Once downloaded, open the VpnSDK.WLVpn.sln file in the extracted folder.

The application is a Microsoft .NET C# WPF application. It follows the normal MVVM patterns associated with a WPF application. This readme does not detail the code in the application but how to change the look of the application generated from the code. If you install Visual Studio 2017, which is what we are using currently, you will get .NET 4.6.1 which is the .NET prerequisite for the application.

We use the WiX toolset for building installers. You will also need to download and install the WiX Toolset build tools and WiX Toolset Visual Studio Extensions that are compatible with your environment from this link: http://wixtoolset.org/releases/

The Wix installation will require you to enable .NET 3.5 on your machine. If WiX does not automatically do this for you, perform the following instructions:

1. Press "Windows Logo" + "R" keys on the keyboard.
2. Type "appwiz.cpl" in the "Run" command box and press "ENTER" on the keyboard or click Run.
3. In the "*Programs and Features" window, click on the link "*Turn Windows features on or off**".
4. Check if the "*.NET Framework 3.5 (includes .NET 2.0 and 3.0)*" option is available in it.
5. If yes, then enable it and then click on "*OK*".
6. Follow the on-screen instructions to complete the installation and restart the computer, if prompted.

If you run in to any errors, the following link will provide information from Microsoft on how to solve any potential issues.

https://answers.microsoft.com/en-us/insider/forum/insider_wintp-insider_install/how-to-instal-net-framework-35-on-windows-10/450b3ba6-4d19-45ae-840e-78519f36d7a4?auth=1


## VpnSDK
To be able to build the solution you need to have the access to WLVPN MyGet feed. Please contact us about that https://wlvpn.com/#contact
API reference is available by request.