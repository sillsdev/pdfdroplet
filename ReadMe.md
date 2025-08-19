PdfDroplet is a free, simple little gui tool which only does one thing:  it takes your PDF and gives you a new one, with the pages combined and reordered, ready for saving & printing as booklets.

PdfDroplet.exe can also be used as a library for making booklets in other programs. [Bloom](https://github.com/BloomBooks/BloomDesktop) uses it this way.

The gui has only been released on Windows, but on Linux PdfDroplet has been used as a non-gui library.

## Building ##

Build the solution PdfDroplet.sln. On Windows, use at least the 2022 Community edition of Visual Studio.  On Linux, you need msbuild and at least mono 6.

In order to get a preview of the output inside of PdfDroplet, you will need a PDF viewer which integrates with Internet Explorer. Acrobat Reader, PdfXchange, and FoxIt all do this. Or, use https://github.com/sillsdev/SmallPdfDropletTest to test via command line.

## Disable Analytics

We don't want developer and tester runs (and crashes) polluting our statistics. Add the environment variable "feedback" with value "off".


## Continuous Build System

Each time code is checked in, an automatic build begins on our [TeamCity build server](http://build.palaso.org/project.html?projectId=PdfDroplet). Similarly, when there is a new version of a PDFDroplet dependency (e.g. SIL.Core), that server automatically rebuilds PDFDroplet.
