using Microsoft.VisualStudio.TestTools.UnitTesting;
using ConnectTheDots.Web;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ConnectTheDots.TestProject
{
	[TestClass]
	public class UnitTest1
	{
		List<IncomingRequest> Actions = new List<IncomingRequest>
		{
			new IncomingRequest{ body = new Point { x = 0, y = 0 } }, //start
			new IncomingRequest{ body = new Point { x = 2, y = 2 } }, //end
			new IncomingRequest{ body = new Point { x = 2, y = 2 } }, //start
			new IncomingRequest{ body = new Point { x = 2, y = 0 } }, //end
			new IncomingRequest{ body = new Point { x = 2, y = 0 } }, //start
			new IncomingRequest{ body = new Point { x = 0, y = 2 } }, //Creates an X, should throw invalid...
			new IncomingRequest{ body = new Point { x = 0, y = 0 } }, 
			new IncomingRequest{ body = new Point { x = 0, y = 3 } }, //creates a vertical line down 
		};

		[TestMethod]
		public void TestLineSegmentCannotOverlap()
		{
			Game game = new Game();
			game.Initialize();
			OutgoingResponse arg = null;
			foreach (IncomingRequest request in Actions)
			{
				if (request.msg == Game.NODE_CLICKED)
				{
					arg = game.Action(request.body);
					//PayloadToJsonString(arg);
				}
			}
			Assert.AreEqual(Game.INVALID_END_NODE, arg?.msg); //As long as last message is invalid 
		}
	}
}
