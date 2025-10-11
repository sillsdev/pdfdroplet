DesktopAnalytics.net
===============================

[Segment.IO](http://segment.io) provides a nice <i>server-oriented</i> library in [Analytics.net](https://github.com/segmentio/Analytics.NET). This project is just one little class that wraps that to provide some standard behavior needed by <i>desktop</i> applications:

+ Supplies guids for user ids, saves that in a Settings file
+ Set $Browser to the Operating System Version (e.g. "Windows 8").
+ Auto events
 + First launch ("Create" to fit MixPanel's expectation)
 + Version Upgrade

##Usage

###Adding to your project

1) Follow the instructions at [Segment.io](https://segment.io/libraries/.net) to add the Analytics package.

2) Clone and build this project, or download from TODO

###Initialization
    using (new Analytics("mySegmentIOSecret"))
	{	
		//run your app UI
	}

###Tracking

    Analytics.Track("Create New Image");

or

    Analytics.Track("Save PDF", new Dictionary<string, string>() {
			{"PageCount",  pageCount}, 
			{"Layout", "A4Landscape"}
        });

###Error Reporting

    Analytics.ReportException(error);

##Dependencies

The project is currently built for .net 4 client profile. If you get the solution, nuget should auto-restore the two dependencies when you build; they are not part of the source tree.

##License

MIT Licensed
(As are the dependencies, Analytics.Net and Json.Net).

##Roadmap
Add user notification of tracking and opt-out UI.

Add opt-in user identification (e.g., email)