// (C) 2025 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 09.04.2025 16:04

using System.Reflection;
using System.Runtime.InteropServices;
using Android;
using Android.App;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("BDA")]
[assembly: ComVisible(false)]
[assembly: UsesFeature("android.hardware.wifi", Required = false)]
[assembly: UsesPermission(Manifest.Permission.Flashlight)]
[assembly: UsesPermission(Manifest.Permission.Camera)]
[assembly: UsesPermission(Manifest.Permission.PostNotifications)]
[assembly: AssemblyVersion("1.2.8.0")]
[assembly: AssemblyFileVersion("1.2.8.0")]
[assembly: AssemblyProduct("BDA")]
[assembly: AssemblyDescription("BDA Forschungsprojekt")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("FOTEC Forschungs- und Technologietransfer GmbH")]
[assembly: AssemblyCopyright("(C) 2009-2022 FOTEC Forschungs- und Technologietransfer GmbH")]
[assembly: AssemblyCulture("")]
[assembly: AssemblyTrademark("(C) 2009-2022 FOTEC Forschungs- und Technologietransfer GmbH")]