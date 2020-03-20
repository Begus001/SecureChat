using System;
using System.Threading;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;

using DuplexShell;
using SecureChatServer.Connection;

namespace SecureChatServer.Main
{
	internal static class ChatManager
	{
		internal static Shell MainShell { get; } = new Shell(1000, "SecureChatServer> ");
		internal static ushort Port { set; get; } = 18000;
		internal static List<Client> Clients { get; } = new List<Client>();
		internal static ushort ErrorRetryDelay { get; set; } = 1000;
		internal static bool Running { get; set; } = true;

		public static void Main()
		{
			TCPListener listener = new TCPListener(Port, MainShell);

			listener.ClientConnected += InitClient;
		}

		// Receive client init information and add client to list
		private static void InitClient(object sender, TCPListenerArgs e)
		{
			uint id = (uint)Clients.Count;

			Clients.Add(new Client(e.Client, MainShell, id));

		}

		public static void DisposeClient(uint id)
		{
			Clients.RemoveAt((int)id);
		}

		public static void DisposeClient(Client client)
		{
			Clients.Remove(client);
		}
	}
}
