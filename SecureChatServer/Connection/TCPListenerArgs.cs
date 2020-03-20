using System;
using System.Net.Sockets;

namespace SecureChatServer.Main
{
	public class TCPListenerArgs : EventArgs
	{
		public TcpClient Client { get; }

		public TCPListenerArgs(TcpClient newClient)
		{
			Client = newClient;
		}
	}
}
