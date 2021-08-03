using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Discount.UI
{
	public class Health : Panel
	{
		public Label HealthLabel;
		public Label TeamLabel;

		public Health()
		{
			HealthLabel = Add.Label( "100", "value" );
			TeamLabel = Add.Label( "", "team-name" );
			Add.Label( "F To Change", "team-change-hint" );
		}

		public override void Tick()
		{
			var player = Local.Pawn;

			if ( player == null )
			{
				return;
			}

			HealthLabel.Text = $"{player.Health.CeilToInt()}";

			if ( player is TeamPlayer teamPlayer )
			{
				TeamLabel.Text = Teams.GetLongTeamName( teamPlayer.Team );
				TeamLabel.Style.FontColor = Teams.GetLightTeamColor( teamPlayer.Team );
			}
			else
			{
				TeamLabel.Text = "Neutral";
				TeamLabel.Style.FontColor = Color.White;
			}
		}
	}
}
