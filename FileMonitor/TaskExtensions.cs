using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileMonitor
{
	public static class TaskExtensions
	{
		/// <summary>
		/// Observes any exceptions thrown by a task and performs an Action on them. (ie: logging). This does not occur on task cancellation. Recommended that you iterate over AggregateException.InnerExceptions in this.
		/// </summary>
		/// <param name="task"></param>
		/// <param name="action"></param>
		/// <returns></returns>
		public static Task OnExceptions(this Task task, Action<AggregateException> action)
		{
			task.ContinueWith(t =>
			{
				action(t.Exception.Flatten());
			}, TaskContinuationOptions.OnlyOnFaulted);
			return task;
		}
	}
}
