using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Discount.UI
{
	class TeamSelectButton : Button
	{
		protected int teamIndex_;
		protected Color notHoveredColor_;
		protected Color hoveredColor_;

		public TeamSelectButton(int teamIndex)
		{
			teamIndex_ = teamIndex;

			string buttonLabelText = "Join Random Team";

			switch ( teamIndex_ )
			{
				case 1:
					buttonLabelText = "Spectate";
					notHoveredColor_ = new Color(0.4f, 0.4f, 0.2f, 0.9f);
					hoveredColor_ = new Color( 0.5f, 0.5f, 0.3f, 0.9f );
					break;

				case 2:
					buttonLabelText = "Join Red Team";
					notHoveredColor_ = new Color( 0.4f, 0.2f, 0.2f, 0.9f );
					hoveredColor_ = new Color( 0.5f, 0.3f, 0.3f, 0.9f );
					break;

				case 3:
					buttonLabelText = "Join Blue Team";
					notHoveredColor_ = new Color( 0.2f, 0.2f, 0.4f, 0.9f );
					hoveredColor_ = new Color( 0.3f, 0.3f, 0.5f, 0.9f );
					break;

				default:
					notHoveredColor_ = new Color( 0.4f, 0.2f, 0.4f, 0.9f );
					hoveredColor_ = new Color( 0.5f, 0.3f, 0.5f, 0.9f );
					break;
			}

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

			string teamNameString = "auto";

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
					break;
			}

			DiscountGame.JoinTeamCommand( teamNameString );
		}
	}
}
