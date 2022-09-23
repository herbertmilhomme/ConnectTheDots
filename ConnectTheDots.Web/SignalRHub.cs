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
		private static Game game = new Game();

		/// <summary>
		/// This is just to test and confirm the real-time connection between client-server is established
		/// </summary>
		public void Send()
		{
			Clients.All.testMessage("HelloWorld", "Greetings!");
		}
		
		public void Send(IncomingRequest obj)
		{
			if (obj.msg == Game.NODE_CLICKED)
			{
				OutgoingResponse arg = game.Action(obj.body);
				Clients.Caller.broadcastMessage(PayloadToJsonString(arg));
			}
			else
				Clients.All.testMessage("HelloWorld", "Greetings!");
		}
		
		public void Initialize()
		{
			game.Initialize();
			Clients.All.broadcastMessage(PayloadToJsonString(Game.INITIALIZE, "Player 1", "Awaiting Player 1's Move"));
		}

		private string PayloadToJsonString(string id, string heading, string message, Line? line = null)
		{
			string msg = string.IsNullOrEmpty(message) ? "null" : string.Format("\"{0}\"", message);
			string head = string.IsNullOrEmpty(heading) ? "null" : string.Format("\"{0}\"", heading);
			string json = "{" + string.Format("\"msg\": \"{0}\", \"body\":", id) +
				"{" +
					string.Format("\"newLine\": {0}, \"heading\": {1}, \"message\": {2}",
						"null", head, msg) + 
				"}}";
			if (id == Game.VALID_END_NODE || id == Game.GAME_OVER)
				json = "{" + string.Format("\"msg\": \"{0}\", \"body\":", id) +
					"{" +
						string.Format("\"newLine\": {0}, \"heading\": {1}, \"message\": {2}",
							LineToJsonString(line.Value), head, msg) + "}}";

			return json;
		}

		private string PayloadToJsonString(OutgoingResponse payload)
		{
			StateUpdate state = payload.body;
			return PayloadToJsonString(payload.msg, state.heading, state.message, state.newLine);
		}

		private string PayloadToJsonString(string id, StateUpdate state)
		{
			return PayloadToJsonString(id, state.heading, state.message, state.newLine);
		}

		private string LineToJsonString(Line line)
		{
			return "{\"start\":{" + string.Format("\"x\": {0}, \"y\": {1}", line.start.x, line.start.y) + "}," +
				"\"end\":{" + string.Format("\"x\": {0}, \"y\": {1}", line.end.x, line.end.y) + "}}";
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
	public class StateUpdate
	{
		public Line? newLine { get; set; }
		public string heading { get; set; }
		public string message { get; set; }
	}
	/// <summary>
	/// Incoming payload about the node the end user is interacting with
	/// </summary>
	public struct IncomingRequest
	{
		public string msg { get; set; }
		public Point body { get; set; }
	}
	/// <summary>
	/// Outgoing payload about the node the end user is interacting with
	/// </summary>
	public class OutgoingResponse
	{
		public string msg { get; set; }
		public StateUpdate body { get; set; }
		public OutgoingResponse() { body = new StateUpdate(); }
	}
	/// <summary>
	/// This is the dots on game board
	/// </summary>
	public class Node
	{
		//private Game game;
		public string pointIndex = string.Empty;
		public Point Position { get; private set; }

		public Node(Point point)
		{
			Position = point;
		}

		public Node(Point point, string index)
		{
			Position = point;
			pointIndex = index;
		}
	}
	public struct Tile
	{
		public Directional Directional;
		//public NeighborList Neighbors;
		public Directional Neighbors;
	}
	public struct Directional {
		//True if the node is active
		//(active represents the node can be selected to form a line between two points)
		//Null represents the game boundary
		public bool? U;		//up
		public bool? UL;	//upleft
		public bool? UR;	//upright
		public bool? D;		//down
		public bool? DL;	//downleft
		public bool? DR;	//downright
		public bool? L;		//left
		public bool? R;		//right
		public bool? M;		//mid
	}
	public enum Directions
	{
		UP,
		DOWN,
		LEFT,
		RIGHT,
		UPLEFT,
		UPRIGHT,
		DOWNLEFT,
		DOWNRIGHT
	}
	/// <summary>
	/// Each move is numbered. Lines that connect more that two nodes have each segment numbered. 
	/// Player 1 made the odd numbered moves and Player 2 made the even numbered moves. 
	/// Player 1 made the first move (1) and was forced to make the last move (9). 
	/// Thus, Player 2 won.
	/// </summary>
	/// <remarks>
	/// The algorithm for the game runs on "Wave function collapse"
	/// </remarks>
	public partial class Game
	{
		public const string INITIALIZE			= "INITIALIZE";
		public const string NODE_CLICKED		= "NODE_CLICKED";
		public const string VALID_START_NODE	= "VALID_START_NODE";
		public const string INVALID_START_NODE	= "INVALID_START_NODE";
		public const string VALID_END_NODE		= "VALID_END_NODE";
		public const string INVALID_END_NODE	= "INVALID_END_NODE";
		public const string GAME_OVER			= "GAME_OVER";
		/// <summary>
		/// Track move-turns made by each entry to queue
		/// </summary>
		/// Otherwise, a list of KeyValuePairs would be next solution...
		public Queue<Line> Turns;
		public IDictionary<Point, Node> Grid;
		public bool IsPlayerOneTurn;
		private Point? startNode;
		public string Player { get { return IsPlayerOneTurn ? "Player 1" : "Player 2"; } }
		public void Initialize()
		{
			//Clear values for a new game
			startNode = null;
			IsPlayerOneTurn = true;
			Turns = new Queue<Line>();
			Grid = new Dictionary<Point, Node>()
			{
				//first row
				{ new Point { x = 0, y = 0 }, new Node(new Point { x = 0, y = 0 }, "bul") },	//Upper left
				{ new Point { x = 1, y = 0 }, new Node(new Point { x = 1, y = 0 }, "bu") },		//Upper mid
				{ new Point { x = 2, y = 0 }, new Node(new Point { x = 2, y = 0 }, "bu") },		//Upper mid
				{ new Point { x = 3, y = 0 }, new Node(new Point { x = 3, y = 0 }, "bur") }		//Upper right
			};
		}
		public OutgoingResponse Action(Point point)
		{
			OutgoingResponse response = new OutgoingResponse();
			if (Turns.Count == 0)
			{
				//First Move of the Game
				if(startNode == null)
				{
					startNode = point;
					response.msg = VALID_START_NODE;
					response.body.heading = Player;
					response.body.message = null;
					return response;
				}
				Line line = new Line { start = startNode.Value, end = point };
				startNode = null;
				IsPlayerOneTurn = false;
				response.msg = VALID_END_NODE;
				response.body.newLine = line;
				response.body.heading = Player;
				response.body.message = null;
				Turns.Enqueue(line);
				return response;
			}
			return response;
		}
		Directions CalculateDirection(Line line)
		{
			//Positive is Right, Negative is Left
			int x = line.end.x - line.start.x;
			int y = line.end.y - line.start.y;
			//if the neighboring node returns true, it means it can connect
			if (x > 0)
			{
				if (y > 0)
					return Directions.DOWNRIGHT;
				if (y < 0)
					return Directions.UPRIGHT;
				else
					return Directions.RIGHT;
			}
			else if (x < 0)
			{
				if (y > 0)
					return Directions.DOWNLEFT;
				if (y < 0)
					return Directions.UPLEFT;
				else
					return Directions.LEFT;
			}
			else //if (x < 0)
			{
				if (y > 0)
					return Directions.DOWN;
				else //if (y < 0)
					return Directions.UP;
			}
		}
	}
}