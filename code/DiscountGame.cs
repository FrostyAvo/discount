using Discount.UI;

using Sandbox;
using Sandbox.UI;

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
		protected static HudEntity<RootPanel> activeHud_;

		protected static HudEntity<RootPanel> ActiveHud
		{
			get
			{
				return activeHud_;
			}

			set
			{
				if (activeHud_ != null)
				{
					activeHud_.Delete();
				}

				activeHud_ = value;
			}
		}

		public DiscountGame()
		{
			if ( IsServer )
			{
				ActiveHud = new TeamSelectionUi();
			}
		}

		/// <summary>
		/// A client has joined the server. Make them a pawn to play with
		/// </summary>
		public override void ClientJoined( Client client )
		{
			base.ClientJoined( client );

			// Bot clients have a fake SteamID with the Steam account type set to 4 (AnonGameServer)
			if ( ((client.SteamId >> 52) & 0b1111) == 4 )
			{
				// Make bots automatically join a team since they can't navigate the team selection UI
				Player player = new TeamPlayer() { TeamIndex = Rand.Int( 2, 3 ) };

				client.Pawn = player;

				player.Respawn();

				return;
			}

			ChangeTeam(client);
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

				base.MoveToSpawnpoint( pawn );

				return;
			}

			pawn.Transform = spawnpoint.Transform;
		}

		[ServerCmd( "changeteam", Help = "Opens the team selection menu" )]
		public static void ChangeTeamCommand()
		{
			ChangeTeam(ConsoleSystem.Caller);
		}

		public static void ChangeTeam(Client target)
		{
			if ( target == null )
			{
				return;
			}

			// Bot clients have a fake SteamID with the Steam account type set to 4 (AnonGameServer)
			if ( ((target.SteamId >> 52) & 0b1111) == 4 )
			{
				return;
			}

			if ( Current.IsServer )
			{
				ActiveHud = new TeamSelectionUi();
				target.Pawn?.Delete();
			}

			target.Pawn = new SpectatorPlayer();

			if ( Current.IsServer )
			{
				(target.Pawn as Player).Respawn();
			}
		}

		[ServerCmd( "jointeam", Help = "Joins the given team (red, blue, auto or spectator)" )]
		public static void JoinTeamCommand(string teamName)
		{
			Client target = ConsoleSystem.Caller;

			if ( target == null )
			{
				return;
			}

			int teamIndex;

			switch ( teamName )
			{
				case "red":
					teamIndex = 2;
					break;

				case "blue":
					teamIndex = 3;
					break;

				case "auto":
					teamIndex = Rand.Int(2, 3);
					break;

				case "spectator":
					teamIndex = 1;
					break;

				default:
					return;
			}

			if ( Current.IsServer )
			{
				ActiveHud = new MainHud();
				target.Pawn?.Delete();
			}

			target.Pawn = teamIndex == 1 ? new SpectatorPlayer() : new TeamPlayer() { TeamIndex = teamIndex };

			if ( Current.IsServer )
			{
				(target.Pawn as Player).Respawn();
			}
		}
	}
}
