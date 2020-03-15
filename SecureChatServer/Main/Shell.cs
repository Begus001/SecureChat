using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace SecureChatServer.Main
{
	class Test
	{
		public static void Main()
		{
			Shell shell = new Shell();

			shell.ShellCommandIssued += OnShellCommandIssued;
		}

		private static void OnShellCommandIssued(object sender, EventArgs args)
		{
			Console.WriteLine("COMMAND HANDLED!");
		}
	}

	class Shell
	{
		public const string shellPrompt = "SecureChatServer> ";
		public const ushort historySize = 100; // Size of commandHistory buffer

		private string currentInput; // Current command being displayed
		private short historyIndex = -1; // Current index of commandHistory being displayed
		private ushort inputCursorPosition = 0; // Cursor position of currentInput

		private readonly List<string> commandHistory = new List<string>(); // List of previously entered commands
		private readonly ManualResetEvent blockOutput = new ManualResetEvent(true); // Locks output while inputThread is performing output actions

		public delegate void ShellCommandIssuedHandler(object sender, EventArgs args);
		public event ShellCommandIssuedHandler ShellCommandIssued;

		public Shell()
		{
			Thread inputThread = new Thread(Input);
			inputThread.Start();

			RewriteInput();
		}

		protected virtual void OnShellCommandIssued()
		{
			ShellCommandIssued?.Invoke(this, EventArgs.Empty);
		}

		public void Output(string message)
		{
			blockOutput.WaitOne();
			ClearCurrentLine();
			Console.Write(message + "\n" + shellPrompt + currentInput);

			Console.SetCursorPosition(Console.CursorLeft - inputCursorPosition, Console.CursorTop); // Move cursor to inputCursorPosition
		}

		public void Input()
		{
			ConsoleKeyInfo currentKey;
			while (true)
			{
				currentInput = "";

				while (true)
				{
					currentKey = Console.ReadKey(); // Read key input

					blockOutput.Reset();
					if (currentKey.Key == ConsoleKey.Enter) // If currentKey is the enter key, execute command
					{
						string execCommand = "";

						inputCursorPosition = 0;
						RewriteInput();
						Console.WriteLine();

						historyIndex = -1;

						// Write to commandHistory, delete last element if commandHistorySize reached
						if (currentInput.Length > 0)
						{
							if (commandHistory.Count >= historySize)
							{
								commandHistory.RemoveAt(commandHistory.Count - 1);
							}
							commandHistory.Insert(0, currentInput);

							execCommand = currentInput.Trim();
							currentInput = "";
						}

						blockOutput.Set();

						// Execute
						OnShellCommandIssued();

						RewriteInput();

						break;
					}
					else if (currentKey.Key == ConsoleKey.Backspace) // If currentKey is the backspace key, remove char at inputCursorPosition - 1 in currentInput
					{
						if (currentInput.Length > 0 && inputCursorPosition < currentInput.Length)
						{
							currentInput = currentInput.Remove(currentInput.Length - inputCursorPosition - 1, 1);
						}
						else
						{
							Console.SetCursorPosition(Console.CursorLeft + 1, Console.CursorTop);
						}
					}
					else if (currentKey.Key == ConsoleKey.Delete && inputCursorPosition > 0) // If currentKey is the delete key, remove char at inputCursorPosition in currentInput and move cursor to the right
					{
						currentInput = currentInput.Remove(currentInput.Length - inputCursorPosition, 1);
						inputCursorPosition--;
					}
					else if (currentKey.Key == ConsoleKey.UpArrow) // If currentKey is the up arrow, cycle through commandHistory
					{
						if (historyIndex < commandHistory.Count - 1)
						{
							historyIndex++;
							currentInput = commandHistory[historyIndex];
							inputCursorPosition = 0;
						}
					}
					else if (currentKey.Key == ConsoleKey.DownArrow) // If currentKey is the down arrow, cycle through commandHistory
					{
						if (historyIndex > -1)
						{
							historyIndex--;
							if (historyIndex > -1)
							{
								currentInput = commandHistory[historyIndex];
							}
							else
							{
								currentInput = "";
							}
						}

						inputCursorPosition = 0;
					}
					else if (currentKey.Key == ConsoleKey.LeftArrow) // If currentKey is the left arrow, move cursor to the left
					{
						if (inputCursorPosition < currentInput.Length)
						{
							inputCursorPosition++;
						}
					}
					else if (currentKey.Key == ConsoleKey.RightArrow) // If currentKey is the right arrow, move cursor to the right
					{
						if (inputCursorPosition > 0)
						{
							inputCursorPosition--;
						}
					}
					else if (currentKey.Key == ConsoleKey.Home) // If currentKey is the home key, move cursor to the first position
					{
						inputCursorPosition = (ushort)currentInput.Length;
					}
					else if (currentKey.Key == ConsoleKey.End) // If currentKey is the end key, move cursor to the last position
					{
						inputCursorPosition = 0;
					}
					else
					{
						if (shellPrompt.Length + currentInput.Length < Console.WindowWidth - 5)
						{
							// Check if pressed key is a printable char
							foreach (var current in Enum.GetValues(typeof(SupportedKeys)))
							{
								if (currentKey.Key == (ConsoleKey)current)
								{
									currentInput = currentInput.Insert(currentInput.Length - inputCursorPosition, currentKey.KeyChar.ToString());
								}
							}
						}
					}

					RewriteInput();

					blockOutput.Set();
				}
			}
		}

		// Clears current line and returns cursor to left side
		public void ClearCurrentLine()
		{
			Console.SetCursorPosition(0, Console.CursorTop);

			for (int i = 0; i < Console.WindowWidth - 2; i++)
			{
				Console.Write(" ");
			}

			Console.SetCursorPosition(0, Console.CursorTop);
		}

		// Clears current line, write shellPrompt and currentInput and sets the cursor position
		private void RewriteInput()
		{
			ClearCurrentLine();
			Console.Write(shellPrompt + currentInput);

			Console.SetCursorPosition(Console.CursorLeft - inputCursorPosition, Console.CursorTop); // Move cursor to inputCursorPosition
		}

		// List of supported chars for commands
		private enum SupportedKeys
		{
			A = ConsoleKey.A, B = ConsoleKey.B, C = ConsoleKey.C, D = ConsoleKey.D, E = ConsoleKey.E, F = ConsoleKey.F, G = ConsoleKey.G, H = ConsoleKey.H, I = ConsoleKey.I, J = ConsoleKey.J, K = ConsoleKey.K, L = ConsoleKey.L, M = ConsoleKey.M, N = ConsoleKey.N, O = ConsoleKey.O, P = ConsoleKey.P, Q = ConsoleKey.Q, R = ConsoleKey.R, S = ConsoleKey.S, T = ConsoleKey.T, U = ConsoleKey.U, V = ConsoleKey.V, W = ConsoleKey.W, X = ConsoleKey.X, Y = ConsoleKey.Y, Z = ConsoleKey.Z, D0 = ConsoleKey.D0, D1 = ConsoleKey.D1, D2 = ConsoleKey.D2, D3 = ConsoleKey.D3, D4 = ConsoleKey.D4, D5 = ConsoleKey.D5, D6 = ConsoleKey.D6, D7 = ConsoleKey.D7, D8 = ConsoleKey.D8, D9 = ConsoleKey.D9, NumPad0 = ConsoleKey.NumPad0, NumPad1 = ConsoleKey.NumPad1, NumPad2 = ConsoleKey.NumPad2, NumPad3 = ConsoleKey.NumPad3, NumPad4 = ConsoleKey.NumPad4, NumPad5 = ConsoleKey.NumPad5, NumPad6 = ConsoleKey.NumPad6, NumPad7 = ConsoleKey.NumPad7, NumPad8 = ConsoleKey.NumPad8, NumPad9 = ConsoleKey.NumPad9, Spacebar = ConsoleKey.Spacebar, Period = ConsoleKey.OemPeriod, Comma = ConsoleKey.OemComma, NumComma = ConsoleKey.Decimal, Minus = ConsoleKey.OemMinus, Plus = ConsoleKey.OemPlus
		}
	}
}
