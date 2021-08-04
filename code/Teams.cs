﻿using Sandbox;
using Sandbox.UI;
using System.Collections.Generic;

namespace Discount
{
	public enum Team : sbyte { Unassigned = 0, Spectator = 1, Red = 2, Blue = 3 }

	public partial class Teams : Entity
	{
		private static readonly Color[] lightTeamColors_ =
		{
			new Color(1f, 1f, 1f, 1f),
			new Color(1.0f, 0.8f, 0.5f, 1f),
			new Color(1f, 0.5f, 0.5f, 1f),
			new Color(0.5f, 0.7f, 1f, 1f),
		};

		private static readonly Color[] darkTeamColors_ =
		{
			new Color(0.5f, 0.5f, 0.5f, 1f),
			new Color(0.5f, 0.4f, 0.25f, 1f),
			new Color(0.5f, 0.25f, 0.25f, 1f),
			new Color(0.25f, 0.35f, 0.5f, 1f),
		};

		private static readonly string[] longTeamNames_ =
{
			"Unassigned",
			"Spectator",
			"Red Team",
			"Blue Team",
		};

		private static readonly string[] joinedStrings_ =
{
			"is no longer in a team",
			"has started spectating",
			"has joined the red team",
			"has joined the blue team",
		};

		[Net]
		private List<int> TeamPlayerCounts { get; set; }
		[Net]
		private List<Team> PlayerTeams { get; set; }

		public Teams()
		{
			TeamPlayerCounts = new List<int>( new int[4] );
			PlayerTeams = new List<Team>( new Team[64] );

			Transmit = TransmitType.Always;
		}

		public bool AssignClientToTeam( Client client, Team team )
		{
			// Do nothing if client already in the team
			if ( PlayerTeams[client.NetworkIdent - 1] == team )
			{
				return false;
			}

			if ( Host.IsServer )
			{
				client.Pawn?.Delete();
			}

			Team previousTeam = PlayerTeams[client.NetworkIdent - 1];
			TeamPlayerCounts[(int)previousTeam]--;

			client.Pawn = team == Team.Spectator ? new SpectatorPlayer() { Team = team } : new ClassPlayer( "engineer" ) { Team = team };

			PlayerTeams[client.NetworkIdent - 1] = team;
			TeamPlayerCounts[(int)team]++;

			if ( Host.IsServer )
			{
				(client.Pawn as Player).Respawn();

				Log.Info( $"\"{ client.Name }\" { joinedStrings_[(int)team] }" );
				ChatBox.AddInformation( To.Everyone, $"{ client.Name } { joinedStrings_[(int)team] }", $"avatar:{ client.SteamId }" );
			}

			return true;
		}

		public bool AutoAssignClient( Client client )
		{
			int redPlayerCount = TeamPlayerCounts[(int)Team.Red];
			int bluePlayerCount = TeamPlayerCounts[(int)Team.Blue];

			// Assign to the team with the least players
			if ( redPlayerCount > bluePlayerCount )
			{
				return AssignClientToTeam( client, Team.Blue );
			}
			else if ( redPlayerCount < bluePlayerCount )
			{
				return AssignClientToTeam( client, Team.Red );
			}

			// If equal player counts, assign randomly
			return AssignClientToTeam( client, (Team)Rand.Int( 2, 3 ) );
		}

		public void OnClientConnected( Client client )
		{
			TeamPlayerCounts[(int)Team.Unassigned]++;
		}

		public void OnClientDisconnected( Client client )
		{
			TeamPlayerCounts[(int)PlayerTeams[client.NetworkIdent - 1]]--;

			PlayerTeams[client.NetworkIdent - 1] = Team.Unassigned;
		}

		public static int GetTeamPlayerCount( Team team )
		{
			Teams activeTeams = (Game.Current as DiscountGame)?.Teams;

			if ( activeTeams == null
				|| team < 0
				|| (int)team >= activeTeams.TeamPlayerCounts.Count )
			{
				return -1;
			}

			return activeTeams.TeamPlayerCounts[(int)team];
		}

		public static Color GetLightTeamColor( Team team )
		{
			if (team < 0 || (int)team > lightTeamColors_.Length)
			{
				return lightTeamColors_[0];
			}

			return lightTeamColors_[(int)team];
		}

		public static Color GetDarkTeamColor( Team team )
		{
			if ( team < 0 || (int)team > darkTeamColors_.Length )
			{
				return darkTeamColors_[0];
			}

			return darkTeamColors_[(int)team];
		}

		public static string GetLongTeamName( Team team )
		{
			if ( team < 0 || (int)team > longTeamNames_.Length )
			{
				return "Unknown";
			}

			return longTeamNames_[(int)team];
		}
	}
}
