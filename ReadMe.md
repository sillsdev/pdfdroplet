PdfDroplet is a free, simple little gui tool which only does one thing:  it takes your PDF and gives you a new one, with the pages combined and reordered, ready for saving & printing as booklets.

PdfDroplet.exe can also be used as a library for making booklets in other programs. [Bloom](https://github.com/BloomBooks/BloomDesktop) uses it this way.

The gui has only been released on Windows, but on Linux PdfDroplet has been used as a non-gui library.

## Building ##

In the `build` directory, run

`buildupdate.windows.sh` or 'buildupdate.linux.sh'

(To do this on windows, you'll need some way to run bash scripts.)

That script will fetch dependencies from our TeamCity server. It could take a minute the first time; afterwards, it will be quick as it only downloads what has changed. When you change branches, run this again.

Next build the solution. On Windows, use at least the 2015 Community edition of Visual Studio.

In order to get a preview of the output inside of PdfDroplet, you will need a PDF viewer which integrates with Internet Explorer. Acrobat Reader, PdfXchange, and FoxIt all do this.

## Disable Analytics

We don't want developer and tester runs (and crashes) polluting our statistics. Add the environment variable "feedback" with value "off".


## Continuous Build System

Each time code is checked in, an automatic build begins on our [TeamCity build server](http://build.palaso.org/project.html?projectId=PdfDroplet). Similarly, when there is a new version of a PDFDroplet dependency (e.g. SIL.Core), that server automatically rebuilds PDFDroplet.
