FileMonitor
===========

Watches a file for changes, raising an event when the file is probably done changing.


Often times you want to know when a file changed. [System.IO.FileSystemWatcher](http://msdn.microsoft.com/en-us/library/system.io.filesystemwatcher.aspx) has this ability, but it's not very user-friendly. One of its problems is that its events often fire multiple times for a single logical operation.

FileMonitor filters these events, giving you a single notification when a file changes, with an interface that is painless.

Usage
===========

    var monitor = new FileMonitor(@"C:\some\file.txt");
    monitor.FileChanged += (sender, args) => { /* handle file change */ };
    
