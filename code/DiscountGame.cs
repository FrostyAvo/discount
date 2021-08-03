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
		protected HudEntity<RootPanel> activeHud_;
		protected Teams activeTeams_;

		protected HudEntity<RootPanel> ActiveHud
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

			activeTeams_ = new Teams();
		}

		/// <summary>
		/// A client has joined the server. Make them a pawn to play with
		/// </summary>
		public override void ClientJoined( Client client )
		{
			base.ClientJoined( client );

			if ( ClientIsBot(client) )
			{
				// Make bots automatically join a team since they can't navigate the team selection UI
				if ( IsServer )
				{
					ActiveHud = new MainHud();
				}

				activeTeams_.AutoAssignClient( client );

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
									.Where( (TeamSpawnPoint teamSpawnPoint) => { return teamSpawnPoint.TeamIndex == (int)teamPlayer.Team; } )
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

		public static bool ClientIsBot( Client client )
		{
			// Bot clients have a fake SteamID with the Steam account type set to 4 (AnonGameServer)
			return ( ( client.SteamId >> 52 ) & 0b1111 ) == 4;
		}

		[ServerCmd( "changeteam", Help = "Opens the team selection menu" )]
		public static void ChangeTeamCommand()
		{
			ChangeTeam( ConsoleSystem.Caller );
		}

		public static void ChangeTeam( Client target )
		{
			if ( target == null || Current is not DiscountGame discountGame )
			{
				return;
			}

			// Bots can't navigate the team change menu, don't open it for them
			if ( ClientIsBot(target) )
			{
				return;
			}

			if ( Host.IsServer )
			{
				discountGame.ActiveHud = new TeamSelectionUi();
			}

			discountGame.activeTeams_.AssignClientToTeam( target, Team.Spectator );
		}

		[ServerCmd( "jointeam", Help = "Joins the given team (red, blue, auto or spectator)" )]
		public static void JoinTeamCommand( string teamName )
		{
			Client target = ConsoleSystem.Caller;

			if ( target == null || Current is not DiscountGame discountGame )
			{
				return;
			}

			if ( teamName == "auto" )
			{
				if ( Current.IsServer )
				{
					discountGame.ActiveHud = new MainHud();
				}

				discountGame.activeTeams_.AutoAssignClient( target );
				return;
			}

			Team team;

			switch ( teamName )
			{
				case "spectator":
					team = Team.Spectator;
					break;

				case "red":
					team = Team.Red;
					break;

				case "blue":
					team = Team.Blue;
					break;

				default:
					return;
			}

			if ( Host.IsServer )
			{
				discountGame.ActiveHud = new MainHud();
			}

			discountGame.activeTeams_.AssignClientToTeam( target, team );
		}

		public override void DoPlayerNoclip( Client player )
		{
			// Disable noclip
		}

		public override void DoPlayerDevCam( Client player )
		{
			// Disable devcam
		}
	}
}
