using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;

namespace ConnectTheDots.Web
{
	public class SignalRHub : Hub
	{
		/// <summary>
		/// This is just to test and confirm the real-time connection between client-server is established
		/// </summary>
		public void Send()
		{
			Clients.All.broadcastMessage("HelloWorld", "Greetings!");
		}
	}
}