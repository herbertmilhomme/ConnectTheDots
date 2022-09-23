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
			try
			{
				if (obj.msg == Game.NODE_CLICKED)
				{
					OutgoingResponse arg = game.Action(obj.body);
					Clients.Caller.broadcastMessage(PayloadToJsonString(arg));
				}
			}
			catch (Exception ex)
			{
				Clients.All.testMessage("ExceptionError", ex.ToString());
			}
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
		public static bool operator ==(Point a, Point b)
		{
			return a.x == b.x && a.y == b.y;
		}

		public static bool operator !=(Point x, Point y)
		{
			return !(x == y);
		}
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
		public Point Position { get; private set; }
		/// <summary>
		/// (*)=> 
		/// Node can still connect
		/// </summary>
		public bool? Start;
		/// <summary>
		/// =>(*)
		/// Node has already connected to
		/// </summary>
		public bool End;

		public Node(Point point)
		{
			Position = point;
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
	/// The best algorithm to use for the game, would be to run a "Wave function collapse"
	/// But i dont have enough time, to put together the resources to resolve the logic sequence
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
		public List<KeyValuePair<int,Line>> Turns;
		public IDictionary<Point, Node> Points;
		public bool IsPlayerOneTurn;
		private Point? startNode;
		public string Player { get { return IsPlayerOneTurn ? "Player 1" : "Player 2"; } }
		public void Initialize()
		{
			//Clear values for a new game
			startNode = null;
			IsPlayerOneTurn = true;
			Turns = new List<KeyValuePair<int, Line>>();
			Points = new Dictionary<Point, Node>();
			for (int y = 0; y < 4; y++)
				for (int x = 0; x < 4; x++)
						Points.Add(new Point { x = x, y = y }, new Node(new Point { x = x, y = y }));
						
		}
		public OutgoingResponse Action(Point point)
		{
			int count = 0;
			int turn = Turns.Count;
			OutgoingResponse response = new OutgoingResponse();
			if (turn == 0)
			{
				//First Move of the Game
				if(startNode == null)
				{
					startNode = point;
					response.msg = VALID_START_NODE;
					response.body.heading = Player;
					response.body.message = "Select a second node to complete the line.";
					return response;
				}
				//need to confirm if end node forms a straight line, i.e. a slope of 0 or a 1:1 between x and y
				if(IsSlopeSymmetrical(startNode.Value, point))
				{
					Line line = new Line { start = startNode.Value, end = point };
					AddPointsInLine(line);
					//The first line both ends are able to branch from...
					Points[startNode.Value].Start = true;
					startNode = null;
					IsPlayerOneTurn = false;
					response.msg = VALID_END_NODE;
					response.body.newLine = line;
					response.body.heading = Player;
					response.body.message = null;
					Turns.Add(new KeyValuePair<int, Line>(turn + 1, line));
					return response;
				}
				startNode = null;
				response.msg = INVALID_END_NODE;
				//response.body.newLine = line;
				response.body.heading = Player;
				response.body.message = "Invalid move!";
				return response;
			}
			//Every move after must connect from the first...
			if(startNode == null)
			{
				//This code block works, it's commented out for performance reasoning
				foreach(KeyValuePair<int,Line> l in Turns)
				{
					//Check if the next start point is on either end of point
					if(l.Value.start == point || l.Value.end == point)
					{
						count++;
					}
				}
				if(count == 0 || count > 1) //0 means it doesnt connect, 2 means it's already active
				{
					response.msg = INVALID_START_NODE;
					//response.body.newLine = line;
					response.body.heading = Player;
					response.body.message = "Not a valid starting position.";
					return response;
				}
				startNode = point;
				response.msg = VALID_START_NODE;
				response.body.heading = Player;
				response.body.message = "Select a second node to complete the line.";
				return response;
			}
			//need to confirm if end node forms a straight line, i.e. a slope of 0 or a 1:1 between x and y
			if(IsSlopeSymmetrical(startNode.Value, point)) //if it's a straight line and doesnt overlap
			{
				if (Points[point].End || Points[point].Start == true)
				{
					startNode = null;
					response.msg = INVALID_END_NODE;
					//response.body.newLine = line;
					response.body.heading = Player;
					response.body.message = "Invalid move! Cannot be connected to existing line";
					return response;
				}
				Line line = new Line { start = startNode.Value, end = point };
				foreach(KeyValuePair<int,Line> l in Turns)
				{
					if (LineContainsActivePoint(line)) //Works better as intersect checker than preventing connected lines
					{
						startNode = null;
						response.msg = INVALID_END_NODE;
						//response.body.newLine = line;
						response.body.heading = Player;
						response.body.message = "Invalid move! Intersection.";
						return response;
					}
				}
				//foreach(KeyValuePair<Point,Node> pair in Points)
				//{
				//	//Go through each available node, and try to connect a line between available and edge node
				//	//if (!Points[pair.Key].End || Points[point].Start == null) //Nodes left to connect to
				//	Line l = new Line { start = pair.Key, end = point };
				//	//Check if any moves can be played on board...
				//	if(LinesIntersect(line, l)) //Too Sensitive, start nodes triggered bool
				//	{
				//		startNode = null;
				//		response.msg = INVALID_END_NODE;
				//		//response.body.newLine = line;
				//		response.body.heading = Player;
				//		response.body.message = "Invalid move! Intersection. ";
				//		return response;
				//	}
				//}
				//if(LineContainsActivePoint(line)) //Works better as intersect checker than preventing connected lines
				//{
				//	startNode = null;
				//	response.msg = INVALID_END_NODE;
				//	//response.body.newLine = line;
				//	response.body.heading = Player;
				//	response.body.message = "Invalid move! Intersection.";
				//	return response;
				//}
				startNode = null;
				IsPlayerOneTurn = false;
				response.msg = VALID_END_NODE;
				response.body.newLine = line;
				response.body.heading = Player;
				response.body.message = null;
				AddPointsInLine(line);
				Turns.Add(new KeyValuePair<int, Line>(turn + 1, line));
				return response;
			}
			startNode = null;
			response.msg = INVALID_END_NODE;
			//response.body.newLine = line;
			response.body.heading = Player;
			response.body.message = "Invalid move!";
			return response;
		}
		Directions CalculateDirection(Line line)
		{
			//Positive is Right, Negative is Left
			int x = line.end.x - line.start.x;
			int y = line.end.y - line.start.y;
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
		private void AddPointsInLine(Line line)
		{
			int x = Math.Abs(line.start.x - line.end.x);
			int y = Math.Abs(line.start.y - line.end.y);
			int i = 0; int n = 0;
			Directions d = CalculateDirection(line);
			switch (d)
			{
				case Directions.UP:
					for (i = line.start.y; y >= 0; y--, i--)
					{
						Points[new Point { x = line.start.x, y = i }].Start = false;
						Points[new Point { x = line.start.x, y = i }].End = true;
					}
					break;
				case Directions.DOWN:
					for (i = line.start.y; y >= 0; y--, i++)
					{
						Points[new Point { x = line.start.x, y = i }].Start = false;
						Points[new Point { x = line.start.x, y = i }].End = true;
					}
					break;
				case Directions.LEFT:
					for (n = line.start.x; x >= 0; x--, n--)
					{
						Points[new Point { x = n, y = line.start.y }].Start = false;
						Points[new Point { x = n, y = line.start.y }].End = true;
					}
					break;
				case Directions.RIGHT:
					for (n = line.start.x; x >= 0; x--, n++)
					{
						Points[new Point { x = n, y = line.start.y }].Start = false;
						Points[new Point { x = n, y = line.start.y }].End = true;
					}
					break;
				case Directions.UPLEFT:
					for (i = line.start.y, n = line.start.x; y >= 0; y--, i--, n--)
					{
						Points[new Point { x = n, y = i }].Start = false;
						Points[new Point { x = n, y = i }].End = true;
					}
					break;
				case Directions.UPRIGHT:
					for (i = line.start.y, n = line.start.x; y >= 0; y--, i--, n++)
					{
						Points[new Point { x = n, y = i }].Start = false;
						Points[new Point { x = n, y = i }].End = true;
					}
					break;
				case Directions.DOWNLEFT:
					for (i = line.start.y, n = line.start.x; y >= 0; y--, i++, n--)
					{
						Points[new Point { x = n, y = i }].Start = false;
						Points[new Point { x = n, y = i }].End = true;
					}
					break;
				case Directions.DOWNRIGHT:
					for (i = line.start.y, n = line.start.x; y >= 0; y--, i++, n++)
					{
						Points[new Point { x = n, y = i }].Start = false;
						Points[new Point { x = n, y = i }].End = true;
					}
					break;
			}
			Points[line.end].Start = true;
		}
		private bool LineContainsActivePoint(Line line)
		{
			//Determine if positive or negative for loop below
			int directionX = line.end.x - line.start.x;
			int directionY = line.end.y - line.start.y;
			//Only works to check if lines intersect by checking node points. 
			//ToDo: If an X is formed between nodes, the loop does not prevent it...
			foreach(KeyValuePair<Point,Node> pair in Points)
			{
				if (pair.Value.Start == null) continue; //If the node is not active in game; skip it
				if (line.start == pair.Key || pair.Value.Start == true) continue; //Exceptions made for start of line
				Point p = pair.Key;
				bool isOnX = false; //is the point horizontal to line?
				bool isOnY = false; //is the point vertical to line?
				if (directionX > 0) //means end is greater than start
				{
					if (line.start.x < p.x && p.x < line.end.x)
						isOnX = true;
				}
				else //(directionX < 0) //means start is greater than end
				{
					if (line.start.x > p.x && p.x > line.end.x)
						isOnX = true;
				}
				if (directionY > 0) //means end is greater than start
				{
					if (line.start.y < p.y && p.y < line.end.y)
						isOnY = true;
				}
				else //(directionY < 0) //means start is greater than end
				{
					if (line.start.y > p.y && p.y > line.end.y)
						isOnY = true;
				}
				if (isOnX || isOnY)
					return true; //it means the point collides with the line
			}
			return false;
		}
		/// <summary>
		/// Returns tru if slope or angle of line is 45 degress
		/// </summary>
		/// <param name="start"></param>
		/// <param name="end"></param>
		/// <returns></returns>
		private static bool IsSlopeSymmetrical(Point start, Point end)
		{
			int x = Math.Abs(start.x - end.x);
			int y = Math.Abs(start.y - end.y);
			if (x == 0 || y == 0 || Math.Abs(x - y) == 0)
				return true;
			else 
				return false;
		}
		/// <summary>
		/// Return true if line segments AB and CD intersect
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		private static bool LinesIntersect(Line a, Line b)
		{
			return doIntersect(a.start, b.start, a.end, b.end);
		}
		//https://www.geeksforgeeks.org/check-if-two-given-line-segments-intersect/
		// The main function that returns true if line segment 'p1q1'
		// and 'p2q2' intersect.
		static bool doIntersect(Point p1, Point q1, Point p2, Point q2)
		{
			// Find the four orientations needed for general and
			// special cases
			int o1 = orientation(p1, q1, p2);
			int o2 = orientation(p1, q1, q2);
			int o3 = orientation(p2, q2, p1);
			int o4 = orientation(p2, q2, q1);

			// General case
			//if (o1 != o2 && o3 != o4)
			//	return true;

			// Special Cases
			// p1, q1 and p2 are collinear and p2 lies on segment p1q1
			if (o1 == 0 && onSegment(p1, p2, q1)) return true;

			// p1, q1 and q2 are collinear and q2 lies on segment p1q1
			if (o2 == 0 && onSegment(p1, q2, q1)) return true;

			// p2, q2 and p1 are collinear and p1 lies on segment p2q2
			if (o3 == 0 && onSegment(p2, p1, q2)) return true;

			// p2, q2 and q1 are collinear and q1 lies on segment p2q2
			if (o4 == 0 && onSegment(p2, q1, q2)) return true;

			return false; // Doesn't fall in any of the above cases
		}

		// To find orientation of ordered triplet (p, q, r).
		// The function returns following values
		// 0 --> p, q and r are collinear
		// 1 --> Clockwise
		// 2 --> Counterclockwise
		static int orientation(Point p, Point q, Point r)
		{
			// See https://www.geeksforgeeks.org/orientation-3-ordered-points/
			// for details of below formula.
			int val = (q.y - p.y) * (r.x - q.x) -
					(q.x - p.x) * (r.y - q.y);

			if (val == 0) return 0; // collinear

			return (val > 0) ? 1 : 2; // clock or counterclock wise
		}
		// Given three collinear points p, q, r, the function checks if
		// point q lies on line segment 'pr'
		static bool onSegment(Point p, Point q, Point r)
		{
			if (q.x < Math.Max(p.x, r.x) && q.x > Math.Min(p.x, r.x) &&
				q.y < Math.Max(p.y, r.y) && q.y > Math.Min(p.y, r.y))
				return true;

			return false;
		}
	}
}