using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace FileMonitor
{
	public class FileChangedEventArgs : EventArgs
	{
		public FileInfo File { get; set; }
	}
}
