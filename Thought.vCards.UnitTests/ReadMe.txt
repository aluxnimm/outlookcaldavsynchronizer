
Unit Tests for the vCard Library
--------------------------------

This project contains unit tests for the vCard library.  It uses the
popular NUnit framework, available from www.nunit.org.  The unit testing
framework from Microsoft is restricted to certain commercial versions of
Visual Studio (specifically excluding the express editions) and therefore
NUnit was selected as a free, open source, and stable framework.

- You may need to download NUnit.
- You may need to correct the NUnit assembly reference in this project.


==========================================================================
Installation
==========================================================================

1. Visit www.nunit.org and download NUnit 2.5.2 or later.
   
   NUnit is distributed in several different ways.  Download the MSI file
   if you want a standard Windows installer that will copy NUnit to the
   GAC.  This is the recommended option for most users.
   
   However, feel free to download the binaries (as a ZIP file) or even
   the source code if you are not comfortable running an installer that
   you downloaded from the Internet.  You may copy the NUnit assemblies
   to any suitable folder on your computer.
   
2. Install (or copy the DLL files to a suitable location on your computer).

3. Correct the references in this project.  This project needs a reference
   to the nunit.framework.dll assembly.  This will be located in the
   Global Assembly Cache (GAC) if you ran the MSI installer.  Otherwise
   the DLL will be located wherever you put it!
   
4. Rebuild the solution to make sure everything compiles OK.  


==========================================================================
Using NUnit
==========================================================================

The project folder contains a file called vCards.nunit.  This is an
NUnit project file that references the vCard unit tests.  Double-click
the file (if you installed NUnit) to run the tests.

However, it may be preferable to start the tests from Visual Studio.
This allows you to use the integrated debugger (e.g. to set breakpoints)
while NUnit is running.

1. Right-click the properties of this project.

2. Select the Debug tab.

3. Select "Start external program" in the Start Action section.

4. Browse to the NUnit executable.  Example:
   
     C:\Program Files\NUnit 2.5.2\bin\nunit.exe
   
   The MSI installer has a default installation location of
   C:\Program Files\NUnit <version>\bin.

5. On the same tab (Debug), enter "..\..\vCards.nunit" as command line
   arguments.  This tells the NUnit executable to load the
   project file "vCards.nunit" two folders above the current one.
   Note that Visual Studio will start NUnit from the same folder as
   the DLL, which is something like "debug\bin" or "release\bin".
   
6. Start the project.  Normally you cannot "start" a class library (DLL)
   project, but in this case Visual Studio allows it because you are
   starting an external program.  The integrated debugger still works
   even though an external program is running the DLL.
   
   Email if you have any problems.  It can sometimes be pesky getting
   this to work -- have patience and experiment with startup options.
   There are other ways of getting it to work as well.
   