using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;

using DuplexShell;

namespace SecureChatServer.Connection
{
	internal class Client
	{
		internal uint ID { set; get; }
		internal string Name { get; set; }
		internal IPAddress IP { set; get; }
		internal NetworkStream Stream { get; }
		internal Shell CurrentShell { get; set; }
		internal bool IsInit { get; set; } = false;

		private readonly TcpClient TCPClient;
		private readonly Thread clientHandler;

		public Client(TcpClient client, Shell shell, uint id)
		{
			Stream = client.GetStream();
			TCPClient = client;
			CurrentShell = shell;
			ID = id;

			CurrentShell.Output("New client connected!");

			clientHandler = new Thread(Init);
			clientHandler.Start();
		}

		private void Init()
		{
			using BinaryReader reader = new BinaryReader(Stream, Encoding.UTF8, true);

			string message;

			bool initSequence = false;

			try
			{
				message = reader.ReadString().Trim();
				if (message == "/beginInit") initSequence = true; // Marks begin of init sequence
			}
			catch(IOException e)
			{
				CurrentShell.Error("Init sequence failed, didn't receive any data! IOException: " + e.Message);

				Dispose();

				return;
			}

			// Read init messages
			while (initSequence)
			{
				try
				{
					message = reader.ReadString().Trim();
				}
				catch(IOException e)
				{
					CurrentShell.Error("Init sequence failed, didn't receive any data during sequence! IOException: " + e.Message);

					Dispose();

					return;
				}

				if (message.Contains("/ip")) // Sets ip
				{
					try
					{
						string address = message.Substring(message.IndexOf(" ") + 1, message.Length - message.IndexOf(" ") - 1);
						IP = IPAddress.Parse(address);

						continue;
					}
					catch (FormatException ex)
					{
						CurrentShell.Error("Invalid IP Address received, retrying! FormatException: " + ex.Message);

						continue;
					}
				}
				else if (message.Contains("/name"))
				{
					Name = message.Substring(message.IndexOf(" ") + 1, message.Length - message.IndexOf(" ") - 1);

					continue;
				}
				else if (message == "/endInit") // Marks end of init sequence and starts client
				{
					if (IP != null && Stream != null && Name != null)
					{
						CurrentShell.Output("Client ID: " + ID);
						CurrentShell.Output("Name: " + Name);
						CurrentShell.Output("IP Address: " + IP.ToString());

						initSequence = false;
						IsInit = true;

						return;
					}
					else
					{
						CurrentShell.Error("Insufficient init information received, retrying!"); // If client didn't send sufficient information

						continue;
					}
				}
			}
		}

		private void HandleClient()
		{

		}

		public void SendMessage(string message)
		{
			byte[] msg = Encoding.UTF8.GetBytes(message);
			Stream.Write(msg, 0, msg.Length);
		}

		public void Dispose()
		{
			Stream.Close();
			Stream.Dispose();

			TCPClient.Close();
			TCPClient.Dispose();

			Main.ChatManager.DisposeClient(this);
		}
	}
}
