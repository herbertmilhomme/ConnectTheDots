using System.Collections.Generic;

namespace ConnectTheDots.Web
{
	//This was intended to be used for Wave Function Collapse, but requires a library of ALL possible moves for algorithm to check against
	//A make shift solution was to draft math equation that substitutes a similar value returned back pattern...
	/*public partial class Game
	{
		public readonly static IDictionary<string, Tile> ProtoTypes = new Dictionary<string, Tile> 
		//private readonly static IDictionary<Directional, IList<Point>> ProtoTypes = new Dictionary<Directional, IList<Point>>
		{
			{ 
				"blank", 
				new Tile { Directional = new Directional 
					{ 
						UL	= null,		U	= null,		UR	= null,
						L	= null,		M	= null,		R	= null,
						DL	= null,		D	= null,		DR	= null
					}, Neighbors = new Directional
					{
						UL	= false,	U	= false,	UR	= false,
						L	= false,	M	= false,	R	= false,
						DL	= false,	D	= false,	DR	= false
					}
				}
			},
			{ 
				"bul", //border upper left corner
				new Tile { Directional = new Directional 
					{ 
						UL	= null,		U	= null,		UR	= null,
						L	= null,		M	= false,	R	= false,
						DL	= null,		D	= false,	DR	= false
					}, Neighbors = new Directional
					{
						UL	= false,	U	= false,	UR	= false,
						L	= false,	M	= true,		R	= true,
						DL	= false,	D	= true,		DR	= true
					}
				}
			},
			{ 
				"bu", //border upper mid
				new Tile { Directional = new Directional 
					{ 
						UL	= null,		U	= null,		UR	= null,
						L	= false,	M	= false,	R	= false,
						DL	= false,	D	= false,	DR	= false
					}, Neighbors = new Directional
					{
						UL	= false,	U	= false,	UR	= false,
						L	= true,		M	= true,		R	= true,
						DL	= true,		D	= true,		DR	= true
					}
				}
			},
			{ 
				"bur", //border upper right corner
				new Tile { Directional = new Directional 
					{ 
						UL	= null,		U	= null,		UR	= null,
						L	= false,	M	= false,	R	= null,
						DL	= false,	D	= false,	DR	= null
					}, Neighbors = new Directional
					{
						UL	= false,	U	= false,	UR	= false,
						L	= true,		M	= true,		R	= false,
						DL	= true,		D	= true,		DR	= false
					}
				}
			},
			{ 
				"bl", //border left wall
				new Tile { Directional = new Directional 
					{ 
						UL	= null,		U	= false,	UR	= false,
						L	= null,		M	= false,	R	= false,
						DL	= null,		D	= false,	DR	= false
					}, Neighbors = new Directional
					{
						UL	= false,	U	= true,		UR	= true,
						L	= false,	M	= true,		R	= true,
						DL	= false,	D	= true,		DR	= true
					}
				}
			},
			{ 
				"br", //border right wall
				new Tile { Directional = new Directional 
					{ 
						UL	= false,	U	= false,	UR	= null,
						L	= false,	M	= false,	R	= null,
						DL	= false,	D	= false,	DR	= null
					}, Neighbors = new Directional
					{
						UL	= true,		U	= true,		UR	= false,
						L	= true,		M	= true,		R	= false,
						DL	= true,		D	= true,		DR	= false
					}
				}
			},
			{ 
				"bbl", //border bottom left corner
				new Tile { Directional = new Directional 
					{ 
						UL	= null,		U	= false,	UR	= false,
						L	= null,		M	= false,	R	= false,
						DL	= null,		D	= null,		DR	= null
					}, Neighbors = new Directional
					{
						UL	= false,	U	= true,		UR	= true,
						L	= false,	M	= true,		R	= true,
						DL	= false,	D	= false,	DR	= false
					}
				}
			},
			{ 
				"bb", //border bottom mid
				new Tile { Directional = new Directional 
					{ 
						UL	= false,	U	= false,	UR	= false,
						L	= false,	M	= false,	R	= false,
						DL	= null,		D	= null,		DR	= null
					}, Neighbors = new Directional
					{
						UL	= true,		U	= true,		UR	= true,
						L	= true,		M	= true,		R	= true,
						DL	= false,	D	= false,	DR	= false
					}
				}
			},
			{ 
				"bbr", //border bottom right corner
				new Tile { Directional = new Directional 
					{ 
						UL	= false,	U	= false,	UR	= null,
						L	= false,	M	= false,	R	= null,
						DL	= null,		D	= null,		DR	= null
					}, Neighbors = new Directional
					{
						UL	= true,		U	= true,		UR	= false,
						L	= true,		M	= true,		R	= false,
						DL	= false,	D	= false,	DR	= false
					}
				}
			},
			{ 
				"bul_lr0", //selected border upper left corner => left to right, no moves
				new Tile { Directional = new Directional 
					{ 
						UL	= null,		U	= null,		UR	= null,
						L	= null,		M	= true,		R	= true,
						DL	= null,		D	= false,	DR	= false
					}, Neighbors = new Directional
					{
						UL	= false,	U	= false,	UR	= false,
						L	= false,	M	= true,		R	= false,
						DL	= false,	D	= true,		DR	= true
					}
				}
			},
			{ 
				"bul_lr", //selected border upper left corner => left to right
				new Tile { Directional = new Directional 
					{ 
						UL	= null,		U	= null,		UR	= null,
						L	= null,		M	= true,		R	= true,
						DL	= null,		D	= true,		DR	= false
					}, Neighbors = new Directional
					{
						UL	= false,	U	= false,	UR	= false,
						L	= false,	M	= false,	R	= false,
						DL	= false,	D	= false,	DR	= false
					}
				}
			},
			{ 
				"bul_ldr0", //selected border upper left corner => left to diag right, no moves
				new Tile { Directional = new Directional 
					{ 
						UL	= null,		U	= null,		UR	= null,
						L	= null,		M	= true,		R	= false,
						DL	= null,		D	= false,	DR	= true
					}, Neighbors = new Directional
					{
						UL	= false,	U	= false,	UR	= false,
						L	= false,	M	= true,		R	= true,
						DL	= false,	D	= true,		DR	= false
					}
				}
			},
			{ 
				"bul_ldr", //selected border upper left corner => left to diag right
				new Tile { Directional = new Directional 
					{ 
						UL	= null,		U	= null,		UR	= null,
						L	= null,		M	= true,		R	= false,
						DL	= null,		D	= true,		DR	= true
					}, Neighbors = new Directional
					{
						UL	= false,	U	= false,	UR	= false,
						L	= false,	M	= false,	R	= false,
						DL	= false,	D	= false,	DR	= false
					}
				}
			},
			{ 
				"bul_ldrm", //selected border upper left corner => left to diag right mirror
				new Tile { Directional = new Directional 
					{ 
						UL	= null,		U	= null,		UR	= null,
						L	= null,		M	= true,		R	= true,
						DL	= null,		D	= false,	DR	= true
					}, Neighbors = new Directional
					{
						UL	= false,	U	= false,	UR	= false,
						L	= false,	M	= false,	R	= false,
						DL	= false,	D	= false,	DR	= false
					}
				}
			},
			{ 
				"bu_", //selected border upper mid => 
				new Tile { Directional = new Directional 
					{ 
						UL	= null,		U	= null,		UR	= null,
						L	= false,	M	= false,	R	= false,
						DL	= false,	D	= false,	DR	= false
					}, Neighbors = new Directional
					{
						UL	= false,	U	= false,	UR	= false,
						L	= true,		M	= true,		R	= true,
						DL	= true,		D	= true,		DR	= true
					}
				}
			},
			{ 
				"bur", //border upper right corner
				new Tile { Directional = new Directional 
					{ 
						UL	= null,		U	= null,		UR	= null,
						L	= false,	M	= false,	R	= null,
						DL	= false,	D	= false,	DR	= null
					}, Neighbors = new Directional
					{
						UL	= false,	U	= false,	UR	= false,
						L	= true,		M	= true,		R	= false,
						DL	= true,		D	= true,		DR	= false
					}
				}
			},
			{ 
				"bl", //border left wall
				new Tile { Directional = new Directional 
					{ 
						UL	= null,		U	= false,	UR	= false,
						L	= null,		M	= false,	R	= false,
						DL	= null,		D	= false,	DR	= false
					}, Neighbors = new Directional
					{
						UL	= false,	U	= true,		UR	= true,
						L	= false,	M	= true,		R	= true,
						DL	= false,	D	= true,		DR	= true
					}
				}
			},
			{ 
				"br", //border right wall
				new Tile { Directional = new Directional 
					{ 
						UL	= false,	U	= false,	UR	= null,
						L	= false,	M	= false,	R	= null,
						DL	= false,	D	= false,	DR	= null
					}, Neighbors = new Directional
					{
						UL	= true,		U	= true,		UR	= false,
						L	= true,		M	= true,		R	= false,
						DL	= true,		D	= true,		DR	= false
					}
				}
			},
			{ 
				"bbl", //border bottom left corner
				new Tile { Directional = new Directional 
					{ 
						UL	= null,		U	= false,	UR	= false,
						L	= null,		M	= false,	R	= false,
						DL	= null,		D	= null,		DR	= null
					}, Neighbors = new Directional
					{
						UL	= false,	U	= true,		UR	= true,
						L	= false,	M	= true,		R	= true,
						DL	= false,	D	= false,	DR	= false
					}
				}
			},
			{ 
				"bb", //border bottom mid
				new Tile { Directional = new Directional 
					{ 
						UL	= false,	U	= false,	UR	= false,
						L	= false,	M	= false,	R	= false,
						DL	= null,		D	= null,		DR	= null
					}, Neighbors = new Directional
					{
						UL	= true,		U	= true,		UR	= true,
						L	= true,		M	= true,		R	= true,
						DL	= false,	D	= false,	DR	= false
					}
				}
			},
			{ 
				"bbr", //border bottom right corner
				new Tile { Directional = new Directional 
					{ 
						UL	= false,	U	= false,	UR	= null,
						L	= false,	M	= false,	R	= null,
						DL	= null,		D	= null,		DR	= null
					}, Neighbors = new Directional
					{
						UL	= true,		U	= true,		UR	= false,
						L	= true,		M	= true,		R	= false,
						DL	= false,	D	= false,	DR	= false
					}
				}
			},
			{ 
				"start", 
				new Tile { Directional = new Directional 
					{ 
						UL	= false,	U	= false,	UR	= false,
						L	= false,	M	= false,	R	= false,
						DL	= false,	D	= false,	DR	= false
					}, Neighbors = new Directional
					{
						UL	= true,		U	= true,		UR	= true,
						L	= true,		M	= true,		R	= true,
						DL	= true,		D	= true,		DR	= true
					}
				}
			}
		};
	}*/
}