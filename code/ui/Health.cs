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
				switch ( teamPlayer.TeamIndex )
				{
					case 0:
						TeamLabel.Text = "Unassigned";
						TeamLabel.Style.FontColor = Color.White;
						break;

					case 1:
						TeamLabel.Text = "Spectator";
						TeamLabel.Style.FontColor = Color.White;
						break;

					case 2:
						TeamLabel.Text = "Red Team";
						TeamLabel.Style.FontColor = new Color( 1f, 0.5f, 0.5f );
						break;

					case 3:
						TeamLabel.Text = "Blue Team";
						TeamLabel.Style.FontColor = new Color( 0.5f, 0.5f, 1f );
						break;

					default:
						break;
				}
			}
			else
			{
				TeamLabel.Text = "Neutral";
				TeamLabel.Style.FontColor = Color.White;
			}
		}
	}
}
