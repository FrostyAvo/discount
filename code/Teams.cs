using Sandbox;

namespace Discount
{
	public enum Team { Unassigned = 0, Spectator = 1, Red = 2, Blue = 3 }

	public class Teams
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

		public Teams()
		{

		}

		public void AssignClientToTeam( Client client, Team team )
		{
			if ( Game.Current.IsServer )
			{
				client.Pawn?.Delete();
			}

			client.Pawn = team == Team.Spectator ? new SpectatorPlayer() { Team = team } : new ClassPlayer() { Team = team };

			if ( Game.Current.IsServer )
			{
				(client.Pawn as Player).Respawn();
			}
		}

		public void AutoAssignClient( Client client )
		{
			AssignClientToTeam( client, (Team)Rand.Int( 2, 3 ) );
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
