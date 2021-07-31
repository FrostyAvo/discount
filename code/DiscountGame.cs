using Discount.UI;

using Sandbox;
using System;
using System.Linq;

namespace Discount
{
	/// <summary>
	/// This is your game class. This is an entity that is created serverside when
	/// the game starts, and is replicated to the client. 
	/// 
	/// You can use this to create things like HUDs and declare which player class
	/// to use for spawned players.
	/// 
	/// Your game needs to be registered (using [Library] here) with the same name 
	/// as your game addon. If it isn't then we won't be able to find it.
	/// </summary>
	[Library( "discount" )]
	public partial class DiscountGame : Game
	{
		// Used to assign players evenly to teams, will be replaced later
		[Net, Predicted]
		public int TotalPlayersJoined { get; set; } = 0;

		public DiscountGame()
		{
			if ( IsServer )
			{
				Log.Info( "Discount Has Created Serverside!" );

				new MainHud();
			}

			if ( IsClient )
			{
				Log.Info( "Discount Has Created Clientside!" );
			}
		}

		/// <summary>
		/// A client has joined the server. Make them a pawn to play with
		/// </summary>
		public override void ClientJoined( Client client )
		{
			base.ClientJoined( client );

			var player = new TeamPlayer() { TeamIndex = TotalPlayersJoined % 2 };
			client.Pawn = player;

			TotalPlayersJoined++;

			player.Respawn();

			/*new ClassPlayer() { TeamIndex = TotalPlayersJoined % 2 }.Respawn();
			TotalPlayersJoined++;
			new ClassPlayer() { TeamIndex = TotalPlayersJoined % 2 }.Respawn();
			TotalPlayersJoined++;
			new ClassPlayer() { TeamIndex = TotalPlayersJoined % 2 }.Respawn();
			TotalPlayersJoined++;
			new ClassPlayer() { TeamIndex = TotalPlayersJoined % 2 }.Respawn();
			TotalPlayersJoined++;*/
		}

		public override void MoveToSpawnpoint( Entity pawn )
		{
			if (pawn is not TeamPlayer teamPlayer)
			{
				base.MoveToSpawnpoint( pawn );

				return;
			}

			TeamSpawnPoint spawnpoint = All
									.OfType<TeamSpawnPoint>()
									.Where( (TeamSpawnPoint teamSpawnPoint) => { return teamSpawnPoint.TeamIndex == teamPlayer.TeamIndex; } )
									.OrderBy( x => Guid.NewGuid() )
									.FirstOrDefault();

			if ( spawnpoint == null )
			{
				Log.Warning( $"Couldn't find team spawn point for {pawn}!" );

				return;
			}

			pawn.Transform = spawnpoint.Transform;
		}
	}
}
