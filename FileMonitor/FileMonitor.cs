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
		/// <summary>
		/// This event fires when the file being watched changes and is probably not in use by another process anymore.
		/// </summary>
		public event EventHandler<FileChangedEventArgs> FileChanged;
		
		/// <summary>
		/// This event fires when there is an exception in the Task that attempts to open the file after it's changed.
		/// </summary>
		/// <remarks>Likely an UnauthorizedAccessException or SecurityException.</remarks>
		public event EventHandler<AggregateExceptionArgs> OnException;

		private FileInfo file;
		private FileSystemWatcher watcher;
		private FileChangeQueue changeQueue;
		private object startStopLock;
		private bool disposed;

		public FileMonitor(string file, bool paused = false) : this(new FileInfo(file), paused) { }

		public FileMonitor(FileInfo file, bool paused = false)
		{
			if (file == null) { throw new ArgumentNullException("file"); }
			this.file = file;
			this.startStopLock = new object();
			this.disposed = false;
			this.watcher = new FileSystemWatcher();
			this.changeQueue = new FileChangeQueue(file, NotifyChanged, NotifyOnException);
			this.watcher.Changed += new FileSystemEventHandler(FileChangedHandler);
			this.watcher.NotifyFilter = NotifyFilters.LastWrite;
			this.watcher.Path = file.DirectoryName;
			this.watcher.Filter = file.Name;
			if (!paused)
			{
				this.watcher.EnableRaisingEvents = true;
				this.changeQueue.Start();
			}
		}

		/// <summary>
		/// Stops any changes to the file from being reported until Unpause is called.
		/// </summary>
		public void Pause()
		{
			if (disposed) { throw new ObjectDisposedException(this.GetType().Name); }
			lock (this.startStopLock)
			{
				this.watcher.EnableRaisingEvents = false;
				this.changeQueue.Stop();
			}
		}

		/// <summary>
		/// Resumes reporting changes after a previous call to Pause.
		/// </summary>
		public void Unpause()
		{
			if (disposed) { throw new ObjectDisposedException(this.GetType().Name); }
			lock (this.startStopLock)
			{
				this.changeQueue.Start();
				this.watcher.EnableRaisingEvents = true;
			}
		}

		private void FileChangedHandler(object sender, FileSystemEventArgs e)
		{
			if (disposed) { return; }
			this.changeQueue.QueueEvent();
		}

		private void NotifyChanged()
		{
			var handler = this.FileChanged;
			if (handler != null)
			{
				handler(this, new FileChangedEventArgs() { File = this.file });
			}
		}

		private void NotifyOnException(AggregateException obj)
		{
			var handler = this.OnException;
			if (handler != null)
			{
				handler(this, new AggregateExceptionArgs() { Exception = obj });
			}
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
			this.changeQueue.Dispose();
			this.disposed = true;
		}
	}
}
