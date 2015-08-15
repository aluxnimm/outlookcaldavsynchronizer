## Outlook CalDav Synchronizer ##

Outlook Plugin, which synchronizes events between Outlook and Google, SOGo, Horde or any other CalDAV server. Supported Outlook versions are 2013 and 2010.

### Project Homepage ###
[https://sourceforge.net/projects/outlookcaldavsynchronizer/](https://sourceforge.net/projects/outlookcaldavsynchronizer/)

### License ###
[Affero GNU Public License](http://sourceforge.net/directory/license:osi-approved-open-source/affero-gnu-public-license/)

### Authors ###

- [Gerhard Zehetbauer](https://sourceforge.net/u/nertsch/profile/)
- [Alexander Nimmervoll](https://sourceforge.net/u/nimm/profile/)

This project was initially developed as a master thesis project at the [University of Applied Sciences Technikum Wien](http://www.technikum-wien.at), Software Engineering Degree program 

### Tested CalDAV Servers ###

- SOGo
- Horde Kronolith
- OwnCloud
- SabreDAV
- Google Calendar ( use the following URL: https://www.google.com/calendar/dav/<your_google_calendar_id>/events )
- Zimbra 8.5
- GroupOffice
- Synology NAS

### Features ###

- open source AGPL, the only free Outlook CalDav plugin
- two-way-sync
- SSL/TLS support, support for self-signed certificates
- configurable sync range
- sync multiple calendars per profile
- sync reminders, categories, recurrences with exceptions, importance, transparency
- sync organizer and attendees and own response status
- task support (alpha)
- CardDav is planned 

### Used Libraries ###

-  [DDay.iCal](http://www.ddaysoftware.com/Pages/Projects/DDay.iCal/)
-  [Apache log4net](https://logging.apache.org/log4net/)

### Install instructions ###

Download and extract the `OutlookCalDavSynchronizer-<Version>.zip` into the same folder and start setup.exe.
If the installer is complaining about the missing Visual Studio 2010 Tools for Office Runtime, install it manually from [Microsoft Download Link](https://www.microsoft.com/en-us/download/details.aspx?id=44074)

### Changlog ###

#### 0.99.16 ####
- Make filtering for time range optional

#### 0.99.15 ####
- Implement app.config options for disabling SSL/TLS certificate validation and enabling/disabling SSL3/TLS12

#### 0.99.14 ####
- Use BYSETPOS also for other instances from Outlook fixes first/second/... weekday/weekend day in month

#### 0.99.13 ####
- Use BYSETPOS -1 to fix last (working)day monthly/yearly recurrences from Outlook
- Add UserAgent header to request. (needed by BAIKAL for example)

#### 0.99.12 ####
- Fix Bug: Mapping is wrong, when master event is not first event in CalDAV resource.
- Fix logging issue
- Fix recurrence from Outlook for last weekday in month

#### 0.99.10 ####
- Fetch Etag from server, if it is not included in an update response.
- Factor out CalDavWebClient from CalDavDataAccess.

#### 0.99.9 ####
- Dispose Folders in OptionDialog
- Call GarbageCollector after each synchronizer run to avoid issues with recurrence exceptions

#### 0.99.8 ####
- Fix some caldav timeout issues, properly dispose WebRequests

#### 0.99.7 ####
- Check if organizer address is empty to avoid COM Exception in GetMailUrl
- Fix exdate calculation for moved recurrence exceptions

#### 0.99.6 ####
- Catch 404 response for empty caldav repositories from Synology
- Some generic Refactoring

#### 0.99.4 ####
- Dispose Outlook-Folders after usage
- Some more recurrence exception fixes:
- If Outlook provides a Changed-Exception and a Deleted-Exception with the same OriginalDate the Deleted-Exception is discarded
- Prevent skipping of Appointment-exceptions while moving
- Swap handling of ExeptionDates and RecurrenceIDs
- Clarify logging error
- check if new exception is already present in target

#### 0.99.3 ####
- Fix timezone definition

#### 0.99.2 ####
- fix timezone issues for syncing from Outlook to CalDav for recurrent events
- Add local timezone info to new CalDav events
- Set start and end date in local timezone instead of utc for CalDav events
- fixes recurrent events that span over daylight saving time changes
- fix calculation of exdates for recurrence exceptions
- Fix date calculation for GetOccurence
- Honor BYSETPOS for monthly and yearly recurrence rules

#### 0.99.1 ####
- Improved validation of calendar url in options dialog

#### 0.99 ####
- Fixes for google

#### 0.98 ####
- Add SCHEDULE-AGENT=CLIENT for organizer to avoid sending invitations twice in SOGo, see ticket 45

#### 0.97.8 ####
- Add debug logging for caldav requests

#### 0.97.7 ####
- more fixes for exchange email addresses for attendees and better logging
- catch COM exception for not found recurrence exceptions

#### 0.97.6 ####
- more fixes for GetMailUrl
- Improve handling of WebExceptions

#### 0.97.5 ####
- fixes for GetMailUrl for Exchange and GAL
- fix WebException exceptions
- response header 'location' is allowed to contain a relative Uri

#### 0.97.3 ####
- fix interval for yearly recurrence rules

#### 0.97.2 ####
- add synchronization context if missing
- fix for task sync if start and due dates are equal

#### 0.97.1 ####
- swap default values for sync timespans in options dialog
- catch exceptions if PR_SMTP_ADDRESS property not available
- set meetingstatus to nonmeeting if only own organizer and no attendees are present

#### 0.97 ####
- Initial task sync support (alpha)
- Make caldav requests async, Outlook UI stays responsive during caldav get requests
- some recurrence fixes
- improve total progress handling

#### 0.96 ####
- Fixes for google

#### 0.95.1 ####
- Fix getting smtp address for exchange users

#### 0.95 ####
- Implement 301,302 redirects to support Zimbra
- Add validation for options

#### 0.94 ####
- Fix exception in initial mapping if event subject or summary is null

### User Documentation ###

After installing the plugin, a new ribbon called 'Caldav Synchronizer' is added in Outlook with 3 menu items. 
- Synchronize now
- Options
- About

Use the Options dialog to configure different synchronization profiles. Each profile is responsible for synchronizing one Outlook calendar or task folder with a remote folder of a CalDAV server.

- Add adds a new empty profile
- Delete deletes the current profile
- Copy copies the current profile to a new one

The following properties need to be set for a new profile:

- *Profile name*: An arbitrary name for the profile, which will be displayed at the associated tab.
- *Server settings*:
	- **CalDAV Url:** URL of the remote CalDAV folder. You should use a HTTPS connection here for security reason! If you only have a self signed certificate, add the self signed cert to the Local Computer Trusted Root Certification Authorities. You can import the cert by running the MMC as Administrator. If that fails, see section *'Debugging and more config options'*
	- **Username:** Username to connect to the CalDAV server
	- **Password:** Password used for the connection. The password will be saved encrypted in the option config file.
	- **Email address:** email address used as remote identity for the CalDAV server, necessary to synchronize the organizer
- *Outlook settings*:
	- **Outlook Folder:** Outlook folder that should be used for synchronization
- *Sync settings*:
	- Synchronization settings
		- **Outlook -> CalDav (Replicate):** syncronizes everything from outlook to caldav server (one way)
		- **Outlook <- CalDav (Replicate):** synchronizes everything from caldav server to outlook (one way)
		- **Outlook -> CalDav (Merge):** synchronizes everything from outlook to caldav server but don't change events created in caldav server
		- **Outlook <- CalDav (Merge):** synchronizes everything from caldav server to outlook but don't change events created in outlook
		- **Outlook <-> CalDav:** 2-way synchronization between Outlook and CalDav server with one of the following conflict resolution
	- Conflict resolution (only used in 2-way synchronization mode)
		- **Outlook Wins:** If an event is modified in Outlook and in CalDav server since last snyc, use the Outlook version. If an event is modified in Outlook and deleted in CalDav server since last snyc, also use the Outlook version. If an event is deleted in Outlook and modified in CalDav server, also delete it in CalDav server.
		- **Server Wins:** If an event is modified in Outlook and in CalDav server since last snyc, use the CalDav server version. If an event is modified in Outlook and deleted in CalDav server since last snyc, also delete it in Outlook. If an event is deleted in Outlook and modified in CalDav server, recreate it in Outlook.
		- **Automatic:** If event is modified in Outlook and in CalDav server since last snyc, use the last recent modified version. If an event is modified in Outlook and deleted in CalDav server since last snyc, delete it also in Outlook. If an event is deleted in Outlook and modified in CalDav server, also delete it in CalDav server
	- **Synchronization interval (minutes):** Choose the interval for synchronization in minutes, if 'Manual only' is choosen, there is no automatic sync but you can use the 'Synchronize now' menu item.
	- **Synchronization timespan past (days)** and
	- **Synchronization timespan future (days):** For performance reasons it is useful to sync only a given timespan of a big calendar, especially past events are normally not necessary to sync after a given timespan
	- **Deactivate profile:** If activated, current profile is not synced anymore without the need to delete the profile
	
### Trouble Shooting ###

Options and state information is stored in the following folder:

    C:\Users\<Your Username>\AppData\Local\CalDavSychronizer

There is one `options_<your outlook profile>.xml` file which stores the options for each outlook profile.
For each sync profile there is a subfolder with state information stored in a relations.xml file after the inital sync. If you delete that folder, a fresh inital sync is performed.

Each synchronization attempt is logged in the `log.txt` file. There you can find information about sync duration and the amount of added, deleted or modified events. Errors and Exceptions are logged aswell. For debugging information of caldav requests there is furthermore a logfile `log_calDavAccess.txt`

 
### Debugging and more config options ###

In the install dir (The default is `'C:\Program Files (x86)\Gerhard Zehetbauer\CalDavSynchronizer'`) you will find the app config file

    CalDavSynchronizer.dll.config

In that xml file you can config timeout parameters and config options in the section `appSettings`
After changing parameters you have to restart Outlook.

- **loadOperationThresholdForProgressDisplay**: amount of sync operations to show the progress bar (default 50)
- **calDavConnectTimeout**: timeout for caldav connects (default 90 sec)
- **calDavReadWriteTimeout**; timeout for caldav read/write requests (default 5 sec)
- **enableTaskSynchronization** Support for task sync (alpha) true or false

If you have problems with SSL/TLS and self-signed certificates, you can change the following settings at your own risk.
The recommended way would be to add the self signed cert to the Local Computer Trusted Root Certification Authorities
You can import the cert by running the MMC as Administrator.

- **disableCertificateValidation** set to true to disable SSL/TLS certificate validation, major security risk, use with caution!
- **enableSsl3** set to true to enable deprecated SSLv3, major security risk, use with caution! 
- **enableTls12** set to false to disable TLS12, not recommended 

In the section `log4net` you can define the log level for the main log and for the caldav data access, 
    level value can be DEBUG or INFO
    
