# Development Process

## Continuous Build System

Each time code is checked in, an automatic build begins on our [TeamCity build server](http://build.palaso.org/project.html?projectId=PdfDroplet). Similarly, when there is a new version of a PDFDroplet dependency (e.g. SIL.Core), that server automatically rebuilds PDFDroplet.

## Building C# Source Code ##

To build PDFDroplet you'll need at least a 2015 Community edition of Visual Studio.

Before you'll be able to build you'll have to download some binary dependencies (see below).

To build PDFDroplet you can open and build the solution in Visual Studio or build from the command line using msbuild.

## Get Binary Dependencies

You will need a way to run bash scripts (e.g. Git BASH, included with [Git for Windows](https://git-for-windows.github.io/)).

In the `build` directory, run

`buildupdate.windows.sh`

It could take a minute the first time; afterwards, it will be quick as it only downloads what has changed. When you change branches, run this again.

#### About PdfDroplet Dependencies

Our [Palaso libraries](https://github.com/sillsdev/libpalaso)** hold the classes that are common to multiple products.

## Disable Analytics

We don't want developer and tester runs (and crashes) polluting our statistics. On Windows, add the environment variable "feedback" with value "off".