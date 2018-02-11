# CK2 PortraitBuilder

<table>
  <tr>
    <th style="text-align:center">Build Server</th>
    <th>Framework</th>
    <th style="text-align:center">Status</th>
  </tr>
  <tr>
    <td style="text-align:center">AppVeyor</td>
    <td>.NET 4.5</td>
    <td style="text-align:center"><a href="https://ci.appveyor.com/project/rquinio/portraitbuilder/branch/master"><img src="https://ci.appveyor.com/api/projects/status/ssardstb8qkm35sy/branch/master?svg=true" alt="AppVeyor build status" /></a></td>
  </tr>
  <tr>
    <td style="text-align:center">Travis</td>
    <td>Mono 4.4</td>
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

## Users

- Install [.NET 4.5](https://www.microsoft.com/en-gb/download/details.aspx?id=30653)
- Start PortraitBuilder.exe
- Select your CK2 game directory (ex: C:\Program Files (x86)\Steam\SteamApps\common\Crusader Kings II). This value is kept into a file "gamedir".
- Errors are logged to a log.txt file. Adding -logfull to PortraitBuilder.exe launch options will set log level to DEBUG.

## Developers

Build from PortraitBuilder.sln, it will automatically download dependencies via NuGet from [packages.config](PortraitBuilder/packages.config) file.

Main dependencies:

- [dds-reader](https://github.com/andburn/dds-reader) to read .dds images
- [Hime Parser Generator](https://bitbucket.org/cenotelie/hime/) to perform a basic parsing of CK2 files.
- [SharpZipLib](http://www.icsharpcode.net/opensource/sharpziplib/) to unzip DLCs
- [log4net](https://logging.apache.org/log4net/) to handle logging

To re-generate the Lexer/Parser from the grammar, install [Hime standalone distribution](https://bitbucket.org/cenotelie/hime/downloads/), and use the himecc command line utility:

> himecc PortraitReader.gram -n PortraitBuilder.Parser
