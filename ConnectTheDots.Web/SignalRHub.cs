using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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
			Clients.Caller.testMessage("HelloWorld", "Greetings!");
		}

		public void Refresh()
		{
			Clients.All.refresh();
		}

		public void Send(IncomingRequest obj)
		{
			try
			{
				if (obj.msg == Game.NODE_CLICKED)
				{
					OutgoingResponse arg = game.Action(obj.body);
					Clients.All.broadcastMessage(PayloadToJsonString(arg));
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
			//If a VALID_END_NODE also constitutes the last move in the game,
			//the Server should send GAME_OVER in the msg field instead.
			//All other states should contain NULL in the newLine field.
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

	#region Entity Models
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
		public static bool operator ==(Line a, Line b)
		{
			return (a.start == b.start && a.end == b.end) || (a.start == b.end && a.end == b.start);
		}

		public static bool operator !=(Line x, Line y)
		{
			return !(x == y);
		}
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
	#endregion

	/// <summary>
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
		/// Track move-turns made by each entry to a numbered list
		/// </summary>
		/// Each move is numbered. Lines that connect more that two nodes have each segment numbered.
		public List<KeyValuePair<int,Line>> Turns;
		/// <summary>
		/// A HashSet that stores all the values on game grid, and tracks status Nodes
		/// </summary>
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
			Line line = new Line ();
			OutgoingResponse response = new OutgoingResponse();
			//First move has unique set of rules...
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
					line = new Line { start = startNode.Value, end = point };
					AddPointsInLine(line, turn + 1);
					//The first line both ends are able to branch from...
					Points[startNode.Value].Start = true;
					startNode = null;
					IsPlayerOneTurn = false;
					response.msg = VALID_END_NODE;
					response.body.newLine = line;
					response.body.heading = Player;
					response.body.message = null;
					//Turns.Add(new KeyValuePair<int, Line>(turn + 1, line));
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
			line = new Line { start = startNode.Value, end = point };
			Directions direction = CalculateDirection(line);
			//Does the line traverse 8-directional path to node neighbors?
			if (!IsSlopeSymmetrical(startNode.Value, point))
			{
				startNode = null;
				response.msg = INVALID_END_NODE;
				//response.body.newLine = line;
				response.body.heading = Player;
				response.body.message = "Invalid move!";
				return response;
			}
			//Is the head and tail being connected, at the end of the line? Or connecting to any active node?
			if (Points[point].End || Points[point].Start == true)
			{
				startNode = null;
				response.msg = INVALID_END_NODE;
				//response.body.newLine = line;
				response.body.heading = Player;
				response.body.message = "Invalid move! Cannot be connected to existing line";
				return response;
			}
			//Is the line traversing over a 45 degree line segment?
			if (IsDiagonalOverlap(startNode.Value, direction))
			{
				startNode = null;
				response.msg = INVALID_END_NODE;
				//response.body.newLine = line;
				response.body.heading = Player;
				response.body.message = "Invalid move! Intersection.";
				return response;
			}
			//Are any of the nodes being used twice or overlapping?
			IList<Node> edgeNodes = new Node[0];
			foreach(KeyValuePair<int,Line> l in Turns)
			{
				if (LineContainsActivePoint(line, startNode.Value, out edgeNodes))
				{
					startNode = null;
					response.msg = INVALID_END_NODE;
					//response.body.newLine = line;
					response.body.heading = Player;
					response.body.message = "Invalid move! Cannot be connected to existing line.";
					return response;
				}
			}
			AddPointsInLine(line, direction, turn + 1); //Everything up to this point has already confirmed a valid move entry
			//Are there anymore moves left in the game?
			//Run a loop to check if game has ended, if there are no more moves available
			//foreach(KeyValuePair<Point,Node> pair in Points)
			edgeNodes = Points.Values.Where(x => x.Start == true).ToArray(); //I could've done this from the very beginning...
			if(edgeNodes.Count == 2)
				foreach (Node n in edgeNodes) //for each head/tail on game grid
				{
					//Go through each available node, and try to connect a line between available and edge node
					//if (!Points[pair.Key].End || Points[point].Start == null) //Nodes left to connect to
					//Line l = new Line { start = n.Position, end = point };
					//if no more moves available
					//if (!IsEdgeAvailable(n)) //ToDo: Game is ending too prematurely...
					//{
					//	startNode = null;
					//	response.msg = GAME_OVER;
					//	response.body.newLine = line;
					//	response.body.heading = "Game Over";
					//	response.body.message = string.Format("{0} Wins!", Player);
					//	return response;
					//}
				}
			//If all the rules have passed, and there are no responses, then create line...
			startNode = null;
			IsPlayerOneTurn = !IsPlayerOneTurn;
			response.msg = VALID_END_NODE;
			response.body.newLine = line;
			response.body.heading = Player;
			response.body.message = null;
			//Turns.Add(new KeyValuePair<int, Line>(turn + 1, line));
			return response;
		}

		#region Private Methods
		private Directions CalculateDirection(Line line)
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
		private bool IsEdgeAvailable(Node node)
		{
			Point origin = node.Position;
			Point point = new Point();
			Line line = new Line();
			//Positive is Right, Negative is Left
			//Need to confirm if the two nodes can form a connecting line between each other (that blocks path)
			//Check if open node and isnt head/tail
			point = new Point { x = origin.x, y = origin.y - 1 };		//UP
			if ((point.x >= 0 && point.x < 4) && (point.y >= 0 && point.y < 4) && !Points[point].End)		//UP
			{
				if(!IsDiagonalOverlap(origin, Directions.UP)) return true;
				line = new Line
				{
					start	= origin,
					end		= point     //UP
				};
			}
			point = new Point { x = origin.x, y = origin.y + 1 };		//DOWN
			if ((point.x >= 0 && point.x < 4) && (point.y >= 0 && point.y < 4) && !Points[point].End)		//DOWN
			{
				if(!IsDiagonalOverlap(origin, Directions.DOWN)) return true;
				line = new Line
				{
					start	= origin,
					end		= point     //DOWN
				};
			}
			point = new Point { x = origin.x - 1, y = origin.y };		//RIGHT
			if ((point.x >= 0 && point.x < 4) && (point.y >= 0 && point.y < 4) && !Points[point].End)		//RIGHT
			{
				if(!IsDiagonalOverlap(origin, Directions.RIGHT)) return true;
				line = new Line
				{
					start	= origin,
					end		= point     //RIGHT
				};
			}
			point = new Point { x = origin.x + 1, y = origin.y };		//LEFT
			if ((point.x >= 0 && point.x < 4) && (point.y >= 0 && point.y < 4) && !Points[point].End)		//LEFT
			{
				if(!IsDiagonalOverlap(origin, Directions.LEFT)) return true;
				line = new Line
				{
					start	= origin,
					end		= point     //LEFT
				};
			}
			point = new Point { x = origin.x + 1, y = origin.y - 1 };	//UPLEFT
			if ((point.x >= 0 && point.x < 4) && (point.y >= 0 && point.y < 4) && !Points[point].End)	//UPLEFT
			{
				if(!IsDiagonalOverlap(origin, Directions.UPLEFT)) return true;
				line = new Line
				{
					start	= origin,
					end		= point //UPLEFT
				};
			}
			point = new Point { x = origin.x - 1, y = origin.y + 1 };	//DOWNRIGHT
			if ((point.x >= 0 && point.x < 4) && (point.y >= 0 && point.y < 4) && !Points[point].End)	//DOWNRIGHT
			{
				if(!IsDiagonalOverlap(origin, Directions.DOWNRIGHT)) return true;
				line = new Line
				{
					start	= origin,
					end		= point //DOWNRIGHT
				};
			}
			point = new Point { x = origin.x - 1, y = origin.y - 1 };	//UPRIGHT
			if ((point.x >= 0 && point.x < 4) && (point.y >= 0 && point.y < 4) && !Points[point].End)	//UPRIGHT
			{
				if(!IsDiagonalOverlap(origin, Directions.UPRIGHT)) return true;
				line = new Line
				{
					start	= origin,
					end		= point //UPRIGHT
				};
			}
			point = new Point { x = origin.x + 1, y = origin.y + 1 };	//DOWNLEFT
			if ((point.x >= 0 && point.x < 4) && (point.y >= 0 && point.y < 4) && !Points[point].End)	//DOWNLEFT
			{
				if(!IsDiagonalOverlap(origin, Directions.DOWNLEFT)) return true;
				line = new Line
				{
					start	= origin,
					end		= point //DOWNLEFT
				};
			}
			return false;
		}
		private bool IsDiagonalOverlap(Point origin, Directions direction)
		{
			//Positive is Right, Negative is Left
			//Need to confirm if the two nodes form a connecting line between each other (that blocks path)
			if (direction == Directions.UPRIGHT)
				return Turns.Any(x => x.Value == new Line
				{
					start	= new Point { x = origin.x, y = origin.y - 1 },	//UP
					end		= new Point { x = origin.x - 1, y = origin.y }	//RIGHT
				});
			else if (direction == Directions.UPLEFT)
				return Turns.Any(x => x.Value == new Line
				{
					start	= new Point { x = origin.x, y = origin.y - 1 },	//UP
					end		= new Point { x = origin.x + 1, y = origin.y }	//LEFT
				});
			else if (direction == Directions.DOWNLEFT)
				return Turns.Any(x => x.Value == new Line
				{
					start	= new Point { x = origin.x, y = origin.y + 1 },	//DOWN
					end		= new Point { x = origin.x + 1, y = origin.y }	//LEFT
				});
			else if (direction == Directions.DOWNRIGHT)
				return Turns.Any(x => x.Value == new Line
				{
					start	= new Point { x = origin.x, y = origin.y + 1 },	//DOWN
					end		= new Point { x = origin.x - 1, y = origin.y }	//RIGHT
				});
			return false;
		}
		private void AddPointsInLine(Line line, int? turn = null)
		{
			Directions d = CalculateDirection(line);
			AddPointsInLine(line, d, turn);
		}
		private void AddPointsInLine(Line line, Directions direction, int? turn = null)
		{
			int x = Math.Abs(line.start.x - line.end.x);
			int y = Math.Abs(line.start.y - line.end.y);
			int i = 0; int n = 0; int z = 0;  //Z > 0 = next node in sequence, and can form a line with previous
			Point start = new Point(); Point end = new Point(); //Lines that connect more that two nodes have each segment numbered.
			switch (direction)
			{
				case Directions.UP:
					for (i = line.start.y; y >= 0; y--, i--, z++)
					{
						end = new Point { x = line.start.x, y = i };
						Points[end].Start = false;
						Points[end].End = true;
						if (turn.HasValue && z > 0)
						{
							start = new Point { x = line.start.x, y = i + 1 };
							Turns.Add(new KeyValuePair<int, Line>(turn.Value, new Line { start = start, end = end }));
						}
					}
					break;
				case Directions.DOWN:
					for (i = line.start.y; y >= 0; y--, i++, z++)
					{
						end = new Point { x = line.start.x, y = i };
						Points[end].Start = false;
						Points[end].End = true;
						if (turn.HasValue && z > 0)
						{
							start = new Point { x = line.start.x, y = i - 1 };
							Turns.Add(new KeyValuePair<int, Line>(turn.Value, new Line { start = start, end = end }));
						}
					}
					break;
				case Directions.LEFT:
					for (n = line.start.x; x >= 0; x--, n--, z++)
					{
						end = new Point { x = n, y = line.start.y };
						Points[end].Start = false;
						Points[end].End = true;
						if (turn.HasValue && z > 0)
						{
							start = new Point { x = n + 1, y = line.start.y };
							Turns.Add(new KeyValuePair<int, Line>(turn.Value, new Line { start = start, end = end }));
						}
					}
					break;
				case Directions.RIGHT:
					for (n = line.start.x; x >= 0; x--, n++, z++)
					{
						end = new Point { x = n, y = line.start.y };
						Points[end].Start = false;
						Points[end].End = true;
						if (turn.HasValue && z > 0)
						{
							start = new Point { x = n - 1, y = line.start.y };
							Turns.Add(new KeyValuePair<int, Line>(turn.Value, new Line { start = start, end = end }));
						}
					}
					break;
				case Directions.UPLEFT:
					for (i = line.start.y, n = line.start.x; y >= 0; y--, i--, n--, z++)
					{
						end = new Point { x = n, y = i };
						Points[end].Start = false;
						Points[end].End = true;
						if (turn.HasValue && z > 0)
						{
							start = new Point { x = n + 1, y = i + 1 };
							Turns.Add(new KeyValuePair<int, Line>(turn.Value, new Line { start = start, end = end }));
						}
					}
					break;
				case Directions.UPRIGHT:
					for (i = line.start.y, n = line.start.x; y >= 0; y--, i--, n++, z++)
					{
						end = new Point { x = n, y = i };
						Points[end].Start = false;
						Points[end].End = true;
						if (turn.HasValue && z > 0)
						{
							start = new Point { x = n - 1, y = i + 1 };
							Turns.Add(new KeyValuePair<int, Line>(turn.Value, new Line { start = start, end = end }));
						}
					}
					break;
				case Directions.DOWNLEFT:
					for (i = line.start.y, n = line.start.x; y >= 0; y--, i++, n--, z++)
					{
						end = new Point { x = n, y = i };
						Points[end].Start = false;
						Points[end].End = true;
						if (turn.HasValue && z > 0)
						{
							start = new Point { x = n + 1, y = i - 1};
							Turns.Add(new KeyValuePair<int, Line>(turn.Value, new Line { start = start, end = end }));
						}
					}
					break;
				case Directions.DOWNRIGHT:
					for (i = line.start.y, n = line.start.x; y >= 0; y--, i++, n++, z++)
					{
						end = new Point { x = n, y = i };
						Points[end].Start = false;
						Points[end].End = true;
						if (turn.HasValue && z > 0)
						{
							start = new Point { x = n - 1, y = i - 1 };
							Turns.Add(new KeyValuePair<int, Line>(turn.Value, new Line { start = start, end = end }));
						}
					}
					break;
			}
			Points[line.end].Start = true;
		}
		private bool LineContainsActivePoint(Line line, Point ignore, out IList<Node> corners)
		{
			bool results = false;
			corners = new List<Node>();
			//Determine if positive or negative for loop below
			int directionX = line.end.x - line.start.x;
			int directionY = line.end.y - line.start.y;
			//Only works to check if lines intersect by checking node points.
			//ToDo: If an X is formed between nodes, the loop does not prevent it...
			foreach(KeyValuePair<Point,Node> pair in Points)
			{
				if (pair.Value.Start == true)
				{
					if(!corners.Contains(pair.Value))
						corners.Add(pair.Value);		//Add Head/Tail to separate list for later...
					continue;							//Exceptions made for start of line
				}
				if (pair.Value.Start == null) continue;	//If the node is not active in game; skip it
				if (ignore == pair.Key) continue;		//Exceptions made for start of line
				Point p = pair.Key;
				bool isOnX = false;		//is the point horizontal to line?
				bool isOnY = false;		//is the point vertical to line?
				if (directionX > 0)		//means end is greater than start
				{
					if (line.start.x <= p.x && p.x <= line.end.x)
						isOnX = true;
				}
				else //(directionX < 0)	//means start is greater than end
				{
					if (line.start.x >= p.x && p.x >= line.end.x)
						isOnX = true;
				}
				if (directionY > 0)		//means end is greater than start
				{
					if (line.start.y <= p.y && p.y <= line.end.y)
						isOnY = true;
				}
				else //(directionY < 0)	//means start is greater than end
				{
					if (line.start.y >= p.y && p.y >= line.end.y)
						isOnY = true;
				}
				//Point C (x3,y3) will lie between A & B if:
				//x3 lies between x1 & x2 and y3 lies between y1 & y2
				if (isOnX && isOnY)
					results = true; //it means the point collides with the line
			}
			return results;
		}
		/// <summary>
		/// Returns true if slope or angle of line is 45 degrees
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
		#endregion
	}
}