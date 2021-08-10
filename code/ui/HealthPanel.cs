using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Discount.UI
{
	public class HealthPanel : Panel
	{
		public Label HealthLabel;
		public Label TeamLabel;

		public HealthPanel()
		{
			HealthLabel = Add.Label( "100", "value" );
			TeamLabel = Add.Label( "", "team-name" );
			Add.Label( "Press F To Change Team", "team-change-hint" );
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

				Color teamColor = Teams.GetLightTeamColor( teamPlayer.Team );

				if ( TeamLabel.Style.FontColor != teamColor )
				{
					TeamLabel.Style.FontColor = teamColor;
					TeamLabel.Style.Dirty();
				}
			}
			else
			{
				TeamLabel.Text = "Neutral";

				if ( TeamLabel.Style.FontColor != Color.White )
				{
					TeamLabel.Style.FontColor = Color.White;
					TeamLabel.Style.Dirty();
				}
			}
		}
	}
}
