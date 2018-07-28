# CK2 PortraitBuilder

<table>
  <tr>
    <th style="text-align:center">Build Server</th>
    <th>Framework</th>
    <th style="text-align:center">Status</th>
  </tr>
  <tr>
    <td style="text-align:center">AppVeyor</td>
    <td>.NET 4.6</td>
    <td style="text-align:center"><a href="https://ci.appveyor.com/project/rquinio/portraitbuilder/branch/master"><img src="https://ci.appveyor.com/api/projects/status/ssardstb8qkm35sy/branch/master?svg=true" alt="AppVeyor build status" /></a></td>
  </tr>
  <tr>
    <td style="text-align:center">Travis</td>
    <td>Mono 5.x</td>
    <td style="text-align:center"><a href="https://travis-ci.org/rquinio/PortraitBuilder"><img src="https://travis-ci.org/rquinio/PortraitBuilder.svg?branch=master" alt="Travis build status" /></a></td>
  </tr>
</table>

PortraitBuilder is a Winforms application to help CK2 modders creating custom characters, by previewing their appearance, which can then be used in character history of mods, or to customize a character in a saved game.

This project is a fork of [Measter's 1.x PortraitBuilder](https://github.com/Measter/PortraitBuilder).

See the dedicated [CK2 forum thread](https://forum.paradoxplaza.com/forum/index.php?threads/utility-portrait-builder-v2.941117/), after registering your CK2 game.

## Features

- Reading portraits from vanilla, DLCs zip archives and mod folders
- Saving current portrait as png image
- Import/export of DNA & properties strings

## .NET users (Windows)

- Install [.NET 4.6](https://www.microsoft.com/en-US/download/details.aspx?id=48130)
- Start PortraitBuilder.exe
- Select your CK2 game executable (ex: C:\Program Files (x86)\Steam\SteamApps\common\Crusader Kings II\CK2game.exe). This value is kept into a file "gamedir.txt".
- Errors are logged to a log.txt file. Adding -logfull to PortraitBuilder.exe launch options will set log level to DEBUG.

## Mono users (Linux/Mac/Windows)

- Install [Mono 5.x](http://www.mono-project.com/download/). Note: GTK# is not required.
- Run ". PortraitBuilder" which is an alias for "MONO_PATH=./lib mono PortraitBuilder.exe"
Note: on MacOS, use: "MONO_PATH=./lib mono --arch=32 PortraitBuilder.exe", as [WinForms only work with Mono 32bits on MacOS](http://www.mono-project.com/docs/about-mono/supported-platforms/osx/#32-and-64-bit-support)
- Select your CK2 game executable (ex: ~/.local/share/Steam/SteamApps/common/Crusader Kings II/ck2 or /Users/USER/Library/Application Support/Steam/SteamApps/common/crusader kings ii/ck2.app). This value is kept into a file "gamedir.txt".
- Errors are logged to a log.txt file

## Developers

Install [Visual Studio Community 2017](https://www.visualstudio.com/downloads/).

Build from PortraitBuilder.sln, it will automatically download dependencies via NuGet from [packages.config](PortraitBuilder/packages.config) file.

The built executable for .NET can also be executed with Mono, without needing to re-compile.

Main dependencies:

- [dds-reader](https://github.com/andburn/dds-reader) to read .dds images. A recompiled version targeting .NET 4.5 is used from /lib.
- [Hime Parser Generator](https://bitbucket.org/cenotelie/hime/) to perform a basic parsing of CK2 files.
- [SharpZipLib](http://www.icsharpcode.net/opensource/sharpziplib/) to unzip DLCs
- [log4net](https://logging.apache.org/log4net/) to handle logging

To re-generate the Lexer/Parser from the grammar, install [Hime standalone distribution](https://bitbucket.org/cenotelie/hime/downloads/), and use the himecc command line utility:

> himecc PortraitReader.gram -n PortraitBuilder.Parser

### Using Ubunutu on Windows 10

Create the following symlinks to simulate a CK2 Steam installation on Unix:

- Game files: ln -s /mnt/c/Program\ Files\ \(x86\)/Steam ~/.local/share/Steam
- Shortcut in home folder: ln -s ~/.local/share/Steam/SteamApps/common/Crusader\ Kings\ II ~/ck2
- Mod folder: ln -s /mnt/c/Users/[user]/Documents/Paradox\ Interactive ~/.paradoxinteractive
