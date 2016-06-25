# CK2 PortraitBuilder

This project is a fork of [Measter's 1.x PortraitBuilder](https://github.com/Measter/PortraitBuilder).

See the dedicated [CK2 forum thread](https://forum.paradoxplaza.com/forum/index.php?threads/utility-portrait-builder-v2.941117/), after registering your CK2 game.

## Description

This tool allows to create custom characters, by previewing their appearance, which can then be used in character history of mods, or to customize a character in a saved game.

Features:

- Reading portraits from vanilla, DLCs zip archives and mod folders
- Saving current portrait as png image
- Import/export of DNA & properties strings

## Users

- Install [.NET 4.5](https://www.microsoft.com/en-gb/download/details.aspx?id=30653)
- Start PortraitBuilder.exe
- Select your CK2 game directory (ex: C:\Program Files (x86)\Steam\SteamApps\common\Crusader Kings II). This value is kept into a file "gamedir".
- Errors are logged to a log.txt file. Adding -logfull to PortraitBuilder.exe launch options will set log level to DEBUG.

## Developers

Dependencies:

- The DevIL library, with the DevIL.Net wrapper: http://openil.sourceforge.net/. Note: DevIL.dll needs to be copied manually from libs/ to bin/Debug or bin/Release
- Hime Parser Generator: http://himeparser.codeplex.com/
- SharpZipLib: http://www.icsharpcode.net/opensource/sharpziplib/
- log4net: https://logging.apache.org/log4net/

To re-generate the Lexer/Parser from the grammar, install Hime Parser Generator v0.5.0 (won't work with v1.0), and run the following:

> himecc PortraitReader.gram -n PortraitBuilder.Parser
