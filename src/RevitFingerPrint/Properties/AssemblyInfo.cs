using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Metamorphosis")]
[assembly: AssemblyDescription("")]

#if REVIT2015
[assembly: AssemblyConfiguration("Revit 2015")]
#endif 
#if REVIT2016
[assembly: AssemblyConfiguration("Revit 2016")]
#endif

#if REVIT2017
[assembly: AssemblyConfiguration("Revit 2017")]
#endif 
#if REVIT2018
[assembly: AssemblyConfiguration("Revit 2018")]
#endif 
#if REVIT2019
[assembly: AssemblyConfiguration("Revit 2019")]
#endif 
#if REVIT2020
[assembly: AssemblyConfiguration("Revit 2020")]
#endif 
#if REVIT2021
[assembly: AssemblyConfiguration("Revit 2021")]
#endif 

[assembly: AssemblyCompany("Team Metamorphosis / AEC Hackathon 2016")]
[assembly: AssemblyProduct("Metamorphosis")]
[assembly: AssemblyCopyright("MNM, KM, CP, TH Copyright ©  2016-2020")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("997fca6e-9ee3-48b0-8deb-30149ba1a88b")]

// Make this assembly visible to our friend the Dynamo node!
[assembly:InternalsVisibleTo("MetamorphosisDynamo")]
// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("2.0.0.0")]
[assembly: AssemblyFileVersion("2.0.0.0")]
