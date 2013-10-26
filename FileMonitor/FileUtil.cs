using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;

namespace FileMonitor
{
	public static class FileUtil
	{
		/// <summary>
		/// Attempts to open a file repeatedly until the timeout is reached.
		/// </summary>
		/// <remarks>This method is not recommended for general use.
		/// Just because this method was able to open the file does not mean the file remains uncontested.
		/// This method simply indicates a probability that a file that was in use has ceased being in use, assuming the file only had one accessor.</remarks>
		/// <returns>Whether or not the file was successfully opened.</returns>
		public static bool WaitForUnlock(FileInfo file, int timeoutMs)
		{
			var begin = DateTime.UtcNow;
			while ((DateTime.UtcNow - begin).TotalMilliseconds < timeoutMs)
			{
				try
				{
					using (file.Open(FileMode.Open, FileAccess.Read, FileShare.None))
					{
						return true;
					}
				}
				catch (IOException)
				{
					//it's feasible to examine HResult codes here and throw on codes other than sharing violations,
					//but there are a lot of different reasons the file would be in used by something else
					//such as locking only a portion of a file, open by another process, etc.
					Thread.Sleep(200);
				}
			}
			return false;
		}
	}
}
