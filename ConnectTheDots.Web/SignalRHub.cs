﻿using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;

namespace ConnectTheDots.Web
{
	public class SignalRHub : Hub
	{
		public const string INITIALIZE			= "INITIALIZE";
		public const string NODE_CLICKED		= "NODE_CLICKED";
		public const string VALID_START_NODE	= "VALID_START_NODE";
		public const string INVALID_START_NODE	= "INVALID_START_NODE";
		public const string VALID_END_NODE		= "VALID_END_NODE";
		public const string INVALID_END_NODE	= "INVALID_END_NODE";
		public const string GAME_OVER			= "GAME_OVER";

		/// <summary>
		/// This is just to test and confirm the real-time connection between client-server is established
		/// </summary>
		public void Send()
		{
			Clients.All.broadcastMessage("HelloWorld", "Greetings!");
		}
		
		public void Send(NodePayload obj)
		{
			if (obj.msg == NODE_CLICKED)
				Clients.Caller.broadcastMessage(PayloadToJsonString(INVALID_START_NODE, "", ""));
			else
				Clients.All.broadcastMessage("HelloWorld", "Greetings!");
		}
		
		public void Initialize()
		{
			Clients.All.broadcastMessage(PayloadToJsonString(INITIALIZE, "Player 1", "Awaiting Player 1's Move"));
		}

		private string PayloadToJsonString(string id, string heading, string message, Line? line = null)
		{
			string json = string.Empty;
			if(id == VALID_END_NODE || id == GAME_OVER)
				json = "{" + string.Format("\"msg\": \"{0}\", \"body\": { \"newLine\": {1}, \"heading\": {2}, \"message\": {3} }", 
					id, LineToJsonString(line.Value), string.Format("\"{0}\"", heading),string.Format("\"{0}\"", message)) + "}";
			else	
				json = "{" + string.Format("\"msg\": \"{0}\", \"body\": { \"newLine\": {1}, \"heading\": {2}, \"message\": {3} }", 
					id, "null", string.Format("\"{0}\"", heading),string.Format("\"{0}\"", message)) + "}";

			return json;
		}

		private string LineToJsonString(Line line)
		{
			return "{" + string.Format("\"start\": { \"x\": {0}, \"y\": {1} }, \"end\": { \"x\": {2}, \"y\": {3} } }", 
					line.start.x,line.end.y,line.end.x,line.end.y) + "}";
		}
	}

	/// <summary>
	/// Represents a 2d x/y axis value on a plane
	/// </summary>
	public struct Point
	{
		public int x { get; set; }
		public int y { get; set; }
	}
	/// <summary>
	/// Represents a connection between two 2d x/y axis value on a plane
	/// </summary>
	public struct Line
	{
		public Point start { get; set; }
		public Point end { get; set; }
	}
	/// <summary>
	/// Represents a connection between two 2d x/y axis value on a plane
	/// </summary>
	public struct StateUpdate
	{
		public Point newLine { get; set; }
		public string heading { get; set; }
		public string message { get; set; }
	}
	/// <summary>
	/// Incoming payload about the node the end user is interacting with
	/// </summary>
	public struct NodePayload
	{
		public string msg { get; set; }
		public Point body { get; set; }
	}
}