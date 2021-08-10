using Sandbox;

namespace Discount
{
	[Library( "info_player_teamspawn" )]
	[Hammer.EditorModel( "models/editor/playerstart.vmdl" )]
	[Hammer.EntityTool( "Team Spawnpoint", "Player", "Defines a point where the player belonging to a team can (re)spawn" )]
	public class TeamSpawnPoint : Entity
	{
		[Property( Title = "Team" )]
		public Team Team { get; set; } = Team.Unassigned;
	}
}
