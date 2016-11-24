## Outlook CalDav Synchronizer ##

Outlook Plugin, which synchronizes events, tasks and contacts between Outlook and Google, SOGo, Horde or any other CalDAV or CardDAV server. Supported Outlook versions are 2016, 2013, 2010 and 2007.

### Project Homepage ###
[https://sourceforge.net/projects/outlookcaldavsynchronizer/](https://sourceforge.net/projects/outlookcaldavsynchronizer/)

### License ###
[Affero GNU Public License](http://sourceforge.net/directory/license:osi-approved-open-source/affero-gnu-public-license/)

### Authors ###

- [Gerhard Zehetbauer](https://sourceforge.net/u/nertsch/profile/)
- [Alexander Nimmervoll](https://sourceforge.net/u/nimm/profile/)

This project was initially developed as a master thesis project at the [University of Applied Sciences Technikum Wien](http://www.technikum-wien.at), Software Engineering Degree program.
Outlook CalDav Synchronizer is Free and Open-Source Software (FOSS), still you can support the project by donating on Sourceforge or directly at PayPal

[![paypal](https://www.paypalobjects.com/en_US/i/btn/btn_donateCC_LG.gif)](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=PWA2N6P5WRSJJ&lc=US).

### Collaboration with Nextcloud ###

New collaboration with Nextcloud, see [https://nextcloud.com/blog/nextcloud-offers-caldav-synchronizer-for-outlook-users/](https://nextcloud.com/blog/nextcloud-offers-caldav-synchronizer-for-outlook-users/) 

For possible enterprise support, please contact us!

### Tested CalDAV Servers ###

- SOGo
- Horde Kronolith
- Nextcloud
- Owncloud
- SabreDAV
- Google Calendar
- Zimbra 8.5
- GroupOffice
- Synology NAS
- One.com
- DAViCal
- Baïkal
- Yandex
- OpenX-change
- Posteo
- Landmarks
- Kolab
- Zoho Calendar
- GMX
- Tine 2.0
- Fruux
- Mac OS X Server
- iCloud
- mail.ru
- Radicale
- Cozy Cloud
- Cyrus Imap 2.5

### Features ###

- open source AGPL, the only free Outlook CalDav plugin
- two-way-sync
- SSL/TLS support, support for self-signed certificates
- Manual proxy configuration support for NTLM or basic auth proxies
- Autodiscovery of calendars and addressbooks
- configurable sync range
- sync multiple calendars per profile
- sync reminders, categories, recurrences with exceptions, importance, transparency
- sync organizer and attendees and own response status
- task support
- Google native Contacts API support with mapping of Google contact groups to Outlook categories.
- Google Tasklists support (sync via Google Task Api with Outlook task folders)
- CardDAV support to sync contacts (distribution lists planned)
- time-triggered-sync
- change-triggered-sync
- manual-triggered-sync
- Category Filtering (sync CalDAV calendar/tasks to Outlook categories)
- map CalDAV server colors to Outlook category colors
- show reports of last sync runs and status
- System TrayIcon with notifications
- bulk creation of multiple profiles
- Use server settings from Outlook IMAP/POP3 account profile
- Map Windows to standard IANA/Olson timezones
- Configurable mapping of Outlook custom properties

### Used Libraries ###

-  [DDay.iCal](http://www.ddaysoftware.com/Pages/Projects/DDay.iCal/)
-  [Apache log4net](https://logging.apache.org/log4net/)
-  [Thought.vCard](http://nugetmusthaves.com/Package/Thought.vCards)
-  [NodaTime](http://nodatime.org/)
-  [ColorMine](https://www.nuget.org/packages/ColorMine/)

### Install instructions ###

Download and extract the `OutlookCalDavSynchronizer-<Version>.zip` into any directory and start setup.exe. You can change the default install path, but you need to use a directory on the `C:\` drive.
If the installer is complaining about the missing Visual Studio 2010 Tools for Office Runtime, install it manually from [Microsoft Download Link](https://www.microsoft.com/en-us/download/details.aspx?id=48217)
You should also update manually to the latest Visual Studio 2010 Tools for Office Runtime (Version 10.0.60724) if you have an older version installed, since some COMExceptions have been fixed.

Beginning with version 2.9.0 the default install location is `ProgramFilesDir\CalDavSynchronizer\` and the installer remembers the chosen directory for the next updates. Also the install option to install for Everyone instead of the current user is working now for Outlook 2010 and higher, if you want to install the addin for all users on the current machine. For Outlook 2007 you can only install the addin for the current user.

We recommend updating to the latest .Net Framework but the minimal required version is .Net 4.5, which is not supported on Windows XP. If you need Outlook CalDav Synchronizer for Windows XP you can download a backport to .Net 4.0 from a forked project [here](https://sourceforge.net/projects/outlookcaldavsynchronizerxp/), thanks to [Salvatore Isaja](https://sourceforge.net/u/salvois/profile/) for the awesome work!

### Changelog ###

#### 2.10.0 ####
- New features
	- Add profile type for NextCloud.
	- Add general option to enable useUnsafeHeaderParsing, needed for Yahoo and cPanel Horde.
	- Improve Autodiscovery.
- Bug fixes
	- Fix installer for Office 64-bit installation for AllUsers deployment and copy registry keys to correct HKLM location, ticket #410.
	- Add scrollbar to sync profiles content control, gh issue 176.
	- Fix autodiscovery for iCloud CardDav, ticket #414.
	- Trigger sync also on Outlook startup when TriggerSyncAfterSendReceive is enabled in general options, ticket #415.
	- Catch COMException when Outlook item can't be found in sync reports.

#### 2.9.1 ####
- Hotfix
	- Fix reminder mapping for just upcoming reminders, regression intruduced in 2.9.0, ticket #406.
- New features
	- Add CheckForNewVersions, StoreAppDatainRoamingFolder and IncludeCustomMessageClasses as app.config keys as well, useful for All Users deployment to change defaults.
- Bug fixes
	- Improve CustomPropertyMapping Validation and check if properties are empty to avoid Nullreference Exceptions.
	- Update Google Api Nuget packages.

#### 2.9.0 ####
- New features
	- Add Profile Import/Export.
	- Improve Installer, remove Manufacturer from DefaultLocation and remember InstallDir in registry for updates.
	- Use passive install for updates.
	- Add toolbar buttons to expand and collapse all nodes in synchronization profiles.
	- Add general option to expand all nodes in synchronization profiles by default.	
- Bug fixes
	- Catch COMException when SyncObjects can't be accessed, github issue 175.
	- Fix installer for All users deployment.
	- Fix Map just upcoming reminders for recurring appointments, ticket #398.

#### 2.8.2 ####
- Bug fixes
	- Fix new profile creation for calendar and task profiles and properly initialize customPropertyMapping configuration.
	- Fix UserDefinedCustomPropertyMappings initialization to avoid Nullreference exceptions.
	- Fix formatting of errorMessage in profile validation.

#### 2.8.1 ####
- Bug fixes
	- Avoid Nullreference Exceptions when options are not saved after upgrade to 2.8.0, gh issue 174.
	

#### 2.8.0 ####
- New features
	- Configurable custom properties mapping for Appointments and Tasks.
	- Update NuGet packages for Google API and NUnit.

#### 2.7.0 ####
- New features
	- Map UID to GlobalAppointmentID for new meetings to avoid double events from Mail invites (only possible in Outlook 2013+).
	- Add option to perform CalDAV/CardDAV sync in chunks with configurable chunk size to avoid OutOfMemoryEceptions, ticket #390.
	- Add Button which opens profile data directory for debugging.
- Bug fixes
	- Avoid ArgumentNullException if appointments have no GlobalAppointmentID and log warning, ticket #389.
	- Update icon of profile in options, when OutlookFolderType of profile changes.
	- Fix for ToolBarButtons in Options.

#### 2.6.1 ####
- **WARNING**: This version changes the internal cache structure, when downgrading to an older version, the cache gets cleared and a new inital sync is performed!
- Bug fixes
	- Fix cache conversion for tasks.
	- Ensure synchronization context on every button click

#### 2.6.0 ####
- **WARNING**: This version changes the internal cache structure, when downgrading to an older version, the cache gets cleared and a new inital sync is performed!
- New features
	- Better support for meeting invitations.
	- Improve duplicate event cleaner.
	- Update Google Apis nuget packages to 1.16.0.
	- Include GlobalAppointmentId in RelationCache.
- Bug fixes
	- Update accepted meeting invitations instead of deleting and recreating them to avoid wrong cancellation mails from the CalDAV server.
	- Catch OverflowException for invalid birthdays in contacts, ticket #386.
	- DuplicateEventCleaner: catch exception if appointment doesn't exist anymore.
	- Avoid Nullreference Exception and don't add server resource when it doesn't contain any valid VEVENT or VTODO, gh issue 167.
	- Check if caldav resource is not empty to avoid ArgumentOutOfRangeException.

#### 2.5.1 ####
- New features
	- Add account type for Cozy Cloud and set UseIanaTz as default.
- Bug fixes
	- Set BusyStatus to tentative for meeting invites without response.
	- Follow also 307 redirects in WebDavRequests, fixes autodiscovery for Telstra BigPond.
	- Ensure that discovered resource uris end with slash.
	- Fix linebreak issues for Open-Xchange vcards, ticket #290.
	- Add TYPE=WORK to first Outlook Email Address and TYPE=HOME to second for CardDAV profiles and map work email to first Outlook Email Address and home email to second for CardDAV and google contact profiles.
	- Add default mapping of cell,work and home phone number if PhoneTypes are missing when syncing from CardDAV server to avoid loss of telephone numbers.
	- Exclude received meetings from immediate sync to avoid problems with doubled events.

#### 2.5.0 ####
- New features
	- Add mapping configuration to use IANA timezones instead of Windows timezones.
	- Make addin startup and EntityMapper async.
	- Add progressBar and download new version async, github issue 156.
- Bug fixes
	- Add SCHEDULE-AGENT=CLIENT also to attendees, ticket #354.
	- Avoid empty PARTSTAT and default to NEEDS-ACTION.
	- Add KeepOutlookFileAs option (defaults to true) for contacts to avoid overwriting existing FileAs with FN attribute.
	- Set Outlook contact FullName to FN as fallback if N is missing in vcard.
	- Set Outlook task status to Completed when complete date exists and percentComplete is 100 also when VTODO status is missing, gh issue 154.
	- Fix mapping organizer if CN and email are identical, gh issue 157.
	- Avoid DDay.ICal UTC calls and use NodaTime instead for conversion, gh issue 159.

#### 2.4.0 ####
- New features
	- Add Use Account Password also to bulk profile creation and add posibility to get server settings (DAV url, Email, Username) from Outlook IMAP/Pop3 Account.
	- Add mapping for task alarms with absolute date/time triggers.
	- Add category filter also for tasks, feature 48.
	- Download contact photo if provided by url, fixes contact photo mapping for GMX, ticket #358.
- Bug fixes
	- Change SOGo account profile url path to /SOGo/dav/.
	- Fix mapping of PostalAddress Country in Google Contacts API.
	- Fix mapping of PostalCode for Google Contacts, ticket #352.
	- Update NuGet packages for used external libraries.
	- Log warning and avoid COM Exception for recurring events and tasks when RRULE BYMONTH is invalid, ticket #334.
	- Use correct request URI in reports when server uri has different encoding than resource URI, github issue 152.

#### 2.3.1 ####
- Bug fixes
	- Fix OL2007 toolbar positioning and saving, ticket #351.
	- Use only Start, End and Subject for DuplicateEventCleaner, ticket #330.

#### 2.3.0 ####
- New features
	- Save Outlook 2007 toolBar position and visibility in Registry, github issue 102.
	- Implement duplicate event cleanup in Event mapping configuration.
	- Add CalDavConnectTimeout as general option (feature 46).
- Bug fixes
	- Fix commandbar for OL2007, ticket #339
	- Some fixes for recurrence exceptions if timezone of appointment is different to Outlook local timezone.
	- Fix setting organizer name in Outlook from CommonName and Email.
	- Use general option to show reports for warnings and/or errors also for systray notifications, github issue 143.
	- Map also default events to private when mapping option is activated, ticket #329.
	- Do not steal focus when showing ProgressForm (#328 Window steal focus when initiating sync).
	- validate input in GeneralOptionsForm.

#### 2.2.0 ####
- New features
	- Add general option to trigger sync after Outlook Send/Receive finishes, github issue 141.
	- Add event mapping configuration parameter to map CLASS:PUBLIC to Outlook Private flag, feature request 45.
	- Implement DNS SRV and TXT lookups for autodiscovery from email address.
- Bug fixes
	- Fix autodiscovery when server returns multiple calendar-home-set hrefs, github issue 139.

#### 2.1.3 ####
- New features
	- Add event mapping configuration to use Outlook GlobalAppointmentID for UID attribute, ticket #318.
- Bug fixes
	- Don't log warning if DTEND is not set for allday events, ticket #316.
	- Prefix summary of events and not only meetings with status cancelled, since Android uses this instead of exdates for recurrence exceptions, ticket #307.

#### 2.1.2 ####
- New features
	- Add ProfileType for SOGo.
- Bug fixes
	- Fix detecting deleted appointments from folders in local pst data files when using category filter, ticket #297.

#### 2.1.1 ####
- New features
	- Add ProfileType for Landmarks.
- Bug fixes
	- Avoid sync loops and delete new events, when they represent an invitation from server identity.
	- Fix event mapping of TRANSP to Outlook BusyStatus and use
X-MICROSOFT-CDO-BUSYSTATUS.
	- Fix Autodiscovery behavior in case of url textbox is empty.

#### 2.1.0 ####
- New features
	- Implement Bulk profile creation to add multiple profiles at once and choose the folder for each discovered server resource (calendar, addressbook and task).
	- Query supported-calendar-component-set and filter out VEVENT and VTODO resources for autodisovery.
- Bug fixes
	- add functionality to cope with multiple groups with the same name for Google Contacts API.
	- Restore old CalendarUrl when Google Autodiscovery has error or was cancelled.
	- Map ResponseStatus default to NEEDS-ACTION in MapParticipation1To2 to avoid exception.

#### 2.0.2 ####
- New features
	- Improve event mapping of TRANSP and STATUS to Outlook BusyStatus. (contributed by Florian Saller).
	- Improve Autodiscovery.
	- Add ProfileType for Sarenet.
- Bug fixes
	- Ignore invalid-xml-errors in EntityRelationDataAccess unless a new version has been saved.
	- Fetch all Google Contacts with a single request to avoid 503 errors.
	- Query just contacts from Default Group from Google Contacts API.
	- Do not log an error if delete or update fails because of concurrency effects.
	- Only access AddressEntry if recipient can be resolved and catch possible COMExceptions.

#### 2.0.1 ####
- New features
	- Initial support for mail.ru
	- Add option to keep Outlook photo in contact mapping configuration.
- Bug fixes
	- Catch COMException if birthday can't be set in Outlook, ticket #276.
	- Preserve current mapping configuration, if no folder selected.
	- Fix own identity handling in event mapping (especially for Exchange accounts).
	- Fix possible Nullreference Exception in CardDavDataAccess.GetEntities.
	- Check if key exists before adding to targetExceptionDatesByDate in MapRecurrence1To2, ticket #270.
	- Skip directoryresource, if returned from calendar-query, since mail.ru returns directory itself even with an etag.
	- Disable other checkboxes in schedule settings when Map Attendees is unchecked.
	- Remove X-ABCROP-RECTANGLE from vcard photo attributes since the deserializer can't parse the base64 value, ticket #274.
	- Disable sync now button during synchronization runs.

#### 2.0.0 ####
- New features
	- Add support for Google Contacts API to sync Outlook contact folders with Google contacts which improves mapping and performance, since the Google CardDAV API has some issues (first official release, beta)
	- Support for google contact groups, which are synced to Outlook categories.
	- Sync contact photos, WebPages, Notes, Sensitivity, Hobbies for google contacts.
	- Added mapping for anniversary, relations (spouse, child, etc.) and IMs for google contacts (Contribution from Florian Saller [https://sourceforge.net/u/floriwan/profile/](https://sourceforge.net/u/floriwan/profile/), thank you!)
	- Remove legacy synchronization profile settings user interface.
- Bug fixes
	- Add TYPE=JPEG to vcard photo attributes and catch exceptions in MapPhoto2To1.
	- Catch COM-Exception, when fetching Items from Outlook (ticket #263 Error when deleting contacts with Synchronize changes immediately after changes activated).
	- Fix possible Nullreference Exception in CardDavDataAccess.
	- Fix ForceBasicAuthentication checkbox in WPF UI.
	- Fix and simplify connection testing.
	- Delete contact photo in Outlook if it was deleted on the CardDav server.

#### 1.24.0 ####
- New features
	- Add general option to ignore invalid characters in server response.
	- Implement reordering of synchronization profiles in the WPF UI
- Bug fixes
	- Fix VALARM trigger handling if duration is zero, ticket #253.
	- Fix display issues for reports #259.
	- Add missing check box in WPF EventMappingConfigurationView (Negate filter and sync all Appointments except this category).
	- Hide TimeRangeView for all folder types except Appointments and Tasks.
	- Fix proxy settings in new WPF UI.
	- Disable ConflictResolution for OneWay synchronization in WPF UI.

#### 1.23.0 ####
- New features
	- First implementation of a complete redesign of Synchronization Profiles GUI using WPF framework.
	- General option to switch between modern WPF and standard WinForms GUI.
	- Improve update handling and download README after installing new version.
- Bug fixes
	- Ignore invalid BYMONTHDAY values in recurrence rules, catch COMException and log it as warning.
	- Set HasTime for completed of vtodo to avoid VALUE=DATE for GMT timezone to be RFC compliant, ticket #247.
	- Improve exception Handling.
	- Ensure SynchronizationContext BEFORE invoking async methods, wrap all invocations of async methods with a try-catch, ticket #248.
	- Try to reconstruct master event and properly sync exceptions to Outlook, if server resource consists of recurrence exceptions only.

#### 1.22.0 ####
- New features
	- Add option to enable/disable mapping of recurring tasks in TaskMappingConfiguration to avoid problems with servers that don't support recurring tasks.
	- Add option in ContactMappingConfiguration to fix formatting of phone numbers when syncing from server to Outlook, so that Outlook can detect country and area code, feature request 34.
	- Use email address for vcard FN if fileas, name and company attributes of outlook contact are emtpy.
- Bug fixes
	- Fix VTIMETONE DTSTART generation for timezones with yearly floating DST rules like Jerusalem when syncing to Google, ticket #244. (Workaround for a bug in the DDay.iCal library)
	- Don't add filter category to Outlook categories if Negate filter is activated, ticket #245.

#### 1.21.0 ####
- New features
	- Implement option in network and proxy options to force basic authentication, needed for some servers where negotiation or digest auth are not working properly, fixes connection problems with OS X servers.
	- Add general option to enable/disable tray icon.
	- Improve debug logging.
- Bug fixes
	- Use NullOutlookAccountPasswordProvider if Outlook profile name is null, ticket #239.
	- Fix proxy support in Google tasklibrary and oauth requests, ticket #234.
	- Fix line breaks in vcard notes and street addresses to avoid \r.

#### 1.20.3 ####
- Bug fixes
	- Fix Outlook crash when opening synchronization profiles for Outlook 2007 (ticket #230,#231).

#### 1.20.0 ####
- New features
	- New implementation of partial sync, which triggers immediately after an item is created, changed or deleted in Outlook (with a 10 seconds delay), works also for contacts and tasks now.
	- Add option to use IMAP/Pop3 Password from Outlook Account associated with the folder, the password is fetched from the Windows registry entry of the Outlook profile.
	- Add checkbox to sync all Outlook appointments except a defined category (negates the category filter) in EventMappingConfiguration, feature request 30.
	- Use ComboBox with all available Outlook categories instead of TextBox to choose category filter.
	- Add account types for Fruux, Posteo, Yandex and GMX with predefined DAV Urls and add logos to the account select dialog.
- Bug fixes
	- Fix well-known URIs according to RFC, they contain no trailing slash, fixes autodisovery for fruux.
	- Avoid ArgumentNullException for GoogleTasks if tasklist is empty, ticket #229.
	- clear contact attributes in Outlook when attributes are removed on server, fixes some update mapping issues from server to outlook for CardDAV.

#### 1.19.0 ####
- New features
	- Add System TrayIcon with notifications of sync runs with errors and warnings and context menu.
	- Add Synchronitation Status with info about last sync run time and status, accessible from the TrayIcon or the ribbon.
	- Add TaskMappingConfiguration with possibility to toggle reminder, priority and body mapping.
- Bug fixes
	- Catch COMException when accessing IsInstantSearchEnabled of Outlook store, ticket #223.
	- Fix Error, when opening legacy profiles without proxy options, ticket #224.

#### 1.18.0 ####
- New features
	- Deactivate prefix filter for custom message_classes by default and make it configurable as general option, since Windows Search Service was needed and not available in all setups.
	- Add manual "Check for Update" button in about box.
- Bug fixes
	- proper check for IsInstantSearchEnabled for the store when using prefix filter.
	- Remove unused DisplayAllProfilesAsGeneric general option.
	- Remove unneeded enableTaskSynchronization in app.config.
	- Change mapping errors to warnings for logging.

#### 1.17.0 ####
- New features
	- Improved formatted view for sync reports with possibility to view Outlook and server entities causing mapping warnings or errors.
	- Improve UI. Rename Advanced Options to Network and proxy options and move button to server settings. Move Mapping Configuration button from advanced options form to main profile configuration form.
	- Use prefix comparison in Outlook Repositories to filter also custom message_classes.
- Bug fixes
	- Fix test settings and don't allow an Outlook task folder for google calendar but only for a google tasklist.
	- Change BackColor of all UI forms and textboxes to SystemColors.Window.
	- Fix wordrap in changelog textbox of update window and make window resizable, feature request 24.
	- Use empty password, if decrypting password fails, ticket #165.

#### 1.16.0 ####
- New features
	- Google task support added. You can sync google tasklists to Outlook task folders via the Google Task Api. Just use a google profile and choose a task folder and do autodisocery to select the google tasklist.
	- Improved UpdateChecker, add button to automatically download and extract the new version and start the installer.
	- Improved Synchronisation reports with formatted view.
	- Small UI improvements and layout changes.
	- Add Link to Helppage and Wiki in About Dialog.
	- Improve Autodiscovery in google profiles and add button to start a new autodiscovery.
- Bug fixes
	- fix workaround for Synology NAS empty collections wrongly returning 404 NotFound, ticket #203.
	- Perform delete and create new , if an update for a Google-event returns HTTP-error 403, ticket #205.
	- Fix logging of server resource uri in sync reports.
	- Fix wrong handling of folder in OutlookTaskRepository.
	- Properly dispose WebClient in UpdateChecker and IsOnline check.

#### 1.15.0 ####
- WARNING: This version changes the internal cache structure, when downgrading to an older version, the cache gets cleared and a new inital sync is performed!
- New features
	- Improved handling of Uris, Use custom class WebResourceName instead of System.Uri to identify WebDAV resources. This should fix various issues with filenames with wrongly encoded special chars like slashes or spaces especially for Owncloud, see ticket #193 and discussions.
	- Add advanced option for preemptive authentication and set it to default for new profiles, feature request from ticket #198.
	- Make Options-Tabs draggable.
	- Delete caches if they have a version, other than the required version and implement cache conversion from version 0 to 1. 
	- Improve InitialTaskEntityMatcher and also compare Start and Due Date if available for matching tasks.
- Bug fixes
	- Set PatternEndDate of Recurrence to PatternStartDate if it is an invalid date before the start in the vevent to avoid COMException, ticket #197.
	- Don't set task completed in local timezone, COMPLETED of vtodo must be in UTC, fix regression introduced in 1.14.0.
	- Avoid UTC conversion in InitialEventEntityMatcher and use local timezone to avoid Nullreference Exceptions from Dday.iCal library in some strange timezone cases, ticket #154. Also fix matching of allday events and check if date matches.
	- Catch COMException when getting AddressEntryUserType of Recipient, ticket 109 from github.

#### 1.14.2 ####
- Bug fixes
	- Fix every workday recurrence and avoid INTERVAL=0 which is wrongly set from Outlook Object Model, fixes problem with certain versions of SabreDAV/OwnCloud, where INTERVAL=0 leads to an internal server error
	- Catch also possible ArgumentException in  MapRecurrence1To2 when trying to get AppointmentItem of a changed occurence.
	- Improve handling of DECLINED and cancelled meetings.
	- Prefix AppointmentItem Subject with "Cancelled: " if event status is CANCELLED
	- Use extension .vcf instead of .vcs for newly created vcards.
	- Improve mapping of phonenumbers, map Outlook OtherPhoneNumber and OtherFaxNumber and set TYPE=MAIN for PrimaryPhoneNumber.
	- Improve mapping of HOME and WORK URLs for vcards.
	- Refactor IsOnline() check to avoid problems in proxy environments, ticket #189

#### 1.14.0 ####
- New features
	- Skip sync runs, if network is not available to avoid error reports in that case, add general option to check Internet connection with dns query to www.google.com. If you are in a local network without dns or google.com blocked, disable this option.
	- Implement EventMappingConfiguration options for syncing private flag to CLASS:CONFIDENTIAL and vice versa, feature request 15.
- Bug fixes
	- Fix mapping outlook task dates to DTSTART and DUE, use local timezone and time 00:00:00 for start, 23:59:59 for due values and remove DURATION to be RFC 5545 compliant, see ticket #170. Use also localtime for COMPLETED instead of UTC to be consistent and fix VTIMEZONE DST rules for tasks.
	- Fix yearly recurrence with interval=1 for tasks.
	- Treat not recognized PARTSTAT same way as NEEDS-ACTION according to RFC 5545.
	- Fix mapping of attendees with type resource/room or unknown role, map X-LOCATION to type resource, set CUTYPE=RESOURCE for resources.
	- Catch COMException when setting recurrence interval and ignore invalid intervals for Appointments and Tasks, ticket #174.
	- Fix logging uid of events for recurrence errors.
	- Avoid COMException for invalid organizer in MapAttendeesAndOrganizer2To1, skip organizer if no email and no CN is valid.
	- Replace year 0001 with 1970 in VTIMEZONE definitions before deserializing icaldata, since DDay.iCal is extremely slow otherwise, needed for emClient, see ticket #150.

#### 1.13.2 ####
- Bug fixes
	- Refactor SetOrganizer and GetMailUrl in EventEntityMapper to avoid Nullreference Exceptions and catch COMExceptions.
	- Catch COMExceptions when accessing timezone of AppointmentItem and fallback to UTC in that case.
	- Catch COMException when AppointmentItem of an Exception doesn't exist, ignore that exception then since we can't get the changes. This happens when the recurring event is not in the local Outlook timezone.
	- Set WordWrap in newFeaturesTextBox for better readability of new features, feature request 24.
	- Check for invalid DTEND of vevents and catch COMException when trying to set EndTime, use DTSTART in those cases.
	- Catch COMException in GetEventOrganizer(), fixes issues with OL2007.
	- Avoid possible NullReferenceExceptions in MapAttendeesandOrganizer2To1.
	- Catch possible COMException in MapOrganizer1To2.
	- Catch UriFormatException also in Map2To1 when the server sends invalid attendee email adresses, ticket #168.

#### 1.13.0 ####
- New features
	- Support for GMX calendar, new events need to be created in UTC see section GMX in README.
	- Implement  Show/Clear Log and log level configuration in General Options (feature 22).
	- Add also 1 min and 2 min to avaiable Sync Intervals since requested multiple times.
	- Add option to disable mapping of contact photos in ContactMappingConfiguration, since it is not working properly in OL 2007.
- Bug fixes
	- EnsureSynchronizationContext on callbacks from Outlook, fixes errors when showing synchronization reports when synchronizung items immediately after changes.
	- Do not perform empty queries to repositories, fixes HTTP 400 errors with GMX.
	- Use PR_MESSAGE_CLASS to filter only AppointmentItems/TaskItems in OutlookRepositories, should fix casting errors when other items are in the folder.
	- Fix button layout in MappingConfiguration and OptionsDisplay.
	- Add EventMappingConfiguration to create events in UTC, needed for GMX for example, since local Windows Timezone leads to HTTP 403 error, ticket #162.
	- Catch System.UnauthorizedAccessExceptions in ContactEntityMapper.
	- Execute startup code just once, should fix error in ticket #161.
	- Avoid NullreferenceException when AdressEntry of recipient can't be fetched, ticket #163.

#### 1.12.0 ####
- New features
	- Match added entities with every sync run, this should avoid duplicates and errors, when same event is added in both server and client e.g. a (autoaccepted) meeting invitation.
	- Add "Reset Cache" button to delete the sync cache and start a new initial sync with the next sync run.
	- Delete associated birthday appointment if deleting ContactItem in Outlook, feature #21.
	- Cleanup outdated synchronization reports with configurable timespan.
- Bug fixes
	- Fix issues which might occur due to load behavior of controls.
	- Fix exporting of DateCompleted for tasks, according to the RFC it must be a DATE-TIME value, see ticket #156
	- Convert DateCompleted for tasks from UTC to local date when mapping back to Outlook.
	- Add Reports to ToolBar Buttons for OL2007.
	- Update Meeting Response only if MapAttendees is set in MappingConfiguration.
	- Fix yearly recurrence with interval=1, patch provided by Jonathan Westerholt, ticket #159
	- Revert "Use GlobalAppointmentID for new events instead of random Guid to avoid doubling events from invitations for own attendee". This caused problems with Google when recreating deleted events with same UID.
	- Cleanup outdated synchronization reports.
	- Add context menu which allows to open the cache directory also to Google profile type.
	- Select the new tab in OptionsForm when a new profile is added.

#### 1.11.0 ####
- New features
	- Advanced Logging and configurable Synchronization Reports after each sync run. You can configure if reports should be generated for each sync run or only if errors or warnings occur and if the reports should be shown immediately after the sync run. You can also delete or zip reports from the Reports window.
	- Support for Zoho Calendar, patch provided from Suki Hirata <thirata@outlook.com>
- Bug fixes
	- Factor out common mapping functions for events and tasks and map priority 1-9 according to RFC5545 

#### 1.10.0 ####
- New features
	- Add possibility to set server calendar color to selected category color
	- Allow to specify shortcut key of category and improve EventMappingConfiguration UI
	- Add scheduling configuration options to EventMappingConfiguration and set RSVP for attendees. (You can specify if you want to set SCHEDULE-AGENT:CLIENT or X-SOGO-SEND-APPOINTMENT-NOTIFICATIONS:NO for SOGo)
- bug fixes
	- Escape single quotes in category filter string and validate it in EventMappingConfiguration, it must not contain commas or semicolons to avoid exceptions
	- Use DASL filter instead of JET syntax to fix category filtering for OL 2010(64bit)
	- Take relative redirects in account. (fixes Autodiscovery for some servers based on cpanel/horde)
	- Avoid UTC conversion from Dday.iCal library for upcoming reminder check.
	- Use GlobalAppointmentID for new events instead of random Guid to avoid doubling events from invitations for own attendee.
#### 1.9.0 ####
- New features
	- Map CalDAV server colors to Outlook category colors. It is possible to choose the  category color manually or fetch the color from the server and map it to the nearest supported Outlook color.
- bug fixes
	- Don't use environment specific newline, in data sent to the server
	- Escape Uris, which are inserted into XML documents
	- Remove unused calDavReadWriteTimeout from config

#### 1.8.0 ####
- New features
	- Add filtering on outlook side, so that multiple CalDAV-Calendars can be synchronized into one Outlook calendar via an Outlook category
	- Add mapping configuration options for Contacts (Enable or Disable mapping of Birthdays, feature #12)
	- Provide entity version (etag) on delete and set If-Match header
	- Add option to synchronize just upcoming reminders.
	- Autodiscovery improvemnts: Ignore xml Exceptions during Autodiscovery (needed for some wrong owncloud server paths)  and try hostname without path too if well-known not available, fixes autodiscovery for posteo (https://posteo.de:8443), Display if no resources were found via well-known URLs
- bug fixes
	- Filter out SOGo vlists (contenttype text/x-vlist) since we can't parse them atm, avoids syncing vlists to a empty vcard and destroying the vlist when syncing back to SOGo
	- Trim category names for events,tasks and contacts when mapping to caldav
	- Use ENCODING=b instead of BASE64 according to vcard 3.0 spec for binary attributes

#### 1.7.0 ####
- New features
	- GUI redesign for Google profiles to simplify setup and autodiscovery for google accounts. When creating a new sync profile you can choose between a generic CalDAV/CardDAV and a google profile. For google it is sufficient to enter the Email address and autodiscovery will try to find resources once OAuth is configured.
	- Improvements in autodisovery logic
	- Calendar Colors are now shown in Autodiscovery SelectResourceForm, syncing colors to Outlook categories is work in progress.
	- Add group label to Ribbon and set ScreenTips and SuperTips
- bug fixes
	- Clear ol phonenumbers before updating, fixes doubling of home and work numbers, ticket #142
	- Change TabIndex ordering, ticket #140
	- Delete profile cache also when username is changed, helps when google id is changed , ticket #141
	- Delete profile cache also when time range filter is modified or deactivated, ticket #138
	- Fix calDavDateTimeFormatString and use today instead of now to filter timerange
	- Add missing quotes to etags on system boundaries.

#### 1.6.0 ####
- New features:
	- Provide entity version (etag) on update and set If-Match header
	- Implement own vCardImprovedWriter to fix serialization problems of vCardStandardWriter and avoid costly Regex workarounds 
	- Add TYPE=HOME for personal homepages
- bug fixes:
	- Fix mapping of HomeFaxNumber for vcards, ticket #134
	- Log Exceptions during ConnectionTests and don't try to list calendars or adressbooks for empty homesets, fixes github issue #82
	- Fix GetContacts for Yandex, since Yandex returns directory itself even with an etag
	- Improve error handling
	- Fixes TYPE subproperties needed for Yandex vcards
	- Ensure that Etag is double quoted when adding in Entity repositories, since some caldav servers like yandex send etags without quotes
	
#### 1.5.4 ####
- New features:
	- General options in GUI for changing SSL options and other global settings
	- Add option to store state information in Appdata\Roaming instead of Local, Ticket #125
	- Add mapping configuration options for Appointments (Enable or Disable mapping of reminders, attendees or the description body)
	- Add Donate link to About Dialog
	- Add context menu to options , which allows to open the cache directory
- bug fixes:
	- Fix mapping of ORG property to CompanyName and Department for vcards, ticket #127
	- Catch COM Exceptions when trying to add invalid Outlook items in repositories, ticket #130
	- Fix for unescaping relative urls for entity ids, ticket #129

#### 1.5.3 ####
- Avoid Nullreference Exception which prevents syncing when there are no proxy settings in config file, bug #124
- set correct mime type text/vcard when putting contacts

#### 1.5.2 ####
- Delete profile cache, if outlook-folder or caldav-server-url is changed, ticket #117, prevents data loss and forces new inital sync in such cases
- Fix for linebreak issues of OpenX-change, merged from pull request #79, thx to bjoernbusch

#### 1.5.1 ####
- New features:
	- Support for proxy configuration in GUI to specify manual proxy settings and allow Basic Auth and NTLM proxies
- bug fixes:
	- Use ContactItemWrapper and reload Items to avoid a second sync with a changed modification time in Outlook, see ticket #111
	- Avoid sending meeting response if meeting is self organized
	- Avoid unnecessary connection tests during autodiscovery, fixes Google CardDAV autodiscovery

#### 1.5.0 ####
- New features:
	- Autodiscovery for CardDAV addressbooks
	- Change-triggered partial sync (Synchronize appointment items immediately after change in Outlook)
	- Support Yandex CalDAV server
	- Many improvements for CardDAV
	- Add OAuth Scope for Google CardDAV
- bug fixes:
	- Fix syncing contact notes with umlauts
	- Disable TimeRangeFiltering when contact folder is chosen
	- Ensure that FN of vcard is not empty, since it is a MUST attribute (bug #109)
	- Map AccessClassification of vcards
	- Ensure that vcard UID is not empty also in Updates
	- Skip addressbook collection itself when fetching vcards from Owncloud
	- Use existing UID for filename in PUT requests
	- Get ETAG via propfind if it is not returned in header to avoid Null Reference
	- Check also for Write privilege when detecting calendar access rights
	- Map other vcard telephonenumber types
	- Honor RevisionDate in vcard Updates if available (bug #111)
	- Fix InitialEventEntityMatcher if DTEnd is null (bug #110)
	- Filter only ContactItems in ContactRepository GetTable to avoid COM Exceptions, since we don't sync groups or distribution lists at the moment
	- Avoid exception in Autodiscovery DoubleClick EventHandler when clicking the header

#### 1.4.5 ####
- Fix regex in workaround for DDay.iCal timezone parsing, should fix bug #105 for syncing with Owncloud/Davdroid
- Rework of exdate generation to work around some strange Outlook Exception Collection missing elements, fixes Bug #91 and other recurrence exceptions with complex patterns

#### 1.4.4 ####
- Another fix for VTIMEZONE definition for Google, hopefully fixes US and Moscow timezone
- Add UID when vCard is created, according to the RFC UID is mandatory
- Another fix for recurrence exceptions and exdates for Bug #101
- Handle exceptions when updating outlook folders or during profile loading at startup

#### 1.4.3 ####
- Another DDay.iCal Workaround to fix VTIMEZONE generation for timezones wih changing DST rules like Moscow or Cairo
- bugfix: Use timezone ID for comparison, avoid exporting double VTIMEZONE definitons
- Options for not using keepalive and accepting invalid headers added

#### 1.4.2 ####
- Use StartTimeZone and EndTimeZone of events if different to system timezone
- Map server timezones to Windows Timezones and set time in StartTimeZone to fix recurring events which span DST shifts (fixes bug #94)
- Fix many cases of mapping recurrence exceptions and finding them over DST changes or if Outlook and Server are in different timezones
- Fix for bug #101, wrong exdate calculation
- Fix originalDate calculation of recurrence exceptions if they are on previous day in UTC
- Don't export historical timezone data before 1970

#### 1.4.1 ####
- Add mapping of IMAddress for contacts
- Add mapping of contact notes
- Fix vcard mapping for fax numbers and address type other
- Add doubleclick eventhandler for Autodiscovery
- Fixed TestConnection behavior, so that cancelling Autodiscovery works
- Add mapping of contact photos
- Mapping of FormattedName for contacts
- Add mapping for X509 certificates for contacts (e.g. S/MIME) to vCard KEY attribute
- Implement EmailAddress Mapping for Exchange contacts (type "EX")

#### 1.4.0 ####
- Initial CardDAV support to sync contacts (alpha)
- Refactoring of Autodiscovery
- Fix options and about buttons for Outlook 2007

#### 1.3.4 ####
- Add support for Outlook 2007, credits to PierreMarieBaty (pull request #67)
- Refactoring of Autodiscovery
- Refactoring of url validation and test settings
- Added option to automatically fix synchronization settings.
- Avoid ArgumentOutOfRangeException in attendee email substring

#### 1.3.3 ####
- Fix reminder timespan value

#### 1.3.2 ####
- Fix reminders for google
- Clarify test settings info for read-only resources and set Synchronization mode

#### 1.3.1 ####
- Add guard to prevent that a SynchronizationWorker is running multiple times.
- Fix priority mapping for tasks.
- Initial implementation of recurring tasks.

#### 1.3.0 ####
- Add support for Autodiscovery of CalDAV urls
- Workaround: Since DDay.iCal is not capable to parse events, which contain unsorted TimeZoneComponents, they must be sorted before parsing.

#### 1.2.2 ####
- Fixed bug in InitialEventEntityMatcher which caused a duplication of events, when a profile was deleted and recreated.
- Catch UriFormatExceptions in attendee and organizer Values from CalDav.
- Include response message in exception, if a protocol error occurs.

#### 1.2.1 ####
- Fixed HttpClient redirect issue, which affected Zimbra integration

#### 1.2.0 ####
- Added option to ignore new version and wait for next update.
- Added workaround for Group Office, which will tolerate empty VALARMs.
- Disable SynchronizeNowButton during synchronization.
- Disable TestConnectionButton while test is in progress.
- Add proper disposing for web messages.

#### 1.1.0 ####
- Support for Google OAuth
- Perform all web operations in the Background

#### 1.0.4 ####
-Fix TimeRange filter for events

#### 1.0.3 ####
- Add feature, which checks if a newer version is available (Ticket 61)

#### 1.0.2 ####
- Bugfix: Preserve UID when updating an Event.

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

## User Documentation ##

After installing the plugin, a new ribbon called 'Caldav Synchronizer' is added in Outlook with 6 menu items. 
- Synchronize now
- Synchronization Profiles
- General Options
- About
- Reports
- Status

Use the Synchronization Profiles dialog to configure different synchronization profiles. Each profile is responsible for synchronizing one Outlook calendar/task or contact folder with a remote folder of a CalDAV/CardDAV server.
The toolbar on the left upper part provides the following options: 

- **Add new profile** adds a new empty profile
- **Add multiple profiles** bulk profile creation to add multiple profiles at once and choose the folder for each discovered server resource (calendar, addressbook and task)
- **Delete selected profile** deletes the current profile
- **Copy selected profile** copies the current profile to a new one
- **Move selected profile up** change ordering in the tree view
- **Move selected profile down** change ordering in the tree view
- **Open data directory of selected profile** Show directory with cached relations file in explorer for debugging
- **Expand all nodes** expand all nodes in the tree view, enabled by default but can be changed in general options
- **Collapse all nodes** collapse all nodes in the tree view
- **Export Profiles to File** and 
- **Import Profiles from File** See Profile Import/Export

When adding a new profile you can choose between a generic CalDAV/CardDAV, a google profile to simplify the google profile creation and predefined CalDAV/CardDAV profiles for SOGo, Fruux, Posteo, Yandex, GMX, Sarenet and Landmarks where the DAV Url for autodiscovery is already entered. 

The following properties need to be set for a new generic profile:

- *Profile name*: An arbitrary name for the profile, which will be displayed in the tree view.
- - *Outlook settings*:
	- **Outlook Folder:** Outlook folder that should be used for synchronization. You can choose a calendar, contact or task folder. Depending on the folder type, the matching server resource type in the server settings must be used.
	- **Synchronize items immediately after change** Trigger a partial synchronization run immediately after an item is created, changed or deleted in Outlook (with a 10 seconds delay).
- *Server settings*:
	- **DAV Url:** URL of the remote CalDAV or CardDAV server. You should use a HTTPS connection here for security reason! The Url must end with a **/** e.g. **https://myserver.com/** 
	- If you only have a self signed certificate, add the self signed cert to the Local Computer Trusted Root Certification Authorities. You can import the cert by running the MMC as Administrator. If that fails, see section *'Advanced options'*
	- **Username:** Username to connect to the CalDAV server
	- **Password:** Password used for the connection. The password will be saved encrypted in the option config file.
	- ** Use IMAP/POP3 Account Password** Instead of entering the password you can use the IMAP/Pop3 Password from the Outlook Account associated with the folder, the password is fetched from the Windows registry entry of the Outlook profile. 
	- **Email address:** email address used as remote identity for the CalDAV server, necessary to synchronize the organizer. The email address can also be used for autodiscovery via DNS lookups, see section Autodiscovery.

- *Sync settings*:
	- Synchronization settings
		- **Outlook -> Server (Replicate):** syncronizes everything from Outlook to the server (one way)
		- **Outlook <- Server (Replicate):** synchronizes everything from the server to Outlook (one way)
		- **Outlook -> Server (Merge):** synchronizes everything from Outlook to the server but don't change events created in on the server
		- **Outlook <- Server (Merge):** synchronizes everything from the server to Outlook but don't change events created in Outlook
		- **Outlook <-> Server (Two-Way):** Two-Way synchronization between Outlook and the server with one of the following conflict resolution
	- Conflict resolution (only used in Two-Way synchronization mode)
		- **Outlook Wins:** If an event is modified in Outlook and in the server since last snyc, use the Outlook version. If an event is modified in Outlook and deleted in the server since last snyc, also use the Outlook version. If an event is deleted in Outlook and modified in the server, also delete it in the server.
		- **Server Wins:** If an event is modified in Outlook and in the server since last snyc, use the server version. If an event is modified in Outlook and deleted in the server since last snyc, also delete it in Outlook. If an event is deleted in Outlook and modified in the server, recreate it in Outlook.
		- **Automatic:** If event is modified in Outlook and in the server since last snyc, use the last recent modified version. If an event is modified in Outlook and deleted in the server since last snyc, delete it also in Outlook. If an event is deleted in Outlook and modified in the server, also delete it in the server
	- **Synchronization interval (minutes):** Choose the interval for synchronization in minutes, if 'Manual only' is choosen, there is no automatic sync but you can use the 'Synchronize now' menu item.
	- **Perform synchronization in chunks** and
	- **Chunk size** perform CalDAV/CardDAV sync in chunks with configurable chunk size to avoid OutOfMemoryEceptions, disabled by default and should only be enabled for huge resources and low memory.
	- **Use time range filter** and
	- **Synchronization timespan past (days)** and
	- **Synchronization timespan future (days)** For performance reasons it is useful to sync only a given timespan of a big calendar, especially past events are normally not necessary to sync after a given timespan.

- **Clear cache** delete the sync cache and start a new initial sync with the next sync run.
- **Is active checkbox in the tree view** If deactivated, current profile is not synced anymore without the need to delete the profile.

If you expand the tree view of the profile you can configure network and proxy options and mapping configuration options.

- **Network and proxy options**: Here you can configure advanced network options and proxy settings. 
	- **Close connection after each request** Don't use KeepAlive for servers which don't support it. 
	- **Use Preemptive Authentication** Send Authentication header with each request to avoid 401 responses and resending the request, disable only if the server has problems with preemptive authentication.
	- **Force basic authentication** Set basic authentication headers to avoid problems with negotiation or digest authentication with servers like OS X. This is only recommended if you use a secure HTTPS connection, otherwise passwords are sent in cleartext.
	- **Use System Default Proxy** Use proxy settings from Internet Explorer or config file, uses default credentials if available for NTLM authentication.
	- **Use manual proxy configuration** Specify proxy URL as `http://<your-proxy-domain>:<your-proxy-port>` and optional Username and Password for Basic Authentication.
	
- **Mapping Configuration**: Here you can configure what properties should be synced.
	- For appointments you can choose if you want to map reminders (just upcoming, all or none) and the description body.
	- *Timezone settings* See section Timezone mapping below.
	- *Use GlobalAppointmentID for UID attribute:* Use Outlook GlobalAppointmendID instead of random Guid for UID attribute in new CalDAV events. This can avoid duplicate events from invitations.
	- In *Privacy settings* you can configure if you want to map Outlook private appointments to CLASS:CONFIDENTIAL and vice versa. This could be useful for Owncloud for example, if you share your calendar with others and they should see start/end dates of your private appointments. You can also map all CLASS:PUBLIC events to Outlook private appointments.
	- In *Scheduling settings* you can configure if you want to map attendees and organizer and if notifications should be sent by the server. 
	- Use *Don't send appointment notifications for SOGo servers and SCHEDULE-AGENT=CLIENT for other servers if you want to send invitations from Outlook and avoid that the server sends invitations too, but be aware that not all servers (e.g. Google) support the SCHEDULE-AGENT=CLIENT setting. 
	- In *Outlook settings* you can also define a filter category so that multiple CalDAV-Calendars can be synchronized into one Outlook calendar via the defined category (see Category Filter and Color below).
	- *Cleanup duplicate events after each sync run:* removes duplicate Outlook appointments based on start,end and subject of the events after each sync run, be aware of possible performance penalties with this option enabled.
	- For contacts you can configure if birthdays should be mapped or not. If birthdays are mapped, Outlook also creates an recurring appointment for every contact with a defined birthday.
	- You can also configure if contact photos should be mapped or not. Contact photo mapping from Outlook to the server doesn't work in Outlook 2007. You can also add an option to not overwrite the contact photo in Outlook when it changes on the server, which could happen due to other mobile clients reducing the resolution for example.
	- Don't overwrite FileAs in Outlook uses the Outlook settings for FileAs and doesn't overwrite the contact FileAs with the FN from the server.
	- Fix imported phone number format adds round brackets to the area code of phone numbers, so that Outlook can show correct phone number details with country and area code, e.g. +1 23 45678 is mapped to +1 (23) 45678.
	- For tasks (not for Google task profiles) you can configure if you want to map reminders (just upcoming, all or none), the priority of the task, the description body and if recurring tasks should be synchronized.
	- Similar to calendars you can also define a filter category so that multiple CalDAV Tasklists can be synchronized into one Outlook task folder via the defined category.	
### Timezone settings ###

Outlook and Windows use different Timezone definitions than most CalDAV servers and other clients. When adding new events on the server you have different options how the timezone of the newly created VEVENT is generated. The default setting uses the default Windows Timezone from Outlook (e.g. W. Europe Standard Time) or the selected timezones for the start and end of the appointment. Since some servers have problems with that timezone definitions you can change that behaviour in the event mapping configuration with the following options:

- *Create events on server in UTC* Use UTC instead of Outlook Appointment Timezone for creating events on CalDAV server. Not recommended for general use, because recurrence exceptions over DST changes can't be mapped and Appointments with different start and end timezones can't be represented.
- *Create events on server in downloaded IANA Timezones* Use Iana instead of Windows Timezones for creating events on CalDAV server. Needed for servers which do not accept non standard Windows Timezones like GMX for example. Timezone definitions will be downloaded from [http://tzurl.org.](http://tzurl.org)
- *Use IANA Timezone* Use this IANA timezone for default Outlook/Windows timezone. Manually selected different timezones in Outlook appointments will be mapped to first corresponding IANA timezone.
- *Include full IANA zone with historical data* Use full IANA timezone definition with historical data. Needs more bandwith and can be incompatible when manually importing in Outlook.

### Managing meetings and invites ###

Outlook can only track meeting responses and invites in the main calender folder. If you schedule meetings from Outlook which are synced with the CalDAV server you have two possibilities to avoid double invitation mails for all attendees. First, you can enable the option *SCHEDULE-AGENT=CLIENT* (or *Don't send appointment notifications (from SOGo)*" for SOGo servers) and let only Outlook send the meeting invites, if the server supports this option. Or you can disable this option and let the server schedule the meetings after syncing the meeting. Then you need to disable the invitation mails sent from Outlook. This is possible by unchecking the checkbox left to the attendee name in the meeting planning dialog. When syncing meetings created in Outlook to the server, the option *Use GlobalAppointmentID for UID attribute* is recommended. This can avoid duplicate events from invitations.

The response status of all attendees can be synced from Outlook to the server but only the status of the own Outlook identity (if included in the attendees) can be synced from the server to Outlook due to limitations of the Outlook Object Model.

When receiving invites from the CalDAV server and via Email in your INBOX, Outlook will automatically create a tentative meeting in the main calendar folder. 

To avoid double meetings the option *Cleanup duplicate events after each sync run* in event mapping configuration is recommended.

### Free/busy lookups ###

You can configure free/busy lookups globally in the outlook options.
Select Options/Calendar and there free/busy information and use a free/busy url of your server with placeholder like %Name%, e.g. http://myserver/freebusy.php/%Name%
Then ervery attendee in the outlook planning view gets resolved with that url for a free/busy lookup against your server. 

### Scheduling settings and resources ###

If your server supports resources (for SOGo see [http://wiki.sogo.nu/ResourceConfiguration](http://wiki.sogo.nu/ResourceConfiguration))
disable "set SCHEDULE-AGENT=CLIENT" in Mapping Configuration, so that the server can handle the resource invitation mails, add the resource email adress as attendee in the Outlook appointment and choose type ressource (house icon) for it.
 
### Category Filter and Color ###

If you want to sync multiple CalDAV calendars or tasklists into one Outlook folder you can configure an Outlook category for filtering in the *Mapping Configuration*. You can choose a category from the dropdown list of all available Outlook categories or enter a new category name.
For all events/tasks from the server the defined category is added in Outlook, when syncing back from Outlook to the server only appointments/tasks with that category are considered but the filter category is removed. The category name must not contain any commas or semicolons!
With the checkbox below you can also negate the filter and sync all appointments/tasks except this category.
For calendars it is also possible to choose the color of the category or to fetch the calendar color from the server and map it to the nearest supported Outlook category color with the button *Fetch Color*. With *Set DAV Color* it is also possible to sync the choosen category color back to set the server calendar color accordingly. With *Category Shortcut Key* you can define the shortcut key of the selected category for easier access when creating appointments.

### Reminders ###

In event and task mapping configuration you can define if you want to map (all/non/just upcoming) reminders. If you get the following error message when trying to set reminders in Outlook 
> The reminder will not appear because the item is in a folder that doesn’t support reminders.

you can try to change the Outlook options as discussed in
[http://answers.microsoft.com/en-us/office/forum/office_2016-outlook/outlook-2016-calendar-reminders/8f40bcdd-e3fc-4f29-acaf-544f48d63992](http://answers.microsoft.com/en-us/office/forum/office_2016-outlook/outlook-2016-calendar-reminders/8f40bcdd-e3fc-4f29-acaf-544f48d63992)
or try the following reported by #Todo18


1. Create a new storage folder in Outlook via the File menu, Info, Account Settings. In the Data Files tab, you can Add a new (.pst) data file. After the file has been added, Make it the default [data file], and close the dialog.
2. Go to the Calendar window, right click on the calendar that's giving you problems, and select Move Calendar. In the dialog, pick the data file that you created in the first step, and confirm. Don't forget to update the storage folder in the CalDav Synchronizer settings!

### Custom properties mapping ###

When you expand the tree view of the profile for events and tasks, you can configure the mapping of custom properties.

- *Map all Outlook custom properties to X-CALDAVSYNCHRONIZER attributes* If enabled, all Outlook custom text properties of the appointment/task are mapped to DAV attributes with the prefix X-CALDAVSYNCHRONIZER- and vice versa.
- You can also define manual mapping pairs of Outlook custom attributes and DAV X-Attributes. This will overrule the general mapping of all Outlook custom properties if both is activated. Outlook properties that don't exist, will be created. DAV properties MUST start with X-. Only Outlook custom properties of type Text can be mapped.  

### Google Calender / Addressbooks / Tasks settings ###

For Google you can use the new Google type profile which simplifies the setup. You just need to enter the email address of your google account. When testing the settings, you will be redirected to your browser to enter your Google Account password and grant access rights to your Google Calender, Contacts and Tasks for OutlookCalDavSynchronizer via the safe OAuth protocol. After that Autodiscovery will try to find available calendar, addressbook and task resources. 

For contacts you can activate the checkbox **Use Google native API**. This should improve performance and other mapping issues, since the Google Contacts API supports more features than the generic CardDAV API. Compared to CardDAV this adds:

- Support for google contact groups, which are synced to Outlook categories.
- Added mapping for anniversary, relations (spouse, child, etc.) and IMs for google contacts (Contribution from Florian Saller, thank you!).

When switching betwwen native API and CardDAV the sync cache is cleared and a complete initial sync is performed during next sync run.

For tasks you can choose the tasklist you want to sync with an Outlook task folder and the id of the task list is shown in the Discovered Url. With the button 'Edit Url' you still can manually change the Url e.g. when you want to sync a shared google calendar from another account.

If you get an error with insufficient access you need to refresh the token by deleting the previous token in 
`C:\Users\<your Username>\AppData\Roaming\Google.Apis.Auth`

### GMX calendar settings ###

For GMX calendar use the GMX Calendar account type, which sets the autodiscovery DAV Url `https://caldav.gmx.net`
Since GMX doesn't allow to create events with the Windows Timezone IDs, for the GMX account type the `Create events on server with downloaded IANA Timezones` checkbox in Mapping Configuration is checked by default to avoid errors when creating events and syncing from Outlook to GMX.

For GMX addressbook use the DAV Url `https://carddav.gmx.net`

### Synology NAS settings ###

When test settings for your synology NAS profile, you can ignore the warning "The specified Url does not support calendar queries. Some features like time range filter may not work!".
But a user reported, that "Disable directory browsing" setting **must not** be enabled for the calendar folder for proper syncing.

For Synology NAS with SSL support use port 5006 and the following settings in your NAS:
In Synology DSM Navigate to control panel > Terminal & SNMP
Select Enable SSH 
Then enter Advanced Settings and set it to High
Now it will work on port 5006 with https.

### iCloud settings ###

To find the correct DAV url for iCloud you need some Informations from the MacOS, where you are connected with your calendar.

Open with Textedit: `~/Library/Calendars/*.caldav/Info.plist` 
(Its in the hidden User-Library)

Check iCloud Path: PrincipalURL 
    `<string>https://p**-caldav.icloud.com/*********/principal/</string>`

Check: DefaultCalendarPath 
    `<string>/*********/calendars/********-****-****-****-************</string>`

Then you get the DAV url of the calendar:
    `https://p**-caldav.icloud.com/*********/calendars/********-****-****-****-************/`

For syncing iCloud contacts with CardDAV use the following URL
    https://contacts.icloud.com
and press '*Test or discover settings*' for autodiscovery, the final URL should look like
    
    https://contacts.icloud.com:443/<YOUR UNIQUE Apple USER_ID>/carddavhome/card/

There are PHP files available to determine your Apple USER_ID, see

    https://icloud.niftyside.com/

    https://github.com/muhlba91/icloud

### One.com settings ###

The one.com caldav server has problems with escaping, so if your calendar url looks something like

    https://caldav.one.com/calendars/users/USERNAME@DOMAIN.COM/calendar/
use the url

    https://caldav.one.com/calendars/users/USERNAME%40DOMAIN.COM/calendar/

### Autodiscovery ###

When you are using an IMAP/POP3 Account with the same server settings (Username, Email address) you can press *Get IMAP/POP3 account settings* to discover those settings. The DAV url is discovered via DNS lookup from the account email address or the IMAP/POP3/SMTP server url if that fails. Together with the *Use IMAP/POP3 account password* checkbox activated you can fully autoconfigure the server settings from your existing account.

Instead of using the exact calendar/addressbook URL you can use the server address or the principal url and use the 'Test  or discover settings' button in the option dialog to try to autodiscover available calendars and addressbooks on the server. You can  then choose one of the found calendars or addressbooks in the new window.
If your server has redirections for well-known Urls (`./well-known/caldav/` and `./well-known/carddav/` ) you need to enter the server name only (without path). If your domain configured DNS SRV and/or TXT lookups it is also possible leave the DAV url empty and discover it from the entered Email Address via DNS lookups, for example:

    _carddavs._tcp 86400 IN SRV 10 20 443 dav.example.org.
    _caldavs._tcp 86400 IN SRV 10 20 443 dav.example.org.

### Proxy Settings ###
You can now set manual proxy settings in the *Network and proxy options* dialog in each profile. To override the default proxy settings from Windows Internet Explorer you can also specify settings in the app config file, see config options below.
More information can be found at
[https://msdn.microsoft.com/en-us/library/sa91de1e%28v=vs.110%29.aspx](https://msdn.microsoft.com/en-us/library/sa91de1e%28v=vs.110%29.aspx)

### General Options and SSL settings ###
In the General Options Dialog you can change settings which are used for all synchronization profiles.

- **Automatically check for newer versions** set to false to disable checking for updates.
- **Check Internet connection before sync run** checks if an interface is up and try DNS query to dns.msftncsi.com first and if that fails try to download http://www.msftncsi.com/ncsi.txt with the configured proxy before each sync run to avoid error reports if network is unavailable after hibernate for example. Disable this option if you are in a local network where DNS and that URL is blocked.
- **Store data in roaming folder** set to true if you need to store state and profile data in the AppData\Roaming\ directory for roaming profiles in a AD domain for example. When changing this option, a restart of Outlook is required.
- **Fix invalid settings** Fixes invalid settings automatically, when synchronization profiles are edited.
- **Include custom message classes in Outlook filter** Disabled by default, enable only if you have custom forms with message_classes other than the default IPM.Appointment/Contact/Task. For better performance, Windows Search Service shouldn't be deactivated if this option is enabled.
- **Enable Tray Icon** Enabled by default, you can disable the tray icon in the Windows Taskbar if you don't need it.
- **Accept invalid chars in server response** If checked invalid characters in XML server responses are allowed. A typical invalid char, sent by some servers is Form feed (0x0C).
- ** Enable useUnsafeHeaderParsing** Enable, if the server sends invalid http headers, see common network errors. Needed for Yahoo and cPanel Horde servers for example. The general option overrides the setting in the app.config file.
- **Trigger sync after Outlook Send/Receive and on Startup** If checked a manual sync is triggered after the Outlook Send/Receive finishes and on Outlook startup.
- **CalDav Connection Timeout (secs)** For slow server connections you can increaste the timeout value (default 90 secs).
- **Expand all nodes in Synchronization profiles** Enabled by default, expands all nodes in the synchronization profiles to see the suboptions for network settings and mapping configuration.

If you have problems with SSL/TLS and self-signed certificates, you can change the following settings at your own risk.
The recommended way would be to add the self signed cert to the Local Computer Trusted Root Certification Authorities
You can import the cert by running the MMC as Administrator.

- **Disable Certificate Validation** set to true to disable SSL/TLS certificate validation, major security risk, use with caution!
- **Enable Tls12** set to false to disable TLS12, not recommended 
- **Enable Ssl3** set to true to enable deprecated SSLv3, major security risk, use with caution! 

In the **General Logging** section you can show or clear the log file and define the log level. Possible log levels are `INFO` and  `DEBUG`.

### Profile Import/Export ###

In the toolbar of the synchronization profiles you can export all profiles to a file and import profiles from an earlier exported file. When exporting, you can choose a filename, the extension is *.cdsp and all options are saved in an xml format into this file. When importing the file, existing profiles are merged with the imported ones. If the selected Outlook folder for the profile doesn't exist during import, you need to manually select a folder before you can save the options, they are not automatically created. You need also be aware of the fact, that saved profile passwords won't work on other accounts or machines, since the encryption is dependant on the current user. But you can use the account password from the IMAP/POP3 account if available. General options are not saved in that file, but in the registry in `HKEY_CURRENT_USER\Software\CalDavSynchronizer`.

### Reports of sync runs ###

You can also configure Synchronization reports for all profiles, this can be configured via general Options:

- **Log** You can choose if you want to generate reports for *"Only sync runs with errors"* or *"Sync runs with errors or warnings"* or *"All sync runs"*.
- **Show immediately** configures if the Sync reports should be shown immediately after a sync run with errors, with warnings or errors, or not at all.
- **Delete reports older than (days)** Automatically delete reports which are older than the days configured.

You can show reports manually with the **Reports** button in the CalDav Synchronizer Ribbon. There you can choose from available reports (shown as profile name with timestamp of the sync run) and see informations about items synced and if there were any warnings or errors. You can also delete reports or add them to a zip file via the context menu. If the last sync run lead to any errors, a warning symbol is shown in the Ribbon or the Report window opens if configured in the general options.

### Synchronization Status and System TrayIcon with Notifications ###

With the **Status** button in the CalDav Synchronizer Ribbon or via doubleclick from the TrayIcon you can access the status of the active sync profiles with their last sync run shown in minutes ago and the status OK, error, or warning. When clicking on the profile name you get to the according sync profile settings, when clicking the status icon, you can open the according sync report. When a sync run has any errors or warnings you will get a notification from the CalDav Synchronizer TrayIcon.

## Trouble Shooting ##

Options and state information is normally stored in the following folder:

    C:\Users\<Your Username>\AppData\Local\CalDavSychronizer
If you activated Store data in roaming folder the location is changed to the following folder:

    C:\Users\<Your Username>\AppData\Roaming\CalDavSychronizer

There is one `options_<your outlook profile>.xml` file which stores the options for each outlook profile.
For each sync profile there is a subfolder with state information stored in a relations.xml file after the inital sync. If you delete that folder, a fresh inital sync is performed. In the Synchronization profiles dialog a context menu is available in each profile (right click), which allows to open the cache directory and read the relations.xml file.

Each synchronization attempt is logged in the `log.txt` file. There you can find information about sync duration and the amount of added, deleted or modified events. Errors and Exceptions are logged aswell. You can view and clear the log file in **General Options**. There you can also change the log level from `INFO` to `DEBUG`. 

 
### Debugging and more config options ###

In the install dir (The default is `'C:\Program Files (x86)\Gerhard Zehetbauer\CalDavSynchronizer'`) you will find the app config file

    CalDavSynchronizer.dll.config

In that xml file you can config timeout parameters and config options in the section `appSettings`
After changing parameters you have to restart Outlook.

- **loadOperationThresholdForProgressDisplay**: amount of sync operations to show the progress bar (default 50)
- **calDavConnectTimeout**: timeout for caldav connects (default 90 sec), also possible via general option.
- **enableTaskSynchronization** Support for task sync true or false

You can also change defaults for some of the general options like CheckForNewVersions, StoreAppDatainRoamingFolder, IncludeCustomMessageClasses and SSL/TLS options, useful for All Users deployment, because general options are stored per user in the HKCU registry hive.

In the section `system.net` you can define proxy settings, e.g. use of NTLM credentials

    <defaultProxy useDefaultCredentials="true">
    </defaultProxy>

In this section you can also allow UnsafeHeaderParsing if the server sends invalid http headers.

    <system.net>
    	<settings>
    		<servicePointManager expect100Continue="false" />
    		<httpWebRequest useUnsafeHeaderParsing="true" />
    	</settings>
    </system.net>

This setting can also be enabled in the general options, starting with version 2.10.0.

In the section `log4net` you can define the log level for the main log (also possible in general options now) and for the caldav data access, 
    level value can be DEBUG or INFO, e.g. :

	<root>
      <level value="DEBUG" />
      <appender-ref ref="MainLogAppender" />
    </root>
    
### Common network errors ###

- System.Net.Http.HttpRequestException: Response status code does not indicate success: '401' ('Unauthorized').
	- Wrong Username and/or Password provided.
- System.Net.Http.HttpRequestException: An error occurred while sending the request. ---> System.Net.WebException: The underlying connection was closed: A connection that was expected to be kept alive was closed by the server.
	- The server has KeepAlive disabled. Use *"Close connection after each request"* in **Network and proxy options**.
- System.Net.Http.HttpRequestException: An error occurred while sending the request. ---> System.Net.WebException: The server committed a protocol violation. Section=ResponseStatusLine
	- The server sends invalid headers. Enable the general option **Enable useUnsafeHeaderParsing** or the commented out option **useUnsafeHeaderparsing** in the app config file, see **Debugging and more config options** above.