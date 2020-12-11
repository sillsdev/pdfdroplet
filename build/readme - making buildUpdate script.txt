This project formerly used buildUpdate.sh to get the relevant dependencies from TeamCity artifacts. This is no longer needed because these are taken from Nuget packages now instead of TeamCity artifacts.

As a result, I have deleted buildupdate.mono.sh and buildupdate.windows.sh since you do not need to run these anymore before compiling this project.

Below you will find the old instructions, in case in the future, you do need to set it up to download some other dependencies from Teamcity.
--------------------------------------------------
(Re)building buildUpdate.sh requires ruby and https://github.com/chrisvire/BuildUpdate
Here's the command line commands I used:

cd <path to where you want to generate the update scripts>
<your path to buildupdate.rb (part of BuildUpdate repo above)>\buildupdate.rb -t bt54 -f buildupdate.windows.sh -r ..
<your path to buildupdate.rb (part of BuildUpdate repo above)>\buildupdate.rb -t bt344 -f buildupdate.mono.sh -r ..

Explanation:

"-t bt54" points at the Windows configuration that tracks this branch
"-t bt344" points at the Linux configuration that tracks this branch
"-f ____" gives what I want the file to be called
"-r .." takes care of moving the context up from build to the root directory