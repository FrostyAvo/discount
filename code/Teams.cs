using Sandbox;
using Sandbox.UI;
using System.Collections.Generic;

namespace Discount
{
	public enum Team { Unassigned = 0, Spectator = 1, Red = 2, Blue = 3 }

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
		private List<int> teamPlayerCounts_ { get; set; }
		[Net]
		private List<Team> playerTeams_ { get; set; }

		public Teams()
		{
			teamPlayerCounts_ = new List<int>( new int[4] );
			playerTeams_ = new List<Team>( new Team[64] );

			Transmit = TransmitType.Always;
		}

		public bool AssignClientToTeam( Client client, Team team )
		{
			// Do nothing if client already in the team
			if ( playerTeams_[client.NetworkIdent - 1] == team )
			{
				return false;
			}

			if ( Host.IsServer )
			{
				client.Pawn?.Delete();
			}

			Team previousTeam = playerTeams_[client.NetworkIdent - 1];
			teamPlayerCounts_[(int)previousTeam]--;

			client.Pawn = team == Team.Spectator ? new SpectatorPlayer() { Team = team } : new ClassPlayer() { Team = team };

			playerTeams_[client.NetworkIdent - 1] = team;
			teamPlayerCounts_[(int)team]++;

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
			return AssignClientToTeam( client, (Team)Rand.Int( 2, 3 ) );
		}

		public void OnClientConnected( Client client )
		{
			teamPlayerCounts_[(int)Team.Unassigned]++;
		}

		public void OnClientDisconnected( Client client )
		{
			teamPlayerCounts_[(int)playerTeams_[client.NetworkIdent - 1]]--;

			playerTeams_[client.NetworkIdent - 1] = Team.Unassigned;
		}

		public static int GetTeamPlayerCount( Team team )
		{
			Teams activeTeams = (Game.Current as DiscountGame)?.Teams;

			if ( activeTeams == null
				|| team < 0
				|| (int)team >= activeTeams.teamPlayerCounts_.Count )
			{
				return -1;
			}

			return activeTeams.teamPlayerCounts_[(int)team];
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
