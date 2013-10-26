using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FileMonitor
{
	public class FileChangeQueue : IDisposable
	{
		private Action callback;
		private FileInfo file;
		private Action<AggregateException> exceptionCallback;
		private Task worker;
		private object startStopLock;
		private ManualResetEventSlim workloopSignal;
		private ManualResetEventSlim stopWorkSignal;
		private bool isRunning;

		public FileChangeQueue(FileInfo file, Action fileChangedCallback, Action<AggregateException> exceptionCallback)
		{
			this.callback = fileChangedCallback;
			this.file = file;
			this.exceptionCallback = exceptionCallback;
			this.startStopLock = new object();
			this.isRunning = false;
			this.workloopSignal = new ManualResetEventSlim();
			this.stopWorkSignal = new ManualResetEventSlim();
		}

		public void Start()
		{
			lock (this.startStopLock)
			{
				if (this.isRunning) { return; }
				this.isRunning = true;
				this.stopWorkSignal.Reset();
				this.workloopSignal.Reset();
				this.worker = Task.Factory.StartNew(() =>
				{
					Worker();
				}).OnExceptions((aex) =>
				{
					if (this.exceptionCallback != null)
					{
						this.exceptionCallback(aex);
					}
				});
			}
		}

		public void Stop()
		{
			lock (this.startStopLock)
			{
				if (!this.isRunning) { return; }
				this.isRunning = false;
				this.stopWorkSignal.Set();
			}
		}

		public void QueueEvent()
		{
			this.workloopSignal.Set();
		}

		private void Worker()
		{
			while (!this.stopWorkSignal.IsSet)
			{
				this.workloopSignal.Wait();
				this.workloopSignal.Reset();
				if (!this.stopWorkSignal.IsSet)
				{
					NotifyWhenUnlocked();
				}
			}
			this.stopWorkSignal.Reset();
		}

		private void NotifyWhenUnlocked()
		{
			bool result = false;
			do
			{
				this.workloopSignal.Reset();
				result = FileUtil.WaitForUnlock(this.file, 500);
			} while (this.workloopSignal.IsSet);

			if (result && this.callback != null)
			{
				this.callback();
			}
		}

		public void Dispose()
		{
			lock (this.startStopLock)
			{
				if (this.isRunning)
				{
					Stop();
				}
			}
			this.workloopSignal.Dispose();
		}
	}
}
