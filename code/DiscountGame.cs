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

		[Net]
		public Teams Teams { get; protected set; }

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
			Teams = new Teams();
		}

		/// <summary>
		/// A client has joined the server. Make them a pawn to play with
		/// </summary>
		public override void ClientJoined( Client client )
		{
			base.ClientJoined( client );

			Teams.OnClientConnected( client );

			if ( ClientIsBot(client) )
			{
				// Make bots automatically join a team since they can't navigate the team selection UI
				if ( Teams.AutoAssignClient( client ) && IsServer )
				{
					ActiveHud = new MainHud();

					JoinRandomClass( client );
				}

				return;
			}

			ChangeTeam(client);
		}

		public override void ClientDisconnect( Client cl, NetworkDisconnectionReason reason )
		{
			base.ClientDisconnect( cl, reason );

			Teams.OnClientDisconnected( cl );
		}

		public override void MoveToSpawnpoint( Entity pawn )
		{
			if ( pawn is not TeamPlayer teamPlayer )
			{
				base.MoveToSpawnpoint( pawn );

				return;
			}

			TeamSpawnPoint spawnpoint = All
									.OfType<TeamSpawnPoint>()
									.Where( ( TeamSpawnPoint teamSpawnPoint ) =>
									{
										return teamSpawnPoint.TeamIndex == (int)teamPlayer.Team;
									} )
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

			// Make sure player is a spectator before bringing up the menu
			if ( discountGame.Teams.AssignClientToTeam( target, Team.Spectator )
				|| discountGame.Teams.GetClientTeam( target ) == Team.Spectator )
			{
				if ( Host.IsServer
					&& discountGame.ActiveHud is not TeamSelectionUi )
				{
					discountGame.ActiveHud = new TeamSelectionUi();
				}

				if ( target.Pawn is not SpectatorPlayer )
				{
					if ( Host.IsServer )
					{
						target.Pawn?.Delete();
					}

					target.Pawn = new SpectatorPlayer() { Team = Team.Spectator };

					if ( Host.IsServer )
					{
						(target.Pawn as Player).Respawn();
					}
				}
			}
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
				if ( discountGame.Teams.AutoAssignClient( target ) && Host.IsServer )
				{
					ChangeClass( target );
				}

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

			if ( !discountGame.Teams.AssignClientToTeam( target, team ) )
			{
				return;
			}

			if ( team == Team.Spectator )
			{
				if ( Host.IsServer )
				{
					discountGame.ActiveHud = new MainHud();
				}

				if ( target.Pawn is not SpectatorPlayer )
				{
					if ( Host.IsServer )
					{
						target.Pawn?.Delete();
					}

					target.Pawn = new SpectatorPlayer() { Team = Team.Spectator };

					if ( Host.IsServer )
					{
						(target.Pawn as Player).Respawn();
					}
				}
			}
			else
			{
				ChangeClass( target );
			}
		}

		[ServerCmd( "changeclass", Help = "Opens the class selection menu" )]
		public static void ChangeClassCommand()
		{
			ChangeClass( ConsoleSystem.Caller );
		}

		public static void ChangeClass( Client target )
		{
			if ( target == null || Current is not DiscountGame discountGame )
			{
				return;
			}

			// Bots can't navigate the class change menu, don't open it for them
			if ( ClientIsBot( target ) )
			{
				return;
			}

			if ( Host.IsServer )
			{
				discountGame.ActiveHud = new ClassSelectionUi();
			}

			if ( target.Pawn is not SpectatorPlayer )
			{
				if ( Host.IsServer )
				{
					target.Pawn?.Delete();
				}

				target.Pawn = new SpectatorPlayer() { Team = Team.Spectator };

				if ( Host.IsServer )
				{
					(target.Pawn as Player).Respawn();
				}
			}
		}

		[ServerCmd( "joinclass", Help = "Joins the given class" )]
		public static void JoinClassCommand( string className )
		{
			Client target = ConsoleSystem.Caller;

			if ( className == "random" )
			{
				JoinRandomClass( target );
			}
			else
			{
				JoinClass( target, className );
			}
		}

		protected static void JoinClass( Client target, string className )
		{
			if ( target == null || Current is not DiscountGame discountGame )
			{
				return;
			}

			Team targetTeam = discountGame.Teams.GetClientTeam( target );

			// Don't join class if client is not in a valid team
			if ( targetTeam != Team.Red && targetTeam != Team.Blue )
			{
				return;
			}

			ClassPlayer classPlayer = new ClassPlayer( className );

			// If class data for className could not be loaded
			if ( classPlayer.Data == null )
			{
				return;
			}

			if ( Host.IsServer )
			{
				target.Pawn?.Delete();

				discountGame.ActiveHud = new MainHud();
			}

			classPlayer.Team = targetTeam;
			target.Pawn = classPlayer;

			if ( Host.IsServer )
			{
				(target.Pawn as Player).Respawn();
			}
		}

		protected static void JoinRandomClass( Client target )
		{
			JoinClass( target, ClassData.AllClasses[Rand.Int( 0, ClassData.AllClasses.Length - 1 )] );
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
