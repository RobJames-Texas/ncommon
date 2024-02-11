# NCommon
NCommon is a light weight framework that provides implementations of commonly used design patterns for applications using a Domain Driven Design approach. 

## Building NCommon
Building NCommon is done via a [psake] (http://github.com/JamesKovacs/psake) script. Before running the psake build script, make sure you have Powershell 2.0 installed. 

> Import-Module .\psake.psm1  
> Invoke-psake .\default.ps1  

NCommon binaries are built and placed in an **out** directory under the root folder. 

For documentation on NCommon, visit http://riteshrao.github.com/ncommon

-----

Updated to .Net Framework 4.8
Changed to package references
Updated nuget packages to recent versions
Updated code as required for namespace changes and deprications

I'm supporting a legacy system that still uses this library and it being out of date was causing me headaches.
