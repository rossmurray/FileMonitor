using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using NUnit.Framework;

namespace FileMonitor.Tests
{
	[TestFixture]
	public class FileMonitorTests
	{
		private DirectoryInfo currentDir { get { return new DirectoryInfo(Directory.GetCurrentDirectory()); } set { } }
		private FileInfo file {get{ return new FileInfo(Path.Combine(currentDir.FullName, "temp.txt")); } set { } }
		private Random rand = new Random();

		[Test]
		public void CanConstructMonitor()
		{
			var monitor = new FileMonitor(this.file, false);
			Assert.IsNotNull(monitor);
		}

		[Test]
		public void MonitorDetectsChange()
		{
			using (var fs = File.Create(this.file.FullName)) { }
			var monitor = new FileMonitor(this.file);
			var signal = new ManualResetEventSlim();
			monitor.FileChanged += (o, s) => { signal.Set(); };
			Assert.That(signal.IsSet, Is.False);
			ChangeFile();
			signal.Wait(600);
			Assert.That(signal.IsSet, Is.True);
		}

		[Test]
		public void MonitorDoesNotDetectWhenPaused()
		{
			ResetFile();
			var monitor = new FileMonitor(this.file);
			monitor.Pause();
			var signal = new ManualResetEventSlim();
			monitor.FileChanged += (o, s) => { signal.Set(); };
			Assert.That(signal.IsSet, Is.False);
			ChangeFile();
			signal.Wait(600);
			Assert.That(signal.IsSet, Is.False);
		}

		[Test]
		public void MonitorWorksAfterStopAndStart()
		{
			ResetFile();
			var monitor = new FileMonitor(this.file);
			monitor.Pause();
			monitor.Unpause();
			var signal = new ManualResetEventSlim();
			monitor.FileChanged += (o, s) => { signal.Set(); };
			Assert.That(signal.IsSet, Is.False);
			ChangeFile();
			signal.Wait(600);
			Assert.That(signal.IsSet, Is.True);
		}

		[Test]
		public void DisposeDoesNotThrow()
		{
			var monitor = new FileMonitor(this.file.FullName);
			monitor.Dispose();
		}

		private void ResetFile()
		{
			using (var fs = File.Create(this.file.FullName)) { }
		}

		private void ChangeFile()
		{
			var newContents = this.rand.NextDouble().ToString();
			File.WriteAllText(this.file.FullName, newContents);
		}
	}
}
