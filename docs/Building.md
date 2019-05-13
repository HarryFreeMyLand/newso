# Building NewSO

## Prerequisites

### Visual Studio

You will need Visual Studio 2017 or above workloads:

- .NET desktop development
- ASP.NET and web development
- .NET Core cross-platform development

Additionally, you may need to go to Individual components and install the .NET Framework 4.7.2 and SDK and targeting pack, if it isn't already installed.

### MonoGame

NewSO uses a modified version of MonoGame that's pulled in a separate repostiry that's pulled in with a submodule. Github Desktop users will have these submodules automatically and recursively fetched and cloned, others will have to use ``git submodule update --init --recursive``.

Finally, you'll have to go into ``newso\Other\libs\FSOMonoGame`` and click ``Protobuild.exe`` on Windows or type ``mono Protobuild.exe`` on Unix system in order generate the respective project files needed for NewSO to build correctly.


## Instructions

Open up ``NewSO.sln`` and make sure ``FSO.Windows`` is highlighted even if The Sims Online isn't properly installed.