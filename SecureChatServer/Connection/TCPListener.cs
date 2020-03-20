using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using DuplexShell;

namespace SecureChatServer.Main
{
	internal class TCPListener
	{
		internal ushort AcceptClientDelay { get; set; } = 10;
		internal ushort ListenerRetryDelay { get; set; } = 1000;
		internal Shell CurrentShell { get; set; }
		internal ushort Port { get; set; }

		private readonly TcpListener listener = null;


		private TcpClient currentClient = null;

		public event EventHandler<TCPListenerArgs> ClientConnected;

		public TCPListener(ushort port, Shell output)
		{
			Port = port;
			CurrentShell = output;

			try
			{
				CurrentShell.Output("Starting TCP listener...");

				listener = new TcpListener(IPAddress.Any, port);
				listener.Start();


				Thread acceptor = new Thread(AcceptClients);
				acceptor.Start();

				CurrentShell.Output("TCP listener started!");
			}
			catch(SocketException e)
			{
				CurrentShell.Error("Couldn't start listener! SocketException: " + e.Message);
				return;
			}
			catch (ThreadStateException e)
			{
				CurrentShell.Error("Couldn't start listener thread! ThreadStateException: " + e.Message);
				return;
			}
			catch(OutOfMemoryException e)
			{
				CurrentShell.Error("Couldn't start listener thread! OutOfMemoryException: " + e.Message);
				return;
			}
		}

		protected virtual void OnClientConnected(TcpClient client)
		{
			ClientConnected?.Invoke(this, new TCPListenerArgs(client));
		}

		internal void AcceptClients()
		{
			CurrentShell.Output("Listening for new clients on port " + Port + "...");
			while (true)
			{
				try
				{
					currentClient = listener.AcceptTcpClient();

					OnClientConnected(currentClient);

					Thread.Sleep(AcceptClientDelay);

					currentClient = null;
				}
				catch (SocketException e)
				{
					CurrentShell.Error("Couldn't listen for new clients, retrying! SocketException: " + e.Message);
					Thread.Sleep(ListenerRetryDelay);
					continue;
				}
			}
		}
	}
}
