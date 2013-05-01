using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;

namespace FileMonitor
{
	/// <summary>
	/// Watches a file for changes, raising an event when a file has finished changing.
	/// </summary>
	public class FileMonitor : IDisposable
	{
		public event EventHandler<FileChangedEventArgs> FileChanged;
		private FileSystemWatcher watcher;
		private object startStopLock;
		private bool disposed;

		public FileMonitor()
		{
			this.startStopLock = new object();
			this.disposed = false;
			this.watcher = new FileSystemWatcher();
			this.watcher.Changed += new FileSystemEventHandler(FileChangedHandler);
			this.watcher.NotifyFilter = NotifyFilters.LastWrite;
		}

		public void Watch(FileInfo file)
		{
			if (disposed) { throw new ObjectDisposedException(this.GetType().Name); }
			if (file == null) { throw new ArgumentNullException("file"); }
			lock (this.startStopLock)
			{
				this.watcher.EnableRaisingEvents = false;
				this.watcher.Path = file.DirectoryName;
				this.watcher.Filter = file.Name;
				this.watcher.EnableRaisingEvents = true;
			}
		}

		public void StopWatching()
		{
			if (disposed) { throw new ObjectDisposedException(this.GetType().Name); }
			lock (this.startStopLock)
			{
				this.watcher.EnableRaisingEvents = false;
			}
		}

		private void FileChangedHandler(object sender, FileSystemEventArgs e)
		{
			if (disposed) { return; }
			throw new NotImplementedException();
			//throw this event in a queue (another class) whose job is trying to open the file to determine it's done changing.
			//inform event subscribers when the file is available.
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			lock (this.startStopLock)
			{
				this.watcher.Dispose();
			}
			this.disposed = true;
		}
	}
}
