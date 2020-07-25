# KindleClippingsParser
Parses your My Clippings.txt file, splitting them by book, removing duplicates, and outputting them in usable formats.

**Usage**

Put the executable in the same folder as your My Clippings.txt and run.

Or, drag and drop your My Clippings.txt onto the executable.

Or, if you want to specify a format, use KindleClippinsgParser.exe --format [format] [clippings file]. Currently available formats: pages, nopages, json.

**Platforms**

Should run anywhere .NET Core runs. Windows, Linux, MacOS.

**Adding New Formats**

Just add a new class that implements the IOutputFormat interface, the rest will happen automagically. See Output.cs.
