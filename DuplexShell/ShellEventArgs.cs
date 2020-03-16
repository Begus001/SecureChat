using System;
using System.Collections.Generic;
using System.Text;

namespace DuplexShell
{
	public class ShellEventArgs : EventArgs
	{
		public string ExecCommand { get; }
		public List<string> CommandHistory { get; }

		public ShellEventArgs(string execCommand)
		{
			ExecCommand = execCommand;
		}

		public ShellEventArgs(string execCommand, List<string> commandHistory)
		{
			ExecCommand = execCommand;
			CommandHistory = commandHistory;
		}

		public ShellEventArgs(List<string> commandHistory)
		{
			CommandHistory = commandHistory;
		}
	}
}
