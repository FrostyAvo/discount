using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Discount.UI
{
	public class TeamSelectButton : Button
	{
		protected int teamIndex_;
		protected Color notHoveredColor_;
		protected Color hoveredColor_;

		public TeamSelectButton(int teamIndex)
		{
			teamIndex_ = teamIndex;

			notHoveredColor_ = Teams.GetDarkTeamColor( (Team)teamIndex_ );

			notHoveredColor_.a = 0.9f;

			string buttonLabelText;

			switch ( teamIndex_ )
			{
				case 1:
					buttonLabelText = "Spectate";
					break;

				case 2:
					buttonLabelText = "Join Red Team";
					break;

				case 3:
					buttonLabelText = "Join Blue Team";
					break;

				default:
					buttonLabelText = "Join Random Team";
					notHoveredColor_ = new Color( 0.4f, 0.2f, 0.4f, 0.9f );
					break;
			}

			hoveredColor_ = new Color(
				notHoveredColor_.r + 0.1f,
				notHoveredColor_.g + 0.1f,
				notHoveredColor_.b + 0.1f,
				0.9f );

			Add.Label( buttonLabelText, "label" );
		}

		public override void Tick()
		{
			base.Tick();

			if (HasHovered)
			{
				Style.BackgroundColor = hoveredColor_;
			}
			else
			{
				Style.BackgroundColor = notHoveredColor_;
			}
		}

		protected override void OnClick( MousePanelEvent e )
		{
			base.OnClick( e );

			string teamNameString;

			switch (teamIndex_)
			{
				case 1:
					teamNameString = "spectator";
					break;

				case 2:
					teamNameString = "red";
					break;

				case 3:
					teamNameString = "blue";
					break;

				default:
					teamNameString = "auto";
					break;
			}

			DiscountGame.JoinTeamCommand( teamNameString );
		}
	}
}
