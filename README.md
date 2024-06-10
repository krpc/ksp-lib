KSP Libs
========

This repository contains the libraries needed for automated builds of kRPC, primarily used by GitHub actions.

The KSP libraries are stripped (all implementation is removed) so as not to be redistributing copyrighted material.

Building CILStrip on Ubuntu
---------------------------

```
sudo apt install dotnet6
dotnet build cilstrip/CILStrip.csproj
```
