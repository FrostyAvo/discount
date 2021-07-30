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
			TeamLabel = Add.Label("", "team-label" );
		}

		public override void Tick()
		{
			var player = Local.Pawn;

			if ( player == null )
			{
				return;
			}

			HealthLabel.Text = $"{player.Health.CeilToInt()}";

			if ( player is ClassPlayer classPlayer )
			{
				if (classPlayer.TeamIndex == 0)
				{
					TeamLabel.Text = "Red Team";
					TeamLabel.Style.FontColor = new Color( 1f, 0.5f, 0.5f );
				}
				else
				{
					TeamLabel.Text = "Blue Team";
					TeamLabel.Style.FontColor = new Color( 0.5f, 0.5f, 1f );
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
