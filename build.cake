#tool nuget:?package=NUnit.ConsoleRunner&version=3.4.0
#addin nuget:?package=Cake.VersionReader
using System.Xml.Linq;
using System.Xml;
using System.Security.Cryptography;
using System.IO;

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("Configuration", "Release");
var certificatePassword = Argument("CertPassword", "");
var mygetApiKey = Argument("MyGetApiKey", "");
var ApplicationName = Argument("ApplicationName","ConsumerVPN");
var UsesEVCert = Argument<bool>("EVCert", false);
var certSubject = Argument("CertSubject",""); //Add your own cert subject
var TimeServerUrl = "http://timestamp.sectigo.com";

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

// Define directories.
var buildDir = MakeAbsolute(Directory("./Output/") + Directory(configuration));
var solutionDir = MakeAbsolute(Directory("."));


// Codesign stuff.
var signToolSettings = UsesEVCert ? new SignToolSignSettings
{
    TimeStampUri = new Uri(TimeServerUrl),
    DigestAlgorithm = SignToolDigestAlgorithm.Sha256,
    TimeStampDigestAlgorithm = SignToolDigestAlgorithm.Sha256,
    CertSubjectName = certSubject	
} : new SignToolSignSettings 
{
    TimeStampUri = new Uri(TimeServerUrl),
    CertPath = "./Resources/Signing/cert.pfx",
    Password = certificatePassword,
};

// Helper functions

Task("ValidateNuGetStore").Does(() => {
    if(!GlobalNugetContainsSource("https://www.myget.org/F/wlvpn/"))
    {
        Warning("WLVPN MyGet repository not found in global store. Adding repository to local store.");
        NuGetAddSource("WLVPN MyGet Repository", "https://www.myget.org/F/wlvpn/auth/" + mygetApiKey + "/api/v3/index.json");
    }
});


public bool GlobalNugetContainsSource(string partialSource)
{
    try
    {
        XDocument nugetConfig = XDocument.Load(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "NuGet", "NuGet.Config"));
        foreach (var row in nugetConfig.Root.Elements("packageSources").First().Descendants("add"))
        {
            if (row.Attribute("value").Value.Contains(partialSource))
            {
                return true;
            }
        }
    }
    catch(Exception e)
    {
        // ignored
    }
    return false;
}

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
{
    CleanDirectory(buildDir);
});

Task("Restore-NuGet-Packages")
    .IsDependentOn("Clean")
	.IsDependentOn("ValidateNuGetStore")
    .Does(() =>
{
    NuGetRestore("./ConsumerVPN.sln");
});

Task("Build")
    .IsDependentOn("Restore-NuGet-Packages")
    .Does(() =>
{
    // Build installer and application. The bootstrapper automatically builds the application.
    MSBuild("./src/Bootstrapper/Bootstrapper.wixproj", settings => {
		     settings.SetConfiguration(configuration)
			 		.WithProperty("SolutionDir", solutionDir.FullPath + "\\")
				.WithProperty("CertificatePassword", certificatePassword)
				.WithProperty("UsesEVCert", UsesEVCert.ToString());
			 settings.ToolVersion = MSBuildToolVersion.VS2019;
		});    


    // Move installer to root of solution directory.
    var outputFile = Directory("./src/Bootstrapper/bin/") + Directory(configuration) + File("Bootstrapper.exe");
    var clientFile = Directory("./src/App/bin/") + Directory(configuration) + File(ApplicationName.ToString() + ".exe");
    var versionNumber = GetFullVersionNumber(clientFile);
    var setupFilename = String.Format("Setup_{0}.exe", versionNumber);
    if(FileExists("./" + setupFilename))
    {
        DeleteFile("./" + setupFilename);
    }

    MoveFile(outputFile, "./" + setupFilename);

    if(FileExists("/Resources/Signing/cert.pfx") && UsesEVCert)
    {
        // Sign the setup file.
        Sign("./" + setupFilename, signToolSettings);
    }
});


//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("Build");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
